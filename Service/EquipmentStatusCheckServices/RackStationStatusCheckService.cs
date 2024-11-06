using AGVSystem.Models.Map;
using AGVSystemCommonNet6.AGVDispatch.Messages;
using AGVSystemCommonNet6.Alarm;
using EquipmentManagment.MainEquipment;
using EquipmentManagment.WIP;

namespace AGVSystem.Service.EquipmentStatusCheckServices
{
    public class RackStationStatusCheckService : WorkstationChecker
    {
        public RackStationStatusCheckService(clsPortOfRack? rackPort, clsEQ eqAtRack)
        {
            RackPort = rackPort;
            EqAtRack = eqAtRack;
        }


        public override WorkStationCheckResult GetCheckResult(ACTION_TYPE agvAction)
        {
            WorkStationCheckResult basicCheckResult = base.GetCheckResult(agvAction);
            if (basicCheckResult.alarmCode != ALARMS.NONE)
            {
                return basicCheckResult;
            }

            WorkStationCheckResult result = new(RackPort, EqAtRack);
            if (EqAtRack != null)
            {
                WorkStationCheckResult eqCheckResult = CheckFisrtSlotEqStatus(agvAction);
                if (eqCheckResult.alarmCode == ALARMS.Destine_Eq_Status_Down || eqCheckResult.alarmCode == ALARMS.Source_Eq_Status_Down)
                {
                    result.alarmCode = eqCheckResult.alarmCode;
                    return result;
                }
            }

            if (agvAction == ACTION_TYPE.Load && RackPort.CargoExist)
            {
                result.alarmCode = ALARMS.EQ_LOAD_REQUEST_ON_BUT_HAS_CARGO;
                return result;
            }

            if (agvAction == ACTION_TYPE.Unload && !RackPort.CargoExist)
            {
                result.alarmCode = ALARMS.EQ_UNLOAD_REQUEST_ON_BUT_NO_CARGO;
                return result;
            }

            return result;
        }



        public override bool IsConnected()
        {
            return Rack.IsConnected;
        }

        public override bool IsWorkStationEnable()
        {
            bool mapPtEnable = RackPort.TagNumbers.All(tag => tag.GetMapPoint().Enable);
            bool rackEnabled = Rack.EndPointOptions.Enable;
            bool portEnabled = RackPort.Properties.PortEnable == clsPortOfRack.Port_Enable.Enable;
            return mapPtEnable && rackEnabled && portEnabled;
        }

        /// <summary>
        /// 檢查在第一層的設備狀態(如果需要)
        /// </summary>
        /// <returns></returns>
        private WorkStationCheckResult CheckFisrtSlotEqStatus(ACTION_TYPE agvAction)
        {
            EQStationStatusCheckSerivce checker = new EQStationStatusCheckSerivce(EqAtRack);
            return checker.GetCheckResult(agvAction);
        }
    }
}
