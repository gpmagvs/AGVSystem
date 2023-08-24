using AGVSystem.Models.Map;
using AGVSystem.Models.Sys;
using AGVSystemCommonNet6.AGVDispatch.Messages;
using AGVSystemCommonNet6.Alarm;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.TASK;
using AGVSystemCommonNet6;
using EquipmentManagment;
using static AGVSystemCommonNet6.clsEnums;
using AGVSystemCommonNet6.Log;
using AGVSystemCommonNet6.MAP;
using AGVSystemCommonNet6.DATABASE.Helpers;
using AGVSystemCommonNet6.AGVDispatch.RunMode;

namespace AGVSystem.TaskManagers
{
    public class EQTransferTaskManager
    {
        public static void Initialize()
        {
            //clsEQ.OnEqUnloadRequesting += ClsEQ_OnEqUnloadRequesting;
            SystemModes.OnRunModeON += SwitchToRunMode;
            SystemModes.OnRunModeOFF += SwitchToMaintainMode;
        }

        internal static void SwitchToMaintainMode()
        {
            LOG.WARN("Maintain Mode Start");
            //取消預約所有機台

            StaEQPManagager.EQList.FindAll(eq => eq.CMD_Reserve_Low | eq.CMD_Reserve_Up).ForEach(eq =>
            {
                eq.CancelReserve();
            });
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
                Thread.Sleep(1);
                List<clsEQ> unload_req_eq_list = StaEQPManagager.EQList.FindAll(eq => eq.lduld_type == EQLDULD_TYPE.ULD | eq.lduld_type == EQLDULD_TYPE.LDULD)
                                      .FindAll(eq => eq.Unload_Request && eq.Eqp_Status_Down && !eq.CMD_Reserve_Low);

                if (unload_req_eq_list.Count > 0)
                {
                    foreach (clsEQ sourceEQ in unload_req_eq_list)
                    {
                        List<clsEQ> loadable_eqs = sourceEQ.DownstremEQ.FindAll(downstrem_eq => downstrem_eq.Load_Request && downstrem_eq.Eqp_Status_Down && !downstrem_eq.CMD_Reserve_Low);
                        //依距離排序?
                        if (loadable_eqs.Count == 0)
                            continue;
                        clsEQ destineEQ = loadable_eqs.First();
                        destineEQ.ReserveLow();
                        sourceEQ.ReserveLow();

                        var region = AGVSMapManager.MapRegions.First(reg => reg.RegionName == sourceEQ.EndPointOptions.Region);
                        //
                        AGVStatusDBHelper agv_status_db = new AGVStatusDBHelper();
                        List<clsAGVStateDto> agvlist = agv_status_db.GetALL().FindAll(agv => region.AGVPriorty.Contains(agv.AGV_Name));

                        var AGV = agvlist[agvlist.FindIndex(a => a.AGV_Name == region.AGVPriorty[0])];
                        //if (AGV.OnlineStatus != ONLINE_STATE.ONLINE | AGV.MainStatus == MAIN_STATUS.DOWN)
                        //{
                        //    LOG.WARN($"區域-{region.RegionName} 優先指派的AGV({AGV.AGV_Name}) 目前無法執行任務");
                        //    agvlist.Remove(AGV);
                        //    AGV = agvlist.FirstOrDefault(agv => agv.MainStatus != MAIN_STATUS.DOWN);
                        //    if (AGV == null)
                        //        LOG.WARN($"區域-{region.RegionName} 沒有AGV可執行任務");
                        //    else
                        //        LOG.WARN($"區域-{region.RegionName} 指派AGV({AGV.AGV_Name}) 執行任務");
                        //}
                        await TaskManager.AddTask(new clsTaskDto
                        {
                            Action = ACTION_TYPE.Carry,
                            Carrier_ID = "123",
                            DesignatedAGVName = AGV.AGV_Name,
                            From_Station = sourceEQ.EndPointOptions.TagID.ToString(),
                            To_Station = destineEQ.EndPointOptions.TagID.ToString(),
                            TaskName = $"*Local-{DateTime.Now.ToString("yyyyMMddHHmmssfff")}",
                            DispatcherName = "Local_Auto",
                            From_Slot = "1",
                            To_Slot = "1"
                        });


                    }

                }

            }
        }

        public static Tuple<bool, ALARMS> CheckEQLDULDStatus(ACTION_TYPE action, int from_tag, int to_tag)
        {
            //TODO If To EQ Is WIP
            KeyValuePair<int, MapPoint> ToStation = AGVSMapManager.CurrentMap.Points.First(pt => pt.Value.TagNumber == to_tag);
            EQStatusDIDto ToEQStatus = StaEQPManagager.GetEQStatesByTagID(to_tag);
            EQStatusDIDto FromEQStatus = StaEQPManagager.GetEQStatesByTagID(from_tag);

            if (ToEQStatus != null)
            {
                if (!ToEQStatus.IsConnected)
                    return new(false, ALARMS.Endpoint_EQ_NOT_CONNECTED);

                if (action == ACTION_TYPE.Load | action == ACTION_TYPE.LoadAndPark)
                {
                    return new(ToEQStatus.Load_Reuest, ALARMS.EQ_LOAD_REQUEST_IS_NOT_ON);
                }
                else if (action == ACTION_TYPE.Carry)
                {
                    if (!FromEQStatus.Unload_Request)
                        return new(false, ALARMS.EQ_UNLOAD_REQUEST_IS_NOT_ON);
                    if (!ToEQStatus.Load_Reuest)
                        return new(false, ALARMS.EQ_LOAD_REQUEST_IS_NOT_ON);

                    return new(true, ALARMS.NONE);
                }
                else
                {
                    return new(ToEQStatus.Unload_Request, ALARMS.EQ_UNLOAD_REQUEST_IS_NOT_ON);
                }
            }
            else
            {
                if (ToStation.Value.StationType == STATION_TYPE.STK)
                    return new(true, ALARMS.NONE);
                else
                    return new(false, ALARMS.Endpoint_EQ_NOT_CONNECTED);
            }
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
