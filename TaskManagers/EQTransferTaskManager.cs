using AGVSystem.Models.Map;
using AGVSystem.Models.Sys;
using AGVSystemCommonNet6.AGVDispatch.Messages;
using AGVSystemCommonNet6.Alarm;
using AGVSystemCommonNet6.DATABASE;

using AGVSystemCommonNet6;
using static AGVSystemCommonNet6.clsEnums;
using AGVSystemCommonNet6.Log;
using AGVSystemCommonNet6.MAP;
using AGVSystemCommonNet6.DATABASE.Helpers;
using AGVSystemCommonNet6.AGVDispatch.RunMode;
using EquipmentManagment.MainEquipment;
using EquipmentManagment.Device;
using EquipmentManagment.Manager;
using AGVSystemCommonNet6.AGVDispatch;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.CodeAnalysis.Emit;
using AGVSystemCommonNet6.Microservices.VMS;
using EquipmentManagment.Device.Options;
using static AGVSystemCommonNet6.MAP.MapPoint;
using EquipmentManagment.WIP;
using static AGVSystemCommonNet6.GPMRosMessageNet.Services.EquipmentStateRequest;
using NuGet.Protocol;

namespace AGVSystem.TaskManagers
{
    public class EQTransferTaskManager
    {
        public static bool AutoRunning { get; private set; } = false;
        public static void Initialize()
        {
            //clsEQ.OnEqUnloadRequesting += ClsEQ_OnEqUnloadRequesting;
            SystemModes.OnRunModeON += SwitchToRunMode;
            SystemModes.OnRunModeOFF += SwitchToMaintainMode;
        }
        public static Dictionary<clsEQ, clsEQ> UnloadEQQueueing { get; private set; } = new Dictionary<clsEQ, clsEQ>();
        public static Dictionary<string, clsLocalAutoTransferTaskMonitor> MonitoringCarrerTasks = new Dictionary<string, clsLocalAutoTransferTaskMonitor>();
        internal static async void SwitchToMaintainMode()
        {
            while (AutoRunning)
            {
                await Task.Delay(1);
            }
            LOG.WARN("Maintain Mode Start");
            //取消預約所有機台
            StaEQPManagager.MainEQList.FindAll(eq => eq.CMD_Reserve_Low | eq.CMD_Reserve_Up).ForEach(eq =>
            {
                eq.CancelReserve();
            });
            UnloadEQQueueing.Clear();
        }

        internal static async void SwitchToRunMode()
        {
            LOG.WARN("Run Mode Start");

            _ = Task.Run(() => { TransferTaskPairWorker(); });
            //foreach (var eq in unloadReqEQs)
            //{
            //    ClsEQ_OnEqUnloadRequesting("Run Mode On", (eq as clsEQ));
            //    await Task.Delay(1);
            //}
        }

        static async void TransferTaskPairWorker()
        {
            while (SystemModes.RunMode == RUN_MODE.RUN)
            {
                await Task.Delay(10);
                if (SystemModes.TransferTaskMode == TRANSFER_MODE.MANUAL)
                {
                    AutoRunning = false;
                    continue;
                }
                AutoRunning = true;
                List<clsEQ> unload_req_eq_list = StaEQPManagager.MainEQList.FindAll(eq => eq.lduld_type == EQLDULD_TYPE.ULD | eq.lduld_type == EQLDULD_TYPE.LDULD)
                                      .FindAll(eq => eq.Unload_Request && eq.Eqp_Status_Down && !eq.CMD_Reserve_Low && !UnloadEQQueueing.TryGetValue(eq, out clsEQ _eq));

                if (unload_req_eq_list.Count > 0)
                {
                    foreach (clsEQ sourceEQ in unload_req_eq_list)
                    {
                        List<clsEQ> loadable_eqs = sourceEQ.DownstremEQ.FindAll(downstrem_eq => downstrem_eq.Load_Request && downstrem_eq.Eqp_Status_Down && !downstrem_eq.CMD_Reserve_Low);


                        using (var db = new AGVSDatabase())
                        {
                            var TaskToStationId = db.tables.Tasks.AsNoTracking().Where(task => task.To_Station != "-1" && task.State == TASK_RUN_STATUS.WAIT | task.State == TASK_RUN_STATUS.NAVIGATING).Select(task => task.To_Station).ToList();
                            loadable_eqs = loadable_eqs.Where(item => !TaskToStationId.Contains(item.EndPointOptions.TagID.ToString())).ToList();
                        }
                        if (loadable_eqs.Count == 0)
                            continue;
                        clsEQ destineEQ = loadable_eqs.First();
                        UnloadEQQueueing.Add(sourceEQ, sourceEQ); ;
                        var taskOrder = new clsTaskDto
                        {
                            Action = ACTION_TYPE.Carry,
                            DesignatedAGVName = "",
                            From_Station = sourceEQ.EndPointOptions.TagID.ToString(),
                            To_Station = destineEQ.EndPointOptions.TagID.ToString(),
                            TaskName = $"*Local-{DateTime.Now.ToString("yyyyMMddHHmmssffff")}",
                            DispatcherName = "Local_Auto",
                            From_Slot = "1",
                            To_Slot = "1",
                            Priority = 80,
                            Height = destineEQ.EndPointOptions.Height
                        };
                        var taskAddedResult = await TaskManager.AddTask(taskOrder);
                        if (taskAddedResult.confirm)
                        {
                            LOG.INFO($"[Local Auto EQ Transfer] Task-{taskOrder.TaskName}-(From={taskOrder.From_Station} To={taskOrder.To_Station}>> Execute AGV={taskOrder.DesignatedAGVName}) is added.");
                            clsLocalAutoTransferTaskMonitor taskMonitor = new clsLocalAutoTransferTaskMonitor(taskOrder, sourceEQ, destineEQ);
                            MonitoringCarrerTasks.Add(taskOrder.TaskName, taskMonitor);
                            _ = Task.Factory.StartNew(async () =>
                            {
                                taskMonitor.OnMonitorEnd += () =>
                                {
                                    MonitoringCarrerTasks.Remove(taskOrder.TaskName, out var monitor);
                                    monitor?.Dispose();
                                };
                                await taskMonitor.Start();
                            });
                        }
                        else
                        {
                            LOG.ERROR($"[Local Auto EQ Transfer] Task-{taskOrder.TaskName}-(From={taskOrder.From_Station} To={taskOrder.To_Station}>> Execute AGV={taskOrder.DesignatedAGVName}) add FAILURE,{taskAddedResult.alarm_code}");
                            AlarmManagerCenter.AddAlarmAsync(new clsAlarmDto
                            {
                                Time = DateTime.Now,
                                AlarmCode = (int)taskAddedResult.alarm_code,
                                Description_Zh = taskAddedResult.alarm_code.ToString(),
                                Description_En = taskAddedResult.alarm_code.ToString(),
                                Level = ALARM_LEVEL.ALARM,
                                Task_Name = taskOrder.TaskName,
                                Source = ALARM_SOURCE.AGVS,
                                Equipment_Name = taskOrder.DesignatedAGVName,
                            });
                        }
                    }

                }

            }
            AutoRunning = false;
        }
        public static (bool confirm, ALARMS alarm_code, string message, object obj, Type objtype) CheckLoadUnloadStation(int station_tag, int LayerorSlot, ACTION_TYPE actiontype, bool check_rack_move_out_is_empty_or_full = true)
        {
            AGVSystemCommonNet6.MAP.MapPoint MapPoint = AGVSMapManager.GetMapPointByTag(station_tag);
            if (MapPoint == null)
                return new(false, ALARMS.EQ_TAG_NOT_EXIST_IN_CURRENT_MAP, $"站點TAG-{station_tag} 不存在於當前地圖", null, null);
            if (!MapPoint.Enable)
                return (false, ALARMS.Station_Disabled, "站點未啟用，無法指派任務", null, null);

            if (MapPoint.StationType == STATION_TYPE.EQ || MapPoint.StationType == STATION_TYPE.EQ_LD || MapPoint.StationType == STATION_TYPE.EQ_ULD || 
                (MapPoint.StationType == STATION_TYPE.Buffer_EQ && LayerorSlot == 0))
            {
                var Eq = StaEQPManagager.MainEQList.FirstOrDefault(eq => eq.EndPointOptions.TagID == station_tag);
                if (Eq == null)
                    return new(false, ALARMS.EQ_TAG_NOT_EXIST_IN_CURRENT_MAP, $"設備站點TAG-{station_tag},EQ-{Eq.EQName} 不存在於當前地圖", null, null);
                if (!Eq.IsConnected)
                    return new(false, ALARMS.Endpoint_EQ_NOT_CONNECTED, $"設備[{Eq.EQName}] 尚未連線,無法確認狀態", null, null);
                if (actiontype == ACTION_TYPE.Unload)
                {
                    if (Eq.Unload_Request == false)
                        return new(false, ALARMS.EQ_LOAD_REQUEST_IS_NOT_ON, $"設備[{Eq.EQName}] 沒有[出料]請求", null, null);
                    if (Eq.Port_Exist == false)
                        return new(false, ALARMS.EQ_UNLOAD_REQUEST_ON_BUT_NO_CARGO, $"設備[{Eq.EQName}] PORT內無貨物，無法載出", null, null);
                    if (Eq.Up_Pose == false)
                        return new(false, ALARMS.EQ_UNLOAD_REQUEST_ON_BUT_NO_CARGO, $"設備[{Eq.EQName}] Up_Pose=false", null, null);
                    if (check_rack_move_out_is_empty_or_full && Eq.EndPointOptions.CheckRackContentStateIOSignal && Eq.Is_RACK_HAS_TRAY_OR_NOT_TO_LDULD_Unknown)
                        return new(false, ALARMS.EQ_UNLOAD_REQ_BUT_RACK_FULL_OR_EMPTY_IS_UNKNOWN, $"設備[{Eq.EQName}] 無法確定要載出空框或實框", null, null);
                }
                else if (actiontype == ACTION_TYPE.Load)
                {
                    if (Eq.Load_Request == false)
                        return new(false, ALARMS.EQ_LOAD_REQUEST_IS_NOT_ON, $"設備[{Eq.EQName}] 沒有[入料]請求", null, null);
                    if (Eq.Port_Exist == true)
                        return new(false, ALARMS.EQ_LOAD_REQUEST_ON_BUT_HAS_CARGO, $"設備[{Eq.EQName}] 內有貨物，無法載入", null, null);
                    if (Eq.Down_Pose == false)
                        return new(false, ALARMS.EQ_UNLOAD_REQUEST_ON_BUT_NO_CARGO, $"設備[{Eq.EQName}] Down_Pose=false", null, null);
                    if (check_rack_move_out_is_empty_or_full && Eq.EndPointOptions.CheckRackContentStateIOSignal && Eq.Is_RACK_HAS_TRAY_OR_NOT_TO_LDULD_Unknown)
                        return new(false, ALARMS.EQ_LOAD_REQ_BUT_RACK_FULL_OR_EMPTY_IS_UNKNOWN, $"設備[{Eq.EQName}] 無法確定要載入空框或實框", null, null);
                }
                return new(true, ALARMS.NONE, $"GET EQ", Eq, Eq.GetType());
            }
            else if (MapPoint.StationType == STATION_TYPE.Buffer || MapPoint.StationType == STATION_TYPE.Charge_Buffer)
            {
                var Eq = StaEQPManagager.MainEQList.FirstOrDefault(eq => eq.EndPointOptions.TagID == station_tag);
                if (Eq != null)
                {
                    if (!Eq.IsConnected)
                        return new(false, ALARMS.Endpoint_EQ_NOT_CONNECTED, $"設備[{Eq.EQName}] 尚未連線,無法確認狀態", null, null);
                    if (actiontype == ACTION_TYPE.Unload)
                    {
                        if (Eq.Unload_Request == false)
                            return new(false, ALARMS.EQ_LOAD_REQUEST_IS_NOT_ON, $"設備[{Eq.EQName}] 沒有[出料]請求", null, null);
                        if (Eq.Port_Exist == false)
                            return new(false, ALARMS.EQ_UNLOAD_REQUEST_ON_BUT_NO_CARGO, $"設備[{Eq.EQName}] PORT內無貨物，無法載出", null, null);
                        if (Eq.Up_Pose == false)
                            return new(false, ALARMS.EQ_UNLOAD_REQUEST_ON_BUT_NO_CARGO, $"設備[{Eq.EQName}] Up_Pose=false", null, null);
                        if (check_rack_move_out_is_empty_or_full && Eq.EndPointOptions.CheckRackContentStateIOSignal && Eq.Is_RACK_HAS_TRAY_OR_NOT_TO_LDULD_Unknown)
                            return new(false, ALARMS.EQ_UNLOAD_REQ_BUT_RACK_FULL_OR_EMPTY_IS_UNKNOWN, $"設備[{Eq.EQName}] 無法確定要載出空框或實框", null, null);
                    }
                    else if (actiontype == ACTION_TYPE.Load)
                    {
                        if (Eq.Load_Request == false)
                            return new(false, ALARMS.EQ_LOAD_REQUEST_IS_NOT_ON, $"設備[{Eq.EQName}] 沒有[入料]請求", null, null);
                        if (Eq.Port_Exist == true)
                            return new(false, ALARMS.EQ_LOAD_REQUEST_ON_BUT_HAS_CARGO, $"設備[{Eq.EQName}] 內有貨物，無法載入", null, null);
                        if (Eq.Down_Pose == false)
                            return new(false, ALARMS.EQ_UNLOAD_REQUEST_ON_BUT_NO_CARGO, $"設備[{Eq.EQName}] Down_Pose=false", null, null);
                        if (check_rack_move_out_is_empty_or_full && Eq.EndPointOptions.CheckRackContentStateIOSignal && Eq.Is_RACK_HAS_TRAY_OR_NOT_TO_LDULD_Unknown)
                            return new(false, ALARMS.EQ_LOAD_REQ_BUT_RACK_FULL_OR_EMPTY_IS_UNKNOWN, $"設備[{Eq.EQName}] 無法確定要載入空框或實框", null, null);
                    }
                    return new(true, ALARMS.NONE, $"GET EQ", Eq, Eq.GetType());
                }
                else
                {
                    List<clsPortOfRack> ports = StaEQPManagager.GetRackColumnByTag(station_tag);
                    var Rack = ports.FirstOrDefault().ParentRack;
                    if (Rack == null)
                        return new(false, ALARMS.EQ_TAG_NOT_EXIST_IN_CURRENT_MAP, $"WIP站點TAG-{station_tag},EQ-{Rack.EQName} 不存在於當前地圖", null, null);
                    if (Rack.IsConnected == false)
                        return new(false, ALARMS.Endpoint_EQ_NOT_CONNECTED, $"WIP [{Rack.EQName}] 尚未連線,無法確認狀態", null, null);
                    clsPortOfRack specificport = ports.Where(x => x.Layer == LayerorSlot).FirstOrDefault();
                    if (specificport == null)
                        return new(false, ALARMS.EQ_LOAD_REQUEST_IS_NOT_ON, $"WIP設備[{Rack.EQName}, ID:{specificport.Properties.ID}] 料座不存在", null, null);
                    if (actiontype == ACTION_TYPE.Unload)
                    {
                        if (specificport.CargoExist == false)
                            return new(false, ALARMS.EQ_LOAD_REQUEST_IS_NOT_ON, $"WIP設備[{Rack.EQName}, ID:{specificport.Properties.ID}] 料座無貨", null, null);
                    }
                    else if (actiontype == ACTION_TYPE.Load || actiontype == ACTION_TYPE.LoadAndPark)
                    {
                        if (specificport.CargoExist == true)
                            return new(false, ALARMS.EQ_LOAD_REQUEST_IS_NOT_ON, $"WIP設備[{Rack.EQName}, ID:{specificport.Properties.ID}] 料座已占用", null, null);
                    }
                    return new(true, ALARMS.NONE, $" GET RACK", specificport, specificport.GetType());
                }
            }
            else if (MapPoint.StationType == STATION_TYPE.Buffer_EQ && LayerorSlot >= 1) // Buffer_EQ slot >=1 先確認WIP儲位但還是要預約EQ訊號
            {
                List<clsPortOfRack> ports = StaEQPManagager.GetRackColumnByTag(station_tag);
                var Rack = ports.FirstOrDefault().ParentRack;
                if (Rack == null)
                    return new(false, ALARMS.EQ_TAG_NOT_EXIST_IN_CURRENT_MAP, $"WIP站點TAG-{station_tag},EQ-{Rack.EQName} 不存在於當前地圖", null, null);
                if (Rack.IsConnected == false)
                    return new(false, ALARMS.Endpoint_EQ_NOT_CONNECTED, $"WIP [{Rack.EQName}] 尚未連線,無法確認狀態", null, null);
                clsPortOfRack specificport = ports.Where(x => x.Layer == LayerorSlot).FirstOrDefault();
                if (specificport == null)
                    return new(false, ALARMS.EQ_LOAD_REQUEST_IS_NOT_ON, $"WIP設備[{Rack.EQName}, ID:{specificport.Properties.ID}] 料座不存在", null, null);
                if (actiontype == ACTION_TYPE.Unload)
                {
                    if (specificport.CargoExist == false)
                        return new(false, ALARMS.EQ_LOAD_REQUEST_IS_NOT_ON, $"WIP設備[{Rack.EQName}, ID:{specificport.Properties.ID}] 料座無貨", null, null);
                }
                else if (actiontype == ACTION_TYPE.Load || actiontype == ACTION_TYPE.LoadAndPark)
                {
                    if (specificport.CargoExist == true)
                        return new(false, ALARMS.EQ_LOAD_REQUEST_IS_NOT_ON, $"WIP設備[{Rack.EQName}, ID:{specificport.Properties.ID}] 料座已占用", null, null);
                }
                var Eq = StaEQPManagager.MainEQList.FirstOrDefault(eq => eq.EndPointOptions.TagID == station_tag);
                if (Eq == null)
                    return new(false, ALARMS.EQ_TAG_NOT_EXIST_IN_CURRENT_MAP, $"設備站點TAG-{station_tag},EQ-{Eq.EQName} 不存在於當前地圖", null, null);
                if (!Eq.IsConnected)
                    return new(false, ALARMS.Endpoint_EQ_NOT_CONNECTED, $"設備[{Eq.EQName}] 尚未連線,無法確認狀態", null, null);
                //if (actiontype == ACTION_TYPE.Unload)
                //{
                //    if (Eq.Unload_Request == false)
                //        return new(false, ALARMS.EQ_LOAD_REQUEST_IS_NOT_ON, $"設備[{Eq.EQName}] 沒有[出料]請求", null, null);
                //    if (Eq.Port_Exist == false)
                //        return new(false, ALARMS.EQ_UNLOAD_REQUEST_ON_BUT_NO_CARGO, $"設備[{Eq.EQName}] PORT內無貨物，無法載出", null, null);
                //    if (Eq.Up_Pose == false)
                //        return new(false, ALARMS.EQ_UNLOAD_REQUEST_ON_BUT_NO_CARGO, $"設備[{Eq.EQName}] Up_Pose=false", null, null);
                //    if (check_rack_move_out_is_empty_or_full && Eq.EndPointOptions.CheckRackContentStateIOSignal && Eq.Is_RACK_HAS_TRAY_OR_NOT_TO_LDULD_Unknown)
                //        return new(false, ALARMS.EQ_UNLOAD_REQ_BUT_RACK_FULL_OR_EMPTY_IS_UNKNOWN, $"設備[{Eq.EQName}] 無法確定要載出空框或實框", null, null);
                //}
                //else if (actiontype == ACTION_TYPE.Load)
                //{
                //    if (Eq.Load_Request == false)
                //        return new(false, ALARMS.EQ_LOAD_REQUEST_IS_NOT_ON, $"設備[{Eq.EQName}] 沒有[入料]請求", null, null);
                //    if (Eq.Port_Exist == true)
                //        return new(false, ALARMS.EQ_LOAD_REQUEST_ON_BUT_HAS_CARGO, $"設備[{Eq.EQName}] 內有貨物，無法載入", null, null);
                //    if (Eq.Down_Pose == false)
                //        return new(false, ALARMS.EQ_UNLOAD_REQUEST_ON_BUT_NO_CARGO, $"設備[{Eq.EQName}] Down_Pose=false", null, null);
                //    if (check_rack_move_out_is_empty_or_full && Eq.EndPointOptions.CheckRackContentStateIOSignal && Eq.Is_RACK_HAS_TRAY_OR_NOT_TO_LDULD_Unknown)
                //        return new(false, ALARMS.EQ_LOAD_REQ_BUT_RACK_FULL_OR_EMPTY_IS_UNKNOWN, $"設備[{Eq.EQName}] 無法確定要載入空框或實框", null, null);
                //}
                return new(true, ALARMS.NONE, $" GET EQ RACK", Eq, Eq.GetType());
            }
            else
            { return new(false, ALARMS.EQ_TAG_NOT_EXIST_IN_CURRENT_MAP, $"設備站點TAG-{station_tag} 不存在於當前地圖", null, null); }
        }
        public static clsPortOfRack get_empyt_port_of_rack(int _station_tag)
        {
            List<clsPortOfRack> ports = StaEQPManagager.GetRackColumnByTag(_station_tag);
            clsPortOfRack port = ports.Where(x => x.CargoExist == false).OrderBy(x => x.Layer).FirstOrDefault();
            return port==null ? null : port;
            
        }


        public static (bool confirm, ALARMS alarm_code, string message) CheckUnloadStationStatus(int station_tag, bool check_rack_move_out_is_empty_or_full = true)
        {
            bool _eq_exist = TryGetStationEQDIStatus(station_tag, out clsEQ equipment);
            if (!_eq_exist)
                return new(false, ALARMS.EQ_TAG_NOT_EXIST_IN_CURRENT_MAP, $"設備站點TAG-{station_tag} 不存在於當前地圖");
            if (!equipment.IsConnected)
                return new(false, ALARMS.Endpoint_EQ_NOT_CONNECTED, $"設備[{equipment.EQName}] 尚未連線,無法確認狀態");
            if (!equipment.Unload_Request)
                return new(false, ALARMS.EQ_LOAD_REQUEST_IS_NOT_ON, $"設備[{equipment.EQName}] 沒有[出料]請求");
            if (!equipment.Port_Exist)
                return new(false, ALARMS.EQ_UNLOAD_REQUEST_ON_BUT_NO_CARGO, $"設備[{equipment.EQName}] PORT內無貨物，無法載出");
            if (check_rack_move_out_is_empty_or_full && equipment.EndPointOptions.CheckRackContentStateIOSignal && equipment.Is_RACK_HAS_TRAY_OR_NOT_TO_LDULD_Unknown)
                return new(false, ALARMS.EQ_UNLOAD_REQ_BUT_RACK_FULL_OR_EMPTY_IS_UNKNOWN, $"設備[{equipment.EQName}] 無法確定要載出空框或實框");

            return new(true, ALARMS.NONE, "");
        }
        public static (bool confirm, ALARMS alarm_code, string message) CheckLoadStationStatus(int station_tag, bool check_rack_move_out_is_empty_or_full = true)
        {
            bool _eq_exist = TryGetStationEQDIStatus(station_tag, out clsEQ equipment);
            if (!_eq_exist)
                return new(false, ALARMS.EQ_TAG_NOT_EXIST_IN_CURRENT_MAP, $"設備站點TAG-{station_tag} 不存在於當前地圖");
            if (!equipment.IsConnected)
                return new(false, ALARMS.Endpoint_EQ_NOT_CONNECTED, $"設備[{equipment.EQName}] 尚未連線,無法確認狀態");
            if (!equipment.Load_Request)
                return new(false, ALARMS.EQ_LOAD_REQUEST_IS_NOT_ON, $"設備[{equipment.EQName}] 沒有[入料]請求");
            if (equipment.Port_Exist)
                return new(false, ALARMS.EQ_LOAD_REQUEST_ON_BUT_HAS_CARGO, $"設備[{equipment.EQName}] 內有貨物，無法載入");
            if (check_rack_move_out_is_empty_or_full && equipment.EndPointOptions.CheckRackContentStateIOSignal && equipment.Is_RACK_HAS_TRAY_OR_NOT_TO_LDULD_Unknown)
                return new(false, ALARMS.EQ_LOAD_REQ_BUT_RACK_FULL_OR_EMPTY_IS_UNKNOWN, $"設備[{equipment.EQName}] 無法確定要載入空框或實框");

            return new(true, ALARMS.NONE, "");
        }
        public static bool TryGetStationEQDIStatus(int station_tag, out clsEQ equipment)
        {
            equipment = null;
            KeyValuePair<int, MapPoint> StationOnMap = AGVSMapManager.CurrentMap.Points.FirstOrDefault(pt => pt.Value.TagNumber == station_tag);
            if (StationOnMap.Value == null)
            {
                return false;
            }
            equipment = StaEQPManagager.GetEQByTag(station_tag);
            return equipment != null;
        }
        public static (bool confirm, ALARMS alarm_code, string message) CheckUnloadRackStatus(int station_tag, bool check_rack_move_out_is_empty_or_full = true)
        {
            return new(true, ALARMS.NONE, "");
        }
        public static (bool confirm, ALARMS alarm_code, string message, clsRack rack) CheckLoadUnloadRackStatus(int station_tag, string strSlot = "-1")
        {
            bool _eq_exist = TryGetStationWIP(station_tag, out clsRack rack);
            if (!_eq_exist)
                return new(false, ALARMS.EQ_TAG_NOT_EXIST_IN_CURRENT_MAP, $"WIP站點TAG-{station_tag} 不存在於當前地圖", null);
            if (!rack.IsConnected)
                return new(false, ALARMS.Endpoint_EQ_NOT_CONNECTED, $"設備[{rack.EQName}] 尚未連線,無法確認狀態", rack);
            return new(true, ALARMS.NONE, "", rack);
        }

        public static (bool confirm, ALARMS alarm_code, string message) CheckLoadUnloadPortofRackStatus(int station_tag, ACTION_TYPE actiontype, clsRack rack, string strSlot = "-1")
        {
            List<clsPortOfRack> ports = StaEQPManagager.GetRackColumnByTag(station_tag);

            clsPortOfRack specificport = ports.Where(x => x.Layer == Convert.ToInt32(strSlot)).FirstOrDefault();
            if (specificport == null)
                return new(false, ALARMS.EQ_LOAD_REQUEST_IS_NOT_ON, $"WIP設備[{rack.EQName}, ID:{specificport.Properties.ID}] 料座不存在");
            if (actiontype == ACTION_TYPE.Unload)
            {
                if (specificport.CargoExist == false)
                    return new(false, ALARMS.EQ_LOAD_REQUEST_IS_NOT_ON, $"WIP設備[{rack.EQName}, ID:{specificport.Properties.ID}] 料座無貨");
            }
            else if (actiontype == ACTION_TYPE.Load || actiontype == ACTION_TYPE.LoadAndPark)
            {
                if (specificport.CargoExist == true)
                    return new(false, ALARMS.EQ_LOAD_REQUEST_IS_NOT_ON, $"WIP設備[{rack.EQName}, ID:{specificport.Properties.ID}] 料座已占用");
            }
            return new(true, ALARMS.NONE, "");
        }

        public static bool TryGetStationWIP(int station_tag, out clsRack rack)
        {
            rack = null;
            KeyValuePair<int, MapPoint> StationOnMap = AGVSMapManager.CurrentMap.Points.FirstOrDefault(pt => pt.Value.TagNumber == station_tag);
            if (StationOnMap.Value == null)
            {
                return false;
            }
            rack = StaEQPManagager.GetRackByTag(station_tag);
            return rack != null;
        }


        public static (bool confirm, ALARMS alarm_code, string message) CheckEQLDULDStatus(ACTION_TYPE action, int from_tag, int to_tag)
        {
            try
            {
                //TODO If To EQ Is WIP
                KeyValuePair<int, MapPoint> ToStation = AGVSMapManager.CurrentMap.Points.FirstOrDefault(pt => pt.Value.TagNumber == to_tag);
                if (ToStation.Value == null)
                {
                    return new(false, ALARMS.EQ_TAG_NOT_EXIST_IN_CURRENT_MAP, $"設備站點TAG-{to_tag} 不存在於當前地圖");
                }
                EQStatusDIDto ToEQStatus = StaEQPManagager.GetEQStatesByTagID(to_tag);
                EQStatusDIDto FromEQStatus = StaEQPManagager.GetEQStatesByTagID(from_tag);

                if (ToEQStatus != null)
                {
                    if (!ToEQStatus.IsConnected)
                        return new(false, ALARMS.Endpoint_EQ_NOT_CONNECTED, $"設備[{ToEQStatus.EQName}] 尚未連線,無法確認狀態");

                    if (action == ACTION_TYPE.Load | action == ACTION_TYPE.LoadAndPark)
                    {
                        return new(ToEQStatus.Load_Request, ALARMS.EQ_LOAD_REQUEST_IS_NOT_ON, $"設備[{ToEQStatus.EQName}] 沒有[入料]請求");
                    }
                    else if (action == ACTION_TYPE.Carry)
                    {
                        if (!FromEQStatus.Unload_Request)
                            return new(false, ALARMS.EQ_UNLOAD_REQUEST_IS_NOT_ON, $"設備[{ToEQStatus.EQName}] 沒有[入料]請求");
                        if (!ToEQStatus.Load_Request)
                            return new(false, ALARMS.EQ_LOAD_REQUEST_IS_NOT_ON, $"設備[{ToEQStatus.EQName}] 沒有[出料]請求");

                        return new(true, ALARMS.NONE, "");
                    }
                    else
                    {
                        return new(ToEQStatus.Unload_Request, ALARMS.EQ_UNLOAD_REQUEST_IS_NOT_ON, $"設備[{ToEQStatus.EQName}] 沒有[入料]請求");
                    }
                }
                else
                {
                    if (ToStation.Value.StationType == STATION_TYPE.STK)
                        return new(true, ALARMS.NONE, "");
                    else
                        return new(false, ALARMS.Endpoint_EQ_NOT_CONNECTED, $"設備[{ToEQStatus.EQName}] 沒有入料請求");
                }
            }
            catch (Exception ex)
            {
                LOG.Critical(ex);
                return new(false, ALARMS.SYSTEM_ERROR, ex.Message);
            }

        }

        public static (bool confirm, ALARMS alarm_code, string message) CheckEQAcceptCargoType(clsTaskDto taskData)
        {
            clsEQ source_equipment = StaEQPManagager.GetEQByTag(taskData.From_Station_Tag);
            clsEQ destine_equipment = StaEQPManagager.GetEQByTag(taskData.To_Station_Tag);
            int FromStation_CSTType = 0;
            int ToStation_CSTType = 0;

            if (source_equipment != null)
            {
                EQ_ACCEPT_CARGO_TYPE source_eq_accept_cargo_type = source_equipment.EndPointOptions.EQAcceeptCargoType;
                FromStation_CSTType = (int)source_eq_accept_cargo_type;
            }
            else
                return new(false, ALARMS.EQ_TAG_NOT_EXIST_IN_CURRENT_MAP, $"設備站點TAG-{taskData.From_Station_Tag} 不存在於當前地圖");
            if (destine_equipment != null)
            {
                EQ_ACCEPT_CARGO_TYPE source_eq_accept_cargo_type = destine_equipment.EndPointOptions.EQAcceeptCargoType;
                ToStation_CSTType = (int)source_eq_accept_cargo_type;
            }
            else
                return new(false, ALARMS.EQ_TAG_NOT_EXIST_IN_CURRENT_MAP, $"設備站點TAG-{taskData.To_Station_Tag} 不存在於當前地圖");

            if ((EQ_ACCEPT_CARGO_TYPE)FromStation_CSTType == EQ_ACCEPT_CARGO_TYPE.None && (EQ_ACCEPT_CARGO_TYPE)ToStation_CSTType == EQ_ACCEPT_CARGO_TYPE.None)
            {
                taskData.CST_TYPE = FromStation_CSTType;
                return new(true, ALARMS.NONE, "");
            }
            else if ((EQ_ACCEPT_CARGO_TYPE)FromStation_CSTType == EQ_ACCEPT_CARGO_TYPE.None)
            {
                taskData.CST_TYPE = ToStation_CSTType;
                return new(true, ALARMS.NONE, "");
            }
            else if ((EQ_ACCEPT_CARGO_TYPE)ToStation_CSTType == EQ_ACCEPT_CARGO_TYPE.None)
            {
                taskData.CST_TYPE = FromStation_CSTType;
                return new(true, ALARMS.NONE, "");
            }
            else if (FromStation_CSTType == ToStation_CSTType)
            {
                taskData.CST_TYPE = FromStation_CSTType;
                return new(true, ALARMS.NONE, "");
            }
            else
                return new(false, ALARMS.AGV_Type_Is_Not_Allow_To_Execute_Task_At_Source_Equipment, $"FromStation Accept: {(EQ_ACCEPT_CARGO_TYPE)FromStation_CSTType} and ToStation Accept: {(EQ_ACCEPT_CARGO_TYPE)ToStation_CSTType} @@NOT MATCH");
        }

        public static (bool confirm, ALARMS alarm_code, string message) CheckEQAcceptAGVType(int station_tag, string _agv_name)
        {
            using AGVSDatabase database = new AGVSDatabase();
            IEnumerable<clsAGVStateDto> agvstates = database.tables.AgvStates;

            clsAGVStateDto? _agv_assigned = agvstates.FirstOrDefault(agv_dat => agv_dat.AGV_Name == _agv_name);
            VEHICLE_TYPE model = _agv_assigned.Model.ConvertToEQAcceptAGVTYPE();
            clsEQ equipment = StaEQPManagager.GetEQByTag(station_tag);
            if (equipment == null)
                return new(false, ALARMS.EQ_TAG_NOT_EXIST_IN_CURRENT_MAP, $"設備站點TAG-{station_tag} 不存在於當前地圖");
            else
            {
                VEHICLE_TYPE source_eq_accept_agv_model = equipment.EndPointOptions.Accept_AGV_Type;
                if (source_eq_accept_agv_model != VEHICLE_TYPE.ALL && source_eq_accept_agv_model != model)
                    return (false, ALARMS.AGV_Type_Is_Not_Allow_To_Execute_Task_At_Source_Equipment, $"設備TAG-{station_tag}不允許{model}車種進行任務");
                else
                    return new(true, ALARMS.NONE, "");
            }
        }
        public static (bool confirm, ALARMS alarm_code, string message) CheckRackAcceptAGVType(int station_tag, string _agv_name)
        {
            using AGVSDatabase database = new AGVSDatabase();
            IEnumerable<clsAGVStateDto> agvstates = database.tables.AgvStates;

            clsAGVStateDto? _agv_assigned = agvstates.FirstOrDefault(agv_dat => agv_dat.AGV_Name == _agv_name);
            VEHICLE_TYPE model = _agv_assigned.Model.ConvertToEQAcceptAGVTYPE();
            clsRack rack = StaEQPManagager.GetRackByTag(station_tag);
            if (rack == null)
                return new(false, ALARMS.EQ_TAG_NOT_EXIST_IN_CURRENT_MAP, $"RACK站點TAG-{station_tag} 不存在於當前地圖");
            else
            {
                VEHICLE_TYPE source_eq_accept_agv_model = rack.EndPointOptions.Accept_AGV_Type;
                if (source_eq_accept_agv_model != VEHICLE_TYPE.ALL && source_eq_accept_agv_model != model)
                    return (false, ALARMS.AGV_Type_Is_Not_Allow_To_Execute_Task_At_Source_Equipment, $"設備不允許{model}車種進行任務");
                else
                    return new(true, ALARMS.NONE, "");
            }
        }

        public static (bool confirm, ALARMS alarm_code, string message) CheckEQAcceptAGVType(ref clsTaskDto taskData)
        {
            string _agv_name = taskData.DesignatedAGVName;
            if (_agv_name == "" || _agv_name == null)
                return new(true, ALARMS.NONE, "");
            using AGVSDatabase database = new AGVSDatabase();
            IEnumerable<clsAGVStateDto> agvstates = database.tables.AgvStates;

            clsAGVStateDto? _agv_assigned = agvstates.FirstOrDefault(agv_dat => agv_dat.AGV_Name == _agv_name);
            VEHICLE_TYPE model = _agv_assigned.Model.ConvertToEQAcceptAGVTYPE();

            clsEQ source_equipment = StaEQPManagager.GetEQByTag(taskData.From_Station_Tag);
            clsEQ destine_equipment = StaEQPManagager.GetEQByTag(taskData.To_Station_Tag);

            // 不檢查起始站點 因為可能是放貨任務
            if (destine_equipment == null)
                return new(false, ALARMS.EQ_TAG_NOT_EXIST_IN_CURRENT_MAP, $"設備站點TAG-{taskData.To_Station_Tag} 不存在於當前地圖");

            VEHICLE_TYPE destine_eq_accept_agv_model = destine_equipment.EndPointOptions.Accept_AGV_Type;
            if (taskData.need_change_agv == false)
            {
                if (destine_eq_accept_agv_model != VEHICLE_TYPE.ALL && destine_eq_accept_agv_model != model) // 自動改成轉運任務
                {
                    taskData.need_change_agv = true;// VMS OrderHandlerFactory._CreateSequenceTasks 會依這個變數取得轉換站並判別To_Station是不是要改成轉運站
                    taskData.ChangeAGVMiddleStationTag = -1;
                    taskData.TransferToDestineAGVName = "";
                }
                else // 直達車
                    taskData.need_change_agv = false;
            }
            else
            {
                if (taskData.TransferFromTag != -1)
                {   //檢查轉運站可用車種
                    clsEQ transferStation_equipment = StaEQPManagager.GetEQByTag(taskData.TransferFromTag);
                    VEHICLE_TYPE transferStation_eq_accept_agv_model = transferStation_equipment.EndPointOptions.Accept_AGV_Type;
                    if (transferStation_eq_accept_agv_model != VEHICLE_TYPE.ALL && transferStation_eq_accept_agv_model != transferStation_eq_accept_agv_model)
                        return (false, ALARMS.AGV_Type_Is_Not_Allow_To_Execute_Task_At_Destine_Equipment, $"轉運設備不允許{model}車種進行任務");
                }
                else { } //不指定轉運站，會在第一段任務結束轉成第二段任務之後找任務起點可去的離轉運站
                string strTransferToDestineAGVName = taskData.TransferToDestineAGVName;
                if (strTransferToDestineAGVName != "")
                {
                    //檢查終點站可用車種
                    var toDestineAGV = agvstates.FirstOrDefault(agv_dat => agv_dat.AGV_Name == strTransferToDestineAGVName);
                    var modelToDestine = toDestineAGV.Model.ConvertToEQAcceptAGVTYPE();
                    if (destine_eq_accept_agv_model != VEHICLE_TYPE.ALL && destine_eq_accept_agv_model != modelToDestine)
                        return (false, ALARMS.AGV_Type_Is_Not_Allow_To_Execute_Task_At_Destine_Equipment, $"終點設備不允許{modelToDestine}車種進行任務");
                }
                else { } //不指定轉運車種，會在第一段任務結束轉成第二段任務之後自動找離轉運站最近的車
            }
            return new(true, ALARMS.NONE, "");
        }

        private static bool IsEQDataValid(EndPointDeviceAbstract endpoint, out int unloadStationTag, out ALARMS alarm_code)
        {
            alarm_code = ALARMS.NONE;
            unloadStationTag = endpoint.EndPointOptions.TagID;
            MapPoint point = AGVSMapManager.GetMapPointByTag(unloadStationTag);
            if (point == null)
            {
                alarm_code = ALARMS.EQ_UNLOAD_REQUEST_ON_BUT_TAG_NOT_EXIST_ON_MAP;
                return false;
            }
            if (!point.IsEQLink)
            {
                alarm_code = ALARMS.EQ_UNLOAD_REQUEST_ON_BUT_STATION_TYPE_SETTING_IS_NOT_EQ;
                return false;
            }

            return true;
        }

    }
}
