using AGVSystem.Models.Map;
using AGVSystemCommonNet6.AGVDispatch.Messages;
using AGVSystemCommonNet6.Alarm;
using EquipmentManagment.MainEquipment;

namespace AGVSystem.Service.EquipmentStatusCheckServices
{
    public class EQStationStatusCheckSerivce : WorkstationChecker
    {
        public EQStationStatusCheckSerivce(clsEQ Eq)
        {
            base.Eq = Eq;
        }

        public override WorkStationCheckResult GetCheckResult(ACTION_TYPE agvAction)
        {
            var basicCheckResult = base.GetCheckResult(agvAction);
            if (basicCheckResult.alarmCode != ALARMS.NONE)
            {
                return basicCheckResult;
            }
            var result = new WorkStationCheckResult(Eq);
            string eqName = result.workStationName;

            if (!Eq.Eqp_Status_Down)
            {
                result.alarmCode = agvAction == ACTION_TYPE.Unload ? ALARMS.Source_Eq_Status_Down : ALARMS.Destine_Eq_Status_Down;
                return result;
            }

            //LD_ULD Request Check
            if (agvAction == ACTION_TYPE.Unload && !Eq.Unload_Request)
            {
                result.alarmCode = ALARMS.EQ_UNLOAD_REQUEST_IS_NOT_ON;
                return result;
            }

            if (agvAction == ACTION_TYPE.Load && !Eq.Load_Request)
            {
                result.alarmCode = ALARMS.EQ_LOAD_REQUEST_IS_NOT_ON;
                return result;
            }


            //Port Exist Check
            if (agvAction == ACTION_TYPE.Unload && !Eq.Port_Exist)
            {
                //出料但無貨
                result.alarmCode = ALARMS.EQ_UNLOAD_REQUEST_ON_BUT_NO_CARGO;
                return result;
            }

            if (agvAction == ACTION_TYPE.Load && Eq.Port_Exist)
            {
                //入料但有貨
                result.alarmCode = ALARMS.EQ_LOAD_REQUEST_ON_BUT_HAS_CARGO;
                return result;
            }

            if (Eq.EndPointOptions.HasCstSteeringMechanism && Eq.TB_Down_Pose)
            {
                //升降機構確認
                result.alarmCode = ALARMS.EQ_CstSteeringMechanism_NOT_DOWN;
                return result;
            }

            if (Eq.EndPointOptions.HasLDULDMechanism)
            {  //撈爪機構位置確認
                if (agvAction == ACTION_TYPE.Unload && !Eq.Up_Pose)
                {
                    result.alarmCode = ALARMS.EQ_UNLOAD_REQUEST_ON_BUT_POSE_NOT_UP;
                    return result;
                }
                if (agvAction == ACTION_TYPE.Load && !Eq.Down_Pose)
                {
                    result.alarmCode = ALARMS.EQ_LOAD_REQUEST_ON_BUT_POSE_NOT_DOWN;
                    return result;
                }
            }

            return result;
        }

        public override bool IsWorkStationEnable()
        {
            bool mapPointEnable = Eq.EndPointOptions.TagID.GetMapPoint().Enable;
            return mapPointEnable && Eq.EndPointOptions.Enable;
        }

        public override bool IsConnected()
        {
            return Eq.IsConnected;
        }
    }
}
