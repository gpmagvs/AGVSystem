using AGVSystem.Models.EQDevices;
using AGVSystem.Models.Map;
using AGVSystem.Models.Sys;
using AGVSystem.Models.TaskAllocation.HotRun;
using AGVSystemCommonNet6;
using AGVSystemCommonNet6.AGVDispatch;
using AGVSystemCommonNet6.AGVDispatch.Messages;
using AGVSystemCommonNet6.AGVDispatch.RunMode;
using AGVSystemCommonNet6.Alarm;
using AGVSystemCommonNet6.Configuration;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.MAP;
using EquipmentManagment.Device;
using EquipmentManagment.Device.Options;
using EquipmentManagment.MainEquipment;
using EquipmentManagment.Manager;
using EquipmentManagment.WIP;
using Microsoft.EntityFrameworkCore;
using NLog;
using static AGVSystemCommonNet6.MAP.MapPoint;
using static EquipmentManagment.MainEquipment.clsEQ;

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
        /// <summary>
        /// 儲存自動搬運任務, key=> SourceEQ , value=>DestineEQ
        /// </summary>
        internal static async void SwitchToMaintainMode()
        {
            while (AutoRunning)
            {
                await Task.Delay(1);
            }
            logger.Warn("Maintain Mode Start");
            //取消預約所有機台
            StaEQPManagager.MainEQList.ForEach(eq =>
            {
                eq.CancelReserve();
            });
        }

        internal static async void SwitchToRunMode()
        {
            logger.Warn("Run Mode Start");
        }

        public class DeviceIDInfo
        {
            public string portID { get; set; } = "";
            public string zoneID { get; set; } = "";

            public string carrierIDMounted { get; set; } = string.Empty;

            public RACK_CONTENT_STATE outputRackType { get; set; } = RACK_CONTENT_STATE.UNKNOWN;
        }

        public static (bool confirm, ALARMS alarm_code, string message, string message_en, object obj, Type objtype) CheckLoadUnloadStation(int station_tag, int LayerorSlot, ACTION_TYPE actiontype, out DeviceIDInfo deviceIDInfo, bool check_rack_move_out_is_empty_or_full = true, bool bypasseqandrackckeck = false)
        {
            deviceIDInfo = new DeviceIDInfo();

            if (bypasseqandrackckeck == true)
                return new(true, ALARMS.NONE, "已經 Bypass 設備與RACK狀態檢查", $"By Pass EQ and Rack Check", null, null);
            AGVSystemCommonNet6.MAP.MapPoint MapPoint = AGVSMapManager.GetMapPointByTag(station_tag);
            if (MapPoint == null)
                return new(false, ALARMS.EQ_TAG_NOT_EXIST_IN_CURRENT_MAP, $"站點TAG-{station_tag} 不存在於當前地圖", $"Station Tag {station_tag} not exist on current map", null, null);
            if (!MapPoint.Enable)
                return (false, ALARMS.Station_Disabled, "站點未啟用，無法指派任務", "Station not enabled, can't dispatch any task", null, null);

            if (MapPoint.StationType == STATION_TYPE.EQ || MapPoint.StationType == STATION_TYPE.EQ_LD || MapPoint.StationType == STATION_TYPE.EQ_ULD ||
                (MapPoint.StationType == STATION_TYPE.Buffer_EQ && LayerorSlot == 0))
            {
                clsEQ? Eq = StaEQPManagager.MainEQList.FirstOrDefault(eq => eq.EndPointOptions.TagID == station_tag && eq.EndPointOptions.Height == LayerorSlot);
                if (Eq == null)
                    return new(false, ALARMS.EQ_TAG_NOT_EXIST_IN_CURRENT_MAP, $"設備站點TAG-{station_tag},EQ不存在於當前地圖", $"Tag of EQ Station({station_tag}) not exist on current map", null, null);
                deviceIDInfo.portID = Eq.EndPointOptions.DeviceID;
                deviceIDInfo.carrierIDMounted = Eq.PortStatus.CarrierID;
                deviceIDInfo.outputRackType = Eq.EndPointOptions.SpeficRackContentOutput;
                if (Eq.EndPointOptions.IsRoleAsZone && TryGetZoneIDOfEqLocateing(Eq.EndPointOptions.TagID, out string zoneID))
                    deviceIDInfo.zoneID = zoneID;
                logger.Trace($"AGV請求[{actiontype}]於設備-{MapPoint.Graph.Display}(Tag={station_tag}),設備狀態=>\r\n{Eq.GetStatusDescription()}");

                if (!Eq.IsConnected)
                    return new(false, ALARMS.Endpoint_EQ_NOT_CONNECTED, $"設備[{Eq.EQName}] 尚未連線,無法確認狀態", $"EQ {Eq.EQName} is disconnecting ,can't check status", null, null);
                //if (!Eq.IS_EQ_STATUS_NORMAL_IDLE)
                //{
                //    string description = Eq.EndPointOptions.IOLocation.STATUS_IO_SPEC_VERSION == clsEQIOLocation.STATUS_IO_DEFINED_VERSION.V1 ? "EQP_STATUS_IDLE 訊號未ON" : "EQP_STATUS_DOWN 訊號未ON";
                //    return new(false, ALARMS.PortStatusisWrongCannottLoadUnload, $"設備[{Eq.EQName}] 狀態錯誤[{description}]", null, null);
                //}
                if (actiontype == ACTION_TYPE.Unload)
                {
                    if (Eq.Unload_Request == false)
                        return new(false, ALARMS.EQ_UNLOAD_REQUEST_IS_NOT_ON, $"設備[{Eq.EQName}] 沒有[出料]請求", $"EQ {Eq.EQName} not raised [Unload] request", null, null);
                    if (Eq.Port_Exist == false)
                        return new(false, ALARMS.EQ_UNLOAD_REQUEST_ON_BUT_NO_CARGO, $"設備[{Eq.EQName}] PORT內無貨物，無法載出", $"No cargo at EQ {Eq.EQName},can't execute Unload task", null, null);
                    if (Eq.EndPointOptions.HasLDULDMechanism && Eq.Up_Pose == false)
                        return new(false, ALARMS.EQ_UNLOAD_REQUEST_ON_BUT_POSE_NOT_UP, $"設備[{Eq.EQName}] Up_Pose=false", $"Pose of loader at EQ {Eq.EQName} is not UP", null, null);

                    if (check_rack_move_out_is_empty_or_full && Eq.EndPointOptions.CheckRackContentStateIOSignal && Eq.Is_RACK_HAS_TRAY_OR_NOT_TO_LDULD_Unknown)
                        return new(false, ALARMS.EQ_UNLOAD_REQ_BUT_RACK_FULL_OR_EMPTY_IS_UNKNOWN, $"設備[{Eq.EQName}] 無法確定要載出空框或實框", $"EQ {Eq.EQName} can't confirm rack is empty or full to unload", null, null);
                }
                else if (actiontype == ACTION_TYPE.Load)
                {
                    if (Eq.Load_Request == false)
                        return new(false, ALARMS.EQ_LOAD_REQUEST_IS_NOT_ON, $"設備[{Eq.EQName}] 沒有[入料]請求", $"EQ {Eq.EQName} not raised [Load] request", null, null);
                    if (Eq.Port_Exist == true)
                        return new(false, ALARMS.EQ_LOAD_REQUEST_ON_BUT_HAS_CARGO, $"設備[{Eq.EQName}] 內有貨物，無法載入", $"Cargo exist at EQ {Eq.EQName},can't execute Load task", null, null);
                    if (Eq.EndPointOptions.HasLDULDMechanism && Eq.Down_Pose == false)
                        return new(false, ALARMS.EQ_LOAD_REQUEST_ON_BUT_POSE_NOT_DOWN, $"設備[{Eq.EQName}] Down_Pose=false", $"Pose of loader at EQ {Eq.EQName} is not DOWN", null, null);
                    //if (check_rack_move_out_is_empty_or_full && Eq.EndPointOptions.CheckRackContentStateIOSignal && Eq.Is_RACK_HAS_TRAY_OR_NOT_TO_LDULD_Unknown)
                    //    return new(false, ALARMS.EQ_LOAD_REQ_BUT_RACK_FULL_OR_EMPTY_IS_UNKNOWN, $"設備[{Eq.EQName}] 無法確定要載入空框或實框", null, null);
                }
                ////檢查貨物轉向機構位置狀態(例如平對平設備)
                if (Eq.EndPointOptions.HasCstSteeringMechanism && Eq.TB_Down_Pose != true)
                {
                    return new(false, ALARMS.EQ_LOAD_REQUEST_ON_BUT_POSE_NOT_DOWN, $"設備[{Eq.EQName}] 貨物轉向機構位置非位於低位", $"Pose of Steering Mechanism at EQ {Eq.EQName} is not DOWN", null, null);
                }

                return new(true, ALARMS.NONE, $"GET EQ", "", Eq, Eq.GetType());
            }
            else if (MapPoint.StationType == STATION_TYPE.Buffer || MapPoint.StationType == STATION_TYPE.Charge_Buffer)
            {
                var Eq = StaEQPManagager.MainEQList.FirstOrDefault(eq => eq.EndPointOptions.TagID == station_tag);
                if (Eq != null)
                {
                    deviceIDInfo.portID = Eq.EndPointOptions.DeviceID;
                    deviceIDInfo.carrierIDMounted = Eq.PortStatus.CarrierID;
                    deviceIDInfo.outputRackType = Eq.EndPointOptions.SpeficRackContentOutput;
                    if (!Eq.IsConnected)
                        return new(false, ALARMS.Endpoint_EQ_NOT_CONNECTED, $"設備[{Eq.EQName}] 尚未連線,無法確認狀態", $"EQ {Eq.EQName} is not connected, can't confirm status", null, null);
                    if (actiontype == ACTION_TYPE.Unload)
                    {
                        if (Eq.Unload_Request == false)
                            return new(false, ALARMS.EQ_UNLOAD_REQUEST_IS_NOT_ON, $"設備[{Eq.EQName}] 沒有[出料]請求", $"EQ {Eq.EQName} has no [Unload] request", null, null);
                        if (Eq.Port_Exist == false)
                            return new(false, ALARMS.EQ_UNLOAD_REQUEST_ON_BUT_NO_CARGO, $"設備[{Eq.EQName}] PORT內無貨物，無法載出", $"No cargo at EQ {Eq.EQName}, can't unload", null, null);
                        if (Eq.Up_Pose == false)
                            return new(false, ALARMS.EQ_UNLOAD_REQUEST_ON_BUT_POSE_NOT_UP, $"設備[{Eq.EQName}] Up_Pose=false", $"Pose of EQ {Eq.EQName} is not UP", null, null);
                        if (check_rack_move_out_is_empty_or_full && Eq.EndPointOptions.CheckRackContentStateIOSignal && Eq.Is_RACK_HAS_TRAY_OR_NOT_TO_LDULD_Unknown)
                            return new(false, ALARMS.EQ_UNLOAD_REQ_BUT_RACK_FULL_OR_EMPTY_IS_UNKNOWN, $"設備[{Eq.EQName}] 無法確定要載出空框或實框", $"EQ {Eq.EQName} can't confirm rack is empty or full to unload", null, null);
                    }
                    else if (actiontype == ACTION_TYPE.Load)
                    {
                        if (Eq.Load_Request == false)
                            return new(false, ALARMS.EQ_LOAD_REQUEST_IS_NOT_ON, $"設備[{Eq.EQName}] 沒有[入料]請求", $"EQ {Eq.EQName} has no [Load] request", null, null);
                        if (Eq.Port_Exist == true)
                            return new(false, ALARMS.EQ_LOAD_REQUEST_ON_BUT_HAS_CARGO, $"設備[{Eq.EQName}] 內有貨物，無法載入", $"Cargo exists at EQ {Eq.EQName}, can't load", null, null);
                        if (Eq.Down_Pose == false)
                            return new(false, ALARMS.EQ_LOAD_REQUEST_ON_BUT_POSE_NOT_DOWN, $"設備[{Eq.EQName}] Down_Pose=false", $"Pose of EQ {Eq.EQName} is not DOWN", null, null);
                        //if (check_rack_move_out_is_empty_or_full && Eq.EndPointOptions.CheckRackContentStateIOSignal && Eq.Is_RACK_HAS_TRAY_OR_NOT_TO_LDULD_Unknown)
                        //    return new(false, ALARMS.EQ_LOAD_REQ_BUT_RACK_FULL_OR_EMPTY_IS_UNKNOWN, $"設備[{Eq.EQName}] 無法確定要載入空框或實框", null, null);
                    }
                    return new(true, ALARMS.NONE, $"GET EQ", $"GET EQ", Eq, Eq.GetType());
                }
                else
                {
                    List<clsPortOfRack> ports = StaEQPManagager.GetRackColumnByTag(station_tag);
                    if (ports.Count <= 0)
                        return new(false, ALARMS.EQ_TAG_NOT_EXIST_IN_CURRENT_MAP, $"WIP站點TAG-{station_tag}, 無port存在於當前地圖", $"WIP station tag {station_tag} has no port on current map", null, null);
                    var Rack = ports.FirstOrDefault().GetParentRack();
                    if (Rack == null)
                        return new(false, ALARMS.EQ_TAG_NOT_EXIST_IN_CURRENT_MAP, $"WIP站點TAG-{station_tag},EQ-{Rack.EQName} 不存在於當前地圖", $"WIP station tag {station_tag}, EQ {Rack.EQName} not exist on current map", null, null);
                    if (Rack.IsConnected == false)
                        return new(false, ALARMS.Endpoint_EQ_NOT_CONNECTED, $"WIP [{Rack.EQName}] 尚未連線,無法確認狀態", $"WIP {Rack.EQName} is not connected, can't confirm status", null, null);
                    clsPortOfRack specificport = ports.Where(x => x.Layer == LayerorSlot).FirstOrDefault();
                    if (specificport == null)
                        return new(false, ALARMS.EQ_LOAD_REQUEST_IS_NOT_ON, $"WIP設備[{Rack.EQName}, ID:{specificport.Properties.ID}] 料座不存在", $"WIP EQ {Rack.EQName}, ID:{specificport.Properties.ID} port does not exist", null, null);

                    deviceIDInfo.zoneID = Rack.EndPointOptions.DeviceID;
                    deviceIDInfo.portID = specificport.GetLocID();
                    deviceIDInfo.carrierIDMounted = specificport.CarrierID;
                    deviceIDInfo.outputRackType = specificport.StoredRackContentType;

                    if (actiontype == ACTION_TYPE.Unload)
                    {
                        if (specificport.CargoExist == false)
                            return new(false, ALARMS.SourceRackPortNoCargo, $"WIP設備[{Rack.EQName}, ID:{specificport.Properties.ID}] 料座無貨", $"WIP EQ {Rack.EQName}, ID:{specificport.Properties.ID} port has no cargo", null, null);
                    }
                    else if (actiontype == ACTION_TYPE.Load || actiontype == ACTION_TYPE.LoadAndPark)
                    {
                        if (specificport.CargoExist == true)
                            return new(false, ALARMS.DestineRackPortHasCargo, $"WIP設備[{Rack.EQName}, ID:{specificport.Properties.ID}] 料座已占用", $"WIP EQ {Rack.EQName}, ID:{specificport.Properties.ID} port is occupied", null, null);
                    }
                    return new(true, ALARMS.NONE, $" GET RACK", $"GET RACK", specificport, specificport.GetType());
                }
            }
            else if (MapPoint.StationType == STATION_TYPE.Buffer_EQ && LayerorSlot >= 1) // Buffer_EQ slot >=1 先確認WIP儲位但還是要預約EQ訊號
            {
                List<clsPortOfRack> ports = StaEQPManagager.GetRackColumnByTag(station_tag);
                var Rack = ports.FirstOrDefault().GetParentRack();
                if (Rack == null)
                    return new(false, ALARMS.EQ_TAG_NOT_EXIST_IN_CURRENT_MAP, $"WIP站點TAG-{station_tag},EQ-{Rack.EQName} 不存在於當前地圖", $"WIP station tag {station_tag}, EQ {Rack.EQName} not exist on current map", null, null);

                clsPortOfRack specificport = ports.Where(x => x.Layer == LayerorSlot).FirstOrDefault();

                if (Rack.IsConnected == false)
                    return new(false, ALARMS.Endpoint_EQ_NOT_CONNECTED, $"WIP [{Rack.EQName}] 尚未連線,無法確認狀態", $"WIP {Rack.EQName} is not connected, can't confirm status", null, null);
                if (specificport == null)
                    return new(false, ALARMS.EQ_LOAD_REQUEST_IS_NOT_ON, $"WIP設備[{Rack.EQName}, ID:{specificport.Properties.ID}] 料座不存在", $"WIP EQ {Rack.EQName}, ID:{specificport.Properties.ID} port does not exist", null, null);

                deviceIDInfo.zoneID = Rack.EndPointOptions.DeviceID;
                deviceIDInfo.portID = specificport.GetLocID();
                deviceIDInfo.carrierIDMounted = specificport.CarrierID;

                if (actiontype == ACTION_TYPE.Unload)
                {
                    if (specificport.CargoExist == false)
                        return new(false, ALARMS.SourceRackPortNoCargo, $"WIP設備[{Rack.EQName}, ID:{specificport.Properties.ID}] 料座無貨", $"WIP EQ {Rack.EQName}, ID:{specificport.Properties.ID} port has no cargo", null, null);
                }
                else if (actiontype == ACTION_TYPE.Load || actiontype == ACTION_TYPE.LoadAndPark)
                {
                    if (specificport.CargoExist == true)
                        return new(false, ALARMS.DestineRackPortHasCargo, $"WIP設備[{Rack.EQName}, ID:{specificport.Properties.ID}] 料座已占用", $"WIP EQ {Rack.EQName}, ID:{specificport.Properties.ID} port is occupied", null, null);
                }
                var Eq = StaEQPManagager.MainEQList.FirstOrDefault(eq => eq.EndPointOptions.TagID == station_tag);
                if (Eq == null)
                    return new(false, ALARMS.EQ_TAG_NOT_EXIST_IN_CURRENT_MAP, $"設備站點TAG-{station_tag},EQ-{Eq.EQName} 不存在於當前地圖", $"EQ station tag {station_tag}, EQ {Eq.EQName} not exist on current map", null, null);
                if (!Eq.IsConnected)
                    return new(false, ALARMS.Endpoint_EQ_NOT_CONNECTED, $"設備[{Eq.EQName}] 尚未連線,無法確認狀態", $"EQ {Eq.EQName} is not connected, can't confirm status", null, null);
                return new(true, ALARMS.NONE, $" GET EQ RACK", $"GET EQ RACK", Eq, Eq.GetType());
            }
            else
            {
                return new(false, ALARMS.EQ_TAG_NOT_EXIST_IN_CURRENT_MAP, $"設備站點TAG-{station_tag} 不存在於當前地圖", $"EQ station tag {station_tag} not exist on current map", null, null);
            }
        }

        private static bool TryGetZoneIDOfEqLocateing(int eqTag, out string zoneID)
        {
            zoneID = "";
            clsEQ? Eq = StaEQPManagager.MainEQList.FirstOrDefault(eq => eq.EndPointOptions.TagID == eqTag);
            if (Eq == null)
                return false;
            clsRack rack = StaEQPManagager.RacksList.FirstOrDefault(rack => rack.RackOption.ColumnTagMap.SelectMany(tgs => tgs.Value).Contains(eqTag));
            if (rack == null)
                return false;
            zoneID = rack.EndPointOptions.DeviceID;
            return true;
        }

        public static clsPortOfRack get_empyt_port_of_rack(int _station_tag)
        {
            List<clsPortOfRack> ports = StaEQPManagager.GetRackColumnByTag(_station_tag);
            clsPortOfRack port = ports.Where(x => x.CargoExist == false).OrderBy(x => x.Layer).FirstOrDefault();
            return port == null ? null : port;

        }


        public static (bool confirm, ALARMS alarm_code, string message, string message_en) CheckEQAcceptAGVType(int station_tag, int slot, string _agv_name, bool isNeedChangeAGV)
        {
            using AGVSDatabase database = new AGVSDatabase();
            IEnumerable<clsAGVStateDto> agvstates = database.tables.AgvStates;

            clsAGVStateDto? _agv_assigned = agvstates.FirstOrDefault(agv_dat => agv_dat.AGV_Name == _agv_name);
            VEHICLE_TYPE model = _agv_assigned.Model.ConvertToEQAcceptAGVTYPE();
            MapPoint mapPoint = AGVSMapManager.GetMapPointByTag(station_tag);

            (bool confirm, ALARMS alarm_code, string message, string message_en) mapPointTagNotExistReturn = new(false, ALARMS.EQ_TAG_NOT_EXIST_IN_CURRENT_MAP, $"設備站點 TAG-{station_tag} 不存在於當前地圖", $"EQ TAG-{station_tag} not exist on current map");

            if (mapPoint == null)
                return mapPointTagNotExistReturn;

            EndPointDeviceAbstract device = null;

            bool isStationWIP = mapPoint.StationType == STATION_TYPE.Buffer || mapPoint.StationType == STATION_TYPE.Charge_Buffer || (mapPoint.StationType == STATION_TYPE.Buffer_EQ && slot > 0);
            if (isStationWIP)
                device = StaEQPManagager.RacksList.FirstOrDefault(rack => rack.RackOption.ColumnTagMap.Values.SelectMany(k => k).Contains(station_tag));
            else
                device = StaEQPManagager.GetEQByTag(station_tag, slot);

            if (device == null)
                return mapPointTagNotExistReturn;

            (bool confirm, ALARMS alarm_code, string message, string message_en) vehicleModelNotAcceptReturn = new(false, ALARMS.AGV_Type_Is_Not_Allow_To_Execute_Task_At_Source_Equipment, $"設備 [{device.EndPointOptions.Name}] TAG-{station_tag}不允許{model}車種進行任務", $"EQ [{device.EndPointOptions.Name}] (TAG-{station_tag}) not allowed {model} to execute task");

            if (isNeedChangeAGV)
                return (true, ALARMS.NONE, "", "");

            if ((isStationWIP || slot > 0) && model == VEHICLE_TYPE.SUBMERGED_SHIELD)
                return vehicleModelNotAcceptReturn;

            bool isAcceptVehicleModelMatch = device.EndPointOptions.Accept_AGV_Type == VEHICLE_TYPE.ALL || device.EndPointOptions.Accept_AGV_Type == model;

            if (!isAcceptVehicleModelMatch)
                return vehicleModelNotAcceptReturn;
            else
                return (true, ALARMS.NONE, "", "");

        }

    }
}
