using AGVSystem.Models.Map;
using AGVSystemCommonNet6.AGVDispatch;
using AGVSystemCommonNet6.AGVDispatch.Messages;
using AGVSystemCommonNet6.MAP;
using EquipmentManagment.MainEquipment;
using EquipmentManagment.Manager;
using EquipmentManagment.WIP;
using static AGVSystemCommonNet6.MAP.MapPoint;

namespace AGVSystem.Service.EquipmentStatusCheckServices
{
    /// <summary>
    /// 工作站狀態檢查服務，用於檢查工作站的狀態是否正常可取放貨(包含主設備、RACK PORT與充電站)
    /// </summary>
    public class WorkStationStatusCheckService
    {

        public WorkStationStatusCheckService()
        {
        }

        public (WorkStationCheckResult? fromStationCheckResult, WorkStationCheckResult? toStationCheckResult) CheckWorkStationStatus(clsTaskDto orderInfo, ACTION_TYPE agvAction)
        {
            if (orderInfo == null)
            {
                throw new ArgumentNullException(nameof(orderInfo));
            }

            if (orderInfo.Action != ACTION_TYPE.Unload && orderInfo.Action != ACTION_TYPE.Load && orderInfo.Action != ACTION_TYPE.Carry)
            {
                return (new WorkStationCheckResult(), new WorkStationCheckResult());
            }

            int fromSlot = int.Parse(orderInfo.From_Slot); //取貨高度
            int toSlot = int.Parse(orderInfo.To_Slot); //放貨高度

            MapPoint fromWorkStationPt = orderInfo.From_Station_Tag.GetMapPoint();
            MapPoint toWorkStationPt = orderInfo.To_Station_Tag.GetMapPoint();

            WorkstationChecker fromStationChecker = GetWorkStationChecker(fromWorkStationPt, fromSlot);
            WorkstationChecker toStationChecker = GetWorkStationChecker(toWorkStationPt, toSlot);


            WorkStationCheckResult fromStationCheckResult = fromStationChecker?.GetCheckResult(ACTION_TYPE.Unload);
            WorkStationCheckResult toStationCheckResult = toStationChecker?.GetCheckResult(orderInfo.Action == ACTION_TYPE.Carry ? ACTION_TYPE.Load : orderInfo.Action);

            return (fromStationCheckResult, toStationCheckResult);
            //get the work station type
            //from_station => 來源站點 , 當 orderInfo.Action = ACTION_TYPE.Carry , 使用 from_station 判斷要去哪個站點取貨
            //to_station => 目的站點 , 當 orderInfo.Action = ACTION_TYPE.Load , 使用 to_station 判斷要去哪個站點放貨
        }


        private WorkstationChecker GetWorkStationChecker(MapPoint stationPoint, int slot)
        {
            if (stationPoint == null)
                return null;
            STATION_TYPE stationType = stationPoint.StationType;

            bool isEQStation = stationType == STATION_TYPE.EQ || stationType == STATION_TYPE.EQ_LD || stationType == STATION_TYPE.EQ_ULD ||
                               (stationType == STATION_TYPE.Buffer_EQ && slot == 0) || stationType == STATION_TYPE.Elevator || stationType == STATION_TYPE.Elevator_LD
                               || stationType == STATION_TYPE.STK || stationType == STATION_TYPE.STK_ULD || stationType == STATION_TYPE.STK_LD;

            if (isEQStation)
            {
                clsEQ eq = StaEQPManagager.GetEQByTag(stationPoint.TagNumber, slot);
                return new EQStationStatusCheckSerivce(eq);
            }
            else
            {

                List<clsPortOfRack> rackPorts = StaEQPManagager.GetRackColumnByTag(stationPoint.TagNumber);
                clsPortOfRack rackPort = rackPorts.FirstOrDefault(p => p.Properties.Row == slot);
                clsEQ firstSlotEq = null;
                if (stationType == STATION_TYPE.Buffer_EQ)
                {
                    firstSlotEq = StaEQPManagager.GetEQByTag(stationPoint.TagNumber, 0);

                }
                return new RackStationStatusCheckService(rackPort, firstSlotEq);
            }
        }

    }

}
