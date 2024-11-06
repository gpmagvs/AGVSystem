using AGVSystemCommonNet6.Alarm;
using EquipmentManagment.Device;
using EquipmentManagment.MainEquipment;
using EquipmentManagment.WIP;

namespace AGVSystem.Service.EquipmentStatusCheckServices
{
    public class WorkStationCheckResult
    {
        public ALARMS alarmCode { get; set; } = ALARMS.NONE;

        public string message => $"[{workStationName}] " + GetCheckResultMessage(alarmCode).message;
        public string message_en => $"[{workStationName}] " + GetCheckResultMessage(alarmCode).message_en;

        public EndPointDeviceAbstract station { get; set; }
        public clsPortOfRack rackPort { get; set; }
        public EndPointDeviceAbstract eqAtFisrtSlot { get; set; }
        public string workStationName
        {
            get
            {
                if (station != null)
                    return station.EQName;
                else if (rackPort != null)
                    return rackPort.GetParentRack().EQName + "-" + rackPort.Properties.ID;
                else
                    return "";
            }
        }

        public WorkStationCheckResult(EndPointDeviceAbstract station)
        {
            this.station = station;
        }

        public WorkStationCheckResult(clsPortOfRack rackPort, EndPointDeviceAbstract eqAtFisrtSlot)
        {
            this.rackPort = rackPort;
            this.eqAtFisrtSlot = eqAtFisrtSlot;
        }

        public WorkStationCheckResult()
        {
        }
        protected (string message, string message_en) GetCheckResultMessage(ALARMS alarm)
        {
            switch (alarm)
            {
                case ALARMS.NONE:
                    return ("", "");
                case ALARMS.Station_Disabled:
                    return ("站點已停用", "Station is disabled");
                case ALARMS.Endpoint_EQ_NOT_CONNECTED:
                    return ("站點未連線", "Station is not connected");
                case ALARMS.Source_Eq_Status_Down:
                    return ("來源站點為當機狀態", "Station status error");
                case ALARMS.Destine_Eq_Status_Down:
                    return ("目標站點為當機狀態", "Station status error");
                case ALARMS.EQ_UNLOAD_REQUEST_IS_NOT_ON:
                    return ("站點無出料請求", "Station has no unload request");
                case ALARMS.EQ_LOAD_REQUEST_IS_NOT_ON:
                    return ("站點無入料請求", "Station has no load request");
                case ALARMS.EQ_UNLOAD_REQUEST_ON_BUT_NO_CARGO:
                    return ("站點發起出料請求但無貨物", "Unload Request but no cargo");
                case ALARMS.EQ_LOAD_REQUEST_ON_BUT_HAS_CARGO:
                    return ("站點發起入料請求但有貨物", "Load Request but has cargo");
                case ALARMS.EQ_UNLOAD_REQUEST_ON_BUT_POSE_NOT_UP:
                    return ("站點發起出料請求但撈爪位置非上位", "Unload Requst but Loader Pose Not Up");
                case ALARMS.EQ_LOAD_REQUEST_ON_BUT_POSE_NOT_DOWN:
                    return ("站點發起入料請求但撈爪位置非下位", "Load Requst but Loader Pose Not Down");
                case ALARMS.EQ_CstSteeringMechanism_NOT_DOWN:
                    return ("站點貨物轉向機構位置非下位", "Station Cargo Steering Mechanism Pose Not Down");
                default:
                    return ($"站點狀態錯誤({alarm})", $"Station Status Error({alarm})");
            }
            return ("", "");
        }

        internal bool TryReserveEq(out string errorMessage)
        {
            errorMessage = "";
            try
            {
                (this.station as clsEQ)?.Reserve();
                (this.eqAtFisrtSlot as clsEQ)?.Reserve();
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        internal bool TryToEq(out string errorMessage)
        {
            errorMessage = "";
            try
            {
                (this.station as clsEQ)?.ToEQ();
                (this.eqAtFisrtSlot as clsEQ)?.ToEQ();
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }
    }
}
