using AGVSystem.Models.Map;
using AGVSystem.Models.Sys;
using AGVSystemCommonNet6;
using AGVSystemCommonNet6.AGVDispatch;
using AGVSystemCommonNet6.AGVDispatch.Messages;
using AGVSystemCommonNet6.AGVDispatch.RunMode;
using AGVSystemCommonNet6.Alarm;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.MAP;
using EquipmentManagment.Device.Options;
using EquipmentManagment.MainEquipment;
using EquipmentManagment.Manager;
using EquipmentManagment.WIP;
using Microsoft.EntityFrameworkCore;
using NLog;
using static AGVSystemCommonNet6.MAP.MapPoint;

namespace AGVSystem.TaskManagers
{
    public class EQTransferTaskManager
    {
        public static bool AutoRunning { get; private set; } = false;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public static void Initialize()
        {
            //clsEQ.OnEqUnloadRequesting += ClsEQ_OnEqUnloadRequesting;
            SystemModes.OnRunModeON += SwitchToRunMode;
            SystemModes.OnRunModeOFF += SwitchToMaintainMode;
            TransferTaskPairWorker();
        }
        /// <summary>
        /// 儲存自動搬運任務, key=> SourceEQ , value=>DestineEQ
        /// </summary>
        public static Dictionary<clsEQ, clsEQ> UnloadEQQueueing { get; private set; } = new Dictionary<clsEQ, clsEQ>();
        internal static async void SwitchToMaintainMode()
        {
            while (AutoRunning)
            {
                await Task.Delay(1);
            }
            logger.Warn("Maintain Mode Start");
            //取消預約所有機台
            StaEQPManagager.MainEQList.FindAll(eq => eq.CMD_Reserve_Low | eq.CMD_Reserve_Up).ForEach(eq =>
            {
                eq.CancelReserve();
            });
            UnloadEQQueueing.Clear();
        }

        internal static async void SwitchToRunMode()
        {
            logger.Warn("Run Mode Start");
        }
        public static (bool confirm, ALARMS alarm_code, string message, object obj, Type objtype) CheckLoadUnloadStation(int station_tag, int LayerorSlot, ACTION_TYPE actiontype, bool check_rack_move_out_is_empty_or_full = true, bool bypasseqandrackckeck = false)
        {
            if (bypasseqandrackckeck == true)
                return new(true, ALARMS.NONE, $"By Pass EQ and Rack Check", null, null);
            AGVSystemCommonNet6.MAP.MapPoint MapPoint = AGVSMapManager.GetMapPointByTag(station_tag);
            if (MapPoint == null)
                return new(false, ALARMS.EQ_TAG_NOT_EXIST_IN_CURRENT_MAP, $"站點TAG-{station_tag} 不存在於當前地圖", null, null);
            if (!MapPoint.Enable)
                return (false, ALARMS.Station_Disabled, "站點未啟用，無法指派任務", null, null);

            if (MapPoint.StationType == STATION_TYPE.EQ || MapPoint.StationType == STATION_TYPE.EQ_LD || MapPoint.StationType == STATION_TYPE.EQ_ULD ||
                (MapPoint.StationType == STATION_TYPE.Buffer_EQ && LayerorSlot == 0))
            {
                clsEQ? Eq = StaEQPManagager.MainEQList.FirstOrDefault(eq => eq.EndPointOptions.TagID == station_tag && eq.EndPointOptions.Height == LayerorSlot);
                if (Eq == null)
                    return new(false, ALARMS.EQ_TAG_NOT_EXIST_IN_CURRENT_MAP, $"設備站點TAG-{station_tag},EQ不存在於當前地圖", null, null);

                logger.Trace($"AGV請求[{actiontype}]於設備-{MapPoint.Graph.Display}(Tag={station_tag}),設備狀態=>\r\n{Eq.GetStatusDescription()}");

                if (!Eq.IsConnected)
                    return new(false, ALARMS.Endpoint_EQ_NOT_CONNECTED, $"設備[{Eq.EQName}] 尚未連線,無法確認狀態", null, null);
                //if (!Eq.IS_EQ_STATUS_NORMAL_IDLE)
                //{
                //    string description = Eq.EndPointOptions.IOLocation.STATUS_IO_SPEC_VERSION == clsEQIOLocation.STATUS_IO_DEFINED_VERSION.V1 ? "EQP_STATUS_IDLE 訊號未ON" : "EQP_STATUS_DOWN 訊號未ON";
                //    return new(false, ALARMS.PortStatusisWrongCannottLoadUnload, $"設備[{Eq.EQName}] 狀態錯誤[{description}]", null, null);
                //}
                if (actiontype == ACTION_TYPE.Unload)
                {
                    if (Eq.Unload_Request == false)
                        return new(false, ALARMS.EQ_UNLOAD_REQUEST_IS_NOT_ON, $"設備[{Eq.EQName}] 沒有[出料]請求", null, null);
                    if (Eq.Port_Exist == false)
                        return new(false, ALARMS.EQ_UNLOAD_REQUEST_ON_BUT_NO_CARGO, $"設備[{Eq.EQName}] PORT內無貨物，無法載出", null, null);
                    if (Eq.EndPointOptions.HasLDULDMechanism && Eq.Up_Pose == false)
                        return new(false, ALARMS.EQ_UNLOAD_REQUEST_ON_BUT_POSE_NOT_UP, $"設備[{Eq.EQName}] Up_Pose=false", null, null);

                    if (check_rack_move_out_is_empty_or_full && Eq.EndPointOptions.CheckRackContentStateIOSignal && Eq.Is_RACK_HAS_TRAY_OR_NOT_TO_LDULD_Unknown)
                        return new(false, ALARMS.EQ_UNLOAD_REQ_BUT_RACK_FULL_OR_EMPTY_IS_UNKNOWN, $"設備[{Eq.EQName}] 無法確定要載出空框或實框", null, null);
                }
                else if (actiontype == ACTION_TYPE.Load)
                {
                    if (Eq.Load_Request == false)
                        return new(false, ALARMS.EQ_LOAD_REQUEST_IS_NOT_ON, $"設備[{Eq.EQName}] 沒有[入料]請求", null, null);
                    if (Eq.Port_Exist == true)
                        return new(false, ALARMS.EQ_LOAD_REQUEST_ON_BUT_HAS_CARGO, $"設備[{Eq.EQName}] 內有貨物，無法載入", null, null);
                    if (Eq.EndPointOptions.HasLDULDMechanism && Eq.Down_Pose == false)
                        return new(false, ALARMS.EQ_LOAD_REQUEST_ON_BUT_POSE_NOT_DOWN, $"設備[{Eq.EQName}] Down_Pose=false", null, null);
                    //if (check_rack_move_out_is_empty_or_full && Eq.EndPointOptions.CheckRackContentStateIOSignal && Eq.Is_RACK_HAS_TRAY_OR_NOT_TO_LDULD_Unknown)
                    //    return new(false, ALARMS.EQ_LOAD_REQ_BUT_RACK_FULL_OR_EMPTY_IS_UNKNOWN, $"設備[{Eq.EQName}] 無法確定要載入空框或實框", null, null);
                }
                ////檢查貨物轉向機構位置狀態(例如平對平設備)
                if (Eq.EndPointOptions.HasCstSteeringMechanism && Eq.TB_Down_Pose != true)
                {
                    return new(false, ALARMS.EQ_LOAD_REQUEST_ON_BUT_POSE_NOT_DOWN, $"設備[{Eq.EQName}] 貨物轉向機構位置非位於低位", null, null);
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
                            return new(false, ALARMS.EQ_UNLOAD_REQUEST_IS_NOT_ON, $"設備[{Eq.EQName}] 沒有[出料]請求", null, null);
                        if (Eq.Port_Exist == false)
                            return new(false, ALARMS.EQ_UNLOAD_REQUEST_ON_BUT_NO_CARGO, $"設備[{Eq.EQName}] PORT內無貨物，無法載出", null, null);
                        if (Eq.Up_Pose == false)
                            return new(false, ALARMS.EQ_UNLOAD_REQUEST_ON_BUT_POSE_NOT_UP, $"設備[{Eq.EQName}] Up_Pose=false", null, null);
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
                            return new(false, ALARMS.EQ_LOAD_REQUEST_ON_BUT_POSE_NOT_DOWN, $"設備[{Eq.EQName}] Down_Pose=false", null, null);
                        //if (check_rack_move_out_is_empty_or_full && Eq.EndPointOptions.CheckRackContentStateIOSignal && Eq.Is_RACK_HAS_TRAY_OR_NOT_TO_LDULD_Unknown)
                        //    return new(false, ALARMS.EQ_LOAD_REQ_BUT_RACK_FULL_OR_EMPTY_IS_UNKNOWN, $"設備[{Eq.EQName}] 無法確定要載入空框或實框", null, null);
                    }
                    return new(true, ALARMS.NONE, $"GET EQ", Eq, Eq.GetType());
                }
                else
                {
                    List<clsPortOfRack> ports = StaEQPManagager.GetRackColumnByTag(station_tag);
                    if (ports.Count <= 0)
                        return new(false, ALARMS.EQ_TAG_NOT_EXIST_IN_CURRENT_MAP, $"WIP站點TAG-{station_tag}, 無port存在於當前地圖", null, null);
                    var Rack = ports.FirstOrDefault().GetParentRack();
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
                var Rack = ports.FirstOrDefault().GetParentRack();
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
                        return new(false, ALARMS.EQ_UNLOAD_REQUEST_IS_NOT_ON, $"WIP設備[{Rack.EQName}, ID:{specificport.Properties.ID}] 料座無貨", null, null);
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
                return new(true, ALARMS.NONE, $" GET EQ RACK", Eq, Eq.GetType());
            }
            else
            { return new(false, ALARMS.EQ_TAG_NOT_EXIST_IN_CURRENT_MAP, $"設備站點TAG-{station_tag} 不存在於當前地圖", null, null); }
        }
        public static clsPortOfRack get_empyt_port_of_rack(int _station_tag)
        {
            List<clsPortOfRack> ports = StaEQPManagager.GetRackColumnByTag(_station_tag);
            clsPortOfRack port = ports.Where(x => x.CargoExist == false).OrderBy(x => x.Layer).FirstOrDefault();
            return port == null ? null : port;

        }


        public static (bool confirm, ALARMS alarm_code, string message) CheckEQAcceptAGVType(int station_tag, string _agv_name)
        {
            using AGVSDatabase database = new AGVSDatabase();
            IEnumerable<clsAGVStateDto> agvstates = database.tables.AgvStates;

            clsAGVStateDto? _agv_assigned = agvstates.FirstOrDefault(agv_dat => agv_dat.AGV_Name == _agv_name);
            VEHICLE_TYPE model = _agv_assigned.Model.ConvertToEQAcceptAGVTYPE();
            MapPoint mapPoint = AGVSMapManager.GetMapPointByTag(station_tag);

            clsRack? wip = StaEQPManagager.RacksList.FirstOrDefault(rack => rack.RackOption.ColumnTagMap.Values.SelectMany(k => k).Contains(station_tag));

            bool isWIP = wip != null;
            if (isWIP && model == VEHICLE_TYPE.FORK)
            {
                return (true, ALARMS.NONE, "");
            }
            clsEQ equipment = StaEQPManagager.GetEQByTag(station_tag);
            if (equipment == null)
                return new(false, ALARMS.EQ_TAG_NOT_EXIST_IN_CURRENT_MAP, $"設備站點 [{equipment.EndPointOptions.Name}] TAG-{station_tag} 不存在於當前地圖");
            else
            {
                VEHICLE_TYPE eq_accept_agv_model = equipment.EndPointOptions.Accept_AGV_Type;
                if (eq_accept_agv_model != VEHICLE_TYPE.ALL && eq_accept_agv_model != model)
                    return (false, ALARMS.AGV_Type_Is_Not_Allow_To_Execute_Task_At_Source_Equipment, $"設備 [{equipment.EndPointOptions.Name}] TAG-{station_tag}不允許{model}車種進行任務");
                else
                    return new(true, ALARMS.NONE, "");
            }
        }


        public static void HandleTransferOrderFinish(clsTaskDto TransferOrder)
        {
            KeyValuePair<clsEQ, clsEQ> storedTransferEQPair = UnloadEQQueueing.FirstOrDefault(kp => kp.Key.EndPointOptions.TagID == TransferOrder.From_Station_Tag && kp.Value.EndPointOptions.TagID == TransferOrder.To_Station_Tag);
            if (storedTransferEQPair.Key != null && storedTransferEQPair.Value != null)
            {
                UnloadEQQueueing.Remove(storedTransferEQPair.Key);
            }
        }

        static async Task TransferTaskPairWorker()
        {
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(10);
                    if (SystemModes.RunMode == RUN_MODE.MAINTAIN || SystemModes.TransferTaskMode == TRANSFER_MODE.MANUAL)
                    {
                        UnloadEQQueueing.Clear();
                        AutoRunning = false;
                        continue;
                    }
                    AutoRunning = true;

                    List<string> waitOrRunningSouceEQTagStrList = DatabaseCaches.TaskCaches.InCompletedTasks.Select(task => task.From_Station)
                                                                                                       .ToList();
                    List<clsEQ> unload_req_eq_list = StaEQPManagager.MainEQList.FindAll(eq => eq.lduld_type == EQLDULD_TYPE.ULD || eq.lduld_type == EQLDULD_TYPE.LDULD)
                                                                                .FindAll(eq => eq.IsCreateUnloadTaskAble() && !UnloadEQQueueing.TryGetValue(eq, out clsEQ _eq))
                                                                                .Where(eq => !waitOrRunningSouceEQTagStrList.Contains(eq.EndPointOptions.TagID + ""))
                                                                                .OrderBy(eq => eq.UnloadRequestRaiseTime).ToList();

                    if (unload_req_eq_list.Count > 0)
                    {
                        foreach (clsEQ sourceEQ in unload_req_eq_list)
                        {
                            List<clsEQ> downstreamLoadableEQList = sourceEQ.DownstremEQ.FindAll(downstrem_eq => downstrem_eq.IsCreateLoadTaskAble());
                            List<string> waitOrRunningEQTagStrList = DatabaseCaches.TaskCaches.InCompletedTasks.Select(task => task.To_Station)
                                                                                                                .ToList();
                            downstreamLoadableEQList = downstreamLoadableEQList.Where(item => !waitOrRunningEQTagStrList.Contains(item.EndPointOptions.TagID.ToString())).ToList();

                            if (sourceEQ.EndPointOptions.CheckRackContentStateIOSignal)
                            {
                                downstreamLoadableEQList = downstreamLoadableEQList.Where(downstreamEQ => downstreamEQ.EndPointOptions.IsFullEmptyUnloadAsVirtualInput ? true : (sourceEQ.Full_RACK_To_LDULD == downstreamEQ.Full_RACK_To_LDULD) || (sourceEQ.Empty_RACK_To_LDULD == downstreamEQ.Empty_RACK_To_LDULD))
                                                                                   .ToList();
                            }

                            if (downstreamLoadableEQList.Count == 0)
                                continue;
                            clsEQ destineEQ = downstreamLoadableEQList.OrderBy(eq => _CalculateDistanceFromSourceToDestine(eq, sourceEQ)).First();

                            double _CalculateDistanceFromSourceToDestine(clsEQ destineEQ, clsEQ sourceEQ)
                            {
                                MapPoint destinePt = AGVSMapManager.CurrentMap.Points.Values.FirstOrDefault(pt => pt.TagNumber == destineEQ.EndPointOptions.TagID);
                                MapPoint sourcePt = AGVSMapManager.CurrentMap.Points.Values.FirstOrDefault(pt => pt.TagNumber == sourceEQ.EndPointOptions.TagID);
                                double diffX = destinePt.X - sourcePt.X;
                                double diffY = destinePt.Y - sourcePt.Y;
                                return Math.Sqrt(diffX * diffX + diffY * diffY);
                            }

                            var taskOrder = new clsTaskDto
                            {
                                Action = ACTION_TYPE.Carry,
                                DesignatedAGVName = "",
                                From_Station = sourceEQ.EndPointOptions.TagID.ToString(),
                                To_Station = destineEQ.EndPointOptions.TagID.ToString(),
                                TaskName = $"*Local-{DateTime.Now.ToString("yyyyMMddHHmmssffff")}",
                                DispatcherName = "Local_Auto",
                                From_Slot = sourceEQ.EndPointOptions.Height + "",
                                To_Slot = destineEQ.EndPointOptions.Height + "",
                                Priority = 80,
                                Height = destineEQ.EndPointOptions.Height
                            };
                            var taskAddedResult = await TaskManager.AddTask(taskOrder);
                            if (taskAddedResult.confirm)
                            {
                                UnloadEQQueueing.Add(sourceEQ, destineEQ); ;
                                logger.Info($"[Local Auto EQ Transfer] Task-{taskOrder.TaskName}-(From={taskOrder.From_Station} To={taskOrder.To_Station}>> Execute AGV={taskOrder.DesignatedAGVName}) is added.");
                            }
                            else
                            {
                                logger.Error($"[Local Auto EQ Transfer] Task-{taskOrder.TaskName}-(From={taskOrder.From_Station} To={taskOrder.To_Station}>> Execute AGV={taskOrder.DesignatedAGVName}) add FAILURE,{taskAddedResult.alarm_code}");
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
            });
        }

    }
}
