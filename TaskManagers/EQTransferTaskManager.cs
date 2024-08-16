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
using NLog;

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
        }
        public static Dictionary<clsEQ, clsEQ> UnloadEQQueueing { get; private set; } = new Dictionary<clsEQ, clsEQ>();
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
                clsEQ? Eq = StaEQPManagager.MainEQList.FirstOrDefault(eq => eq.EndPointOptions.TagID == station_tag);
                if (Eq == null)
                    return new(false, ALARMS.EQ_TAG_NOT_EXIST_IN_CURRENT_MAP, $"設備站點TAG-{station_tag},EQ不存在於當前地圖", null, null);

                logger.Trace($"AGV請求[{actiontype}]於設備-{MapPoint.Graph.Display}(Tag={station_tag}),設備狀態=>\r\n{Eq.GetStatusDescription()}");

                if (!Eq.IsConnected)
                    return new(false, ALARMS.Endpoint_EQ_NOT_CONNECTED, $"設備[{Eq.EQName}] 尚未連線,無法確認狀態", null, null);
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
                if (Eq.EndPointOptions.HasCstSteeringMechanism && Eq.TB_Down_Pose != false)
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
                return new(false, ALARMS.EQ_TAG_NOT_EXIST_IN_CURRENT_MAP, $"設備站點TAG-{station_tag} 不存在於當前地圖");
            else
            {
                VEHICLE_TYPE eq_accept_agv_model = equipment.EndPointOptions.Accept_AGV_Type;
                if (eq_accept_agv_model != VEHICLE_TYPE.ALL && eq_accept_agv_model != model)
                    return (false, ALARMS.AGV_Type_Is_Not_Allow_To_Execute_Task_At_Source_Equipment, $"設備TAG-{station_tag}不允許{model}車種進行任務");
                else
                    return new(true, ALARMS.NONE, "");
            }
        }

    }
}
