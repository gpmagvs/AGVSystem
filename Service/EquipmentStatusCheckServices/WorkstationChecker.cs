using AGVSystemCommonNet6.AGVDispatch.Messages;
using AGVSystemCommonNet6.Alarm;
using EquipmentManagment.MainEquipment;
using EquipmentManagment.WIP;

namespace AGVSystem.Service.EquipmentStatusCheckServices
{
    public abstract class WorkstationChecker
    {
        public clsEQ Eq { get; protected set; }
        public clsEQ EqAtRack { get; protected set; }
        public clsPortOfRack? RackPort { get; protected set; }
        public clsRack Rack => RackPort.GetParentRack();



        public virtual WorkStationCheckResult GetCheckResult(ACTION_TYPE agvAction)
        {
            var result = new WorkStationCheckResult()
            {
                station = Eq,
                rackPort = RackPort,
                eqAtFisrtSlot = EqAtRack
            };
            if (!IsWorkStationEnable())
                result.alarmCode = AGVSystemCommonNet6.Alarm.ALARMS.Station_Disabled;

            if (!IsConnected())
                result.alarmCode = AGVSystemCommonNet6.Alarm.ALARMS.Endpoint_EQ_NOT_CONNECTED;
            return result;
        }
        public abstract bool IsWorkStationEnable();

        public abstract bool IsConnected();

    }
}
