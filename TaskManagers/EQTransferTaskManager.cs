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
            StaEQPManagager.EQList.FindAll(eq => eq.CMD_Reserve_Low | eq.CMD_Reserve_Up).ForEach(eq =>
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
                AutoRunning = true;
                Thread.Sleep(1);
                List<clsEQ> unload_req_eq_list = StaEQPManagager.EQList.FindAll(eq => eq.lduld_type == EQLDULD_TYPE.ULD | eq.lduld_type == EQLDULD_TYPE.LDULD)
                                      .FindAll(eq => eq.Unload_Request && eq.Eqp_Status_Down && !eq.CMD_Reserve_Low && !UnloadEQQueueing.TryGetValue(eq, out clsEQ _eq));

                if (unload_req_eq_list.Count > 0)
                {
                    foreach (clsEQ sourceEQ in unload_req_eq_list)
                    {
                        List<clsEQ> loadable_eqs = sourceEQ.DownstremEQ.FindAll(downstrem_eq => downstrem_eq.Load_Request && downstrem_eq.Eqp_Status_Down && !downstrem_eq.CMD_Reserve_Low);
                        if (loadable_eqs.Count == 0)
                            continue;
                        clsEQ destineEQ = loadable_eqs.First();
                        UnloadEQQueueing.Add(sourceEQ, sourceEQ); ;

                        //var region = AGVSMapManager.MapRegions.FirstOrDefault(reg => reg.RegionName == sourceEQ.EndPointOptions.Region);
                        //if (region == null)
                        //{
                        //    //TODO region issue!!
                        //    continue;
                        //}
                        ////
                        //AGVStatusDBHelper agv_status_db = new AGVStatusDBHelper();
                        //List<clsAGVStateDto> agvlist = agv_status_db.GetALL().FindAll(agv => region.AGVPriorty.Contains(agv.AGV_Name));
                        //if (agvlist.Count == 0)
                        //{
                        //    AlarmManagerCenter.AddAlarm(ALARMS.Region_Has_No_Agv_To_Dispatch_Task, ALARM_SOURCE.AGVS);
                        //    continue;
                        //}
                        //var AGV = agvlist[agvlist.FindIndex(a => a.AGV_Name == region.AGVPriorty[0])];
                        ////if (AGV.OnlineStatus != ONLINE_STATE.ONLINE | AGV.MainStatus == MAIN_STATUS.DOWN)
                        ////{
                        ////    LOG.WARN($"區域-{region.RegionName} 優先指派的AGV({AGV.AGV_Name}) 目前無法執行任務");
                        ////    agvlist.Remove(AGV);
                        ////    AGV = agvlist.FirstOrDefault(agv => agv.MainStatus != MAIN_STATUS.DOWN);
                        ////    if (AGV == null)
                        ////        LOG.WARN($"區域-{region.RegionName} 沒有AGV可執行任務");
                        ////    else
                        ////        LOG.WARN($"區域-{region.RegionName} 指派AGV({AGV.AGV_Name}) 執行任務");
                        ////}

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
                            Priority = 80
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
