using AGVSystem.Models.Map;
using AGVSystem.Models.Sys;
using AGVSystemCommonNet6;
using AGVSystemCommonNet6.AGVDispatch.Messages;
using AGVSystemCommonNet6.Alarm;
using AGVSystemCommonNet6.Alarm.VMS_ALARM;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.HttpHelper;
using AGVSystemCommonNet6.Log;
using AGVSystemCommonNet6.MAP;
using AGVSystemCommonNet6.TASK;
using EquipmentManagment;
using System.Security.AccessControl;
using static AGVSystemCommonNet6.clsEnums;

namespace AGVSystem.TaskManagers
{
    public class TaskAllocator
    {
        public static List<clsTaskDto> TaskList => DatabaseHelper.GetALL();

        public static List<clsTaskDto> InCompletedTaskList => DatabaseHelper.GetALLInCompletedTask();
        public static List<clsTaskDto> CompletedTaskList => DatabaseHelper.GetALLCompletedTask();

        public static TaskDatabaseHelper DatabaseHelper = new TaskDatabaseHelper();

        public enum TASK_RECIEVE_SOURCE
        {
            LOCAL,
            MANUAL,
            REMOTE,
        }

        public class clsExecuteTaskAck
        {
            public bool Confirm { get; set; }
            public clsAGV AGV { get; set; }
            public clsTaskDto taskData { get; set; }
            public class clsAGV
            {
                public AGV_MODEL model { get; set; }
                public string Name { get; set; }
                public ONLINE_STATE online_state { get; set; }
                public MAIN_STATUS main_state { get; }
            }
        }
        public static void Initialize()
        {
            clsEQ.OnEqUnloadRequesting += ClsEQ_OnEqUnloadRequesting;
            SystemModes.OnRunModeON += SwitchToRunMode;
            SystemModes.OnRunModeOFF += SwitchToMaintainMode;
        }

        internal static void SwitchToMaintainMode()
        {
            LOG.WARN("Maintain Mode Start");
        }

        internal static async void SwitchToRunMode()
        {
            LOG.WARN("Run Mode Start");

            //先把目前有Unload Requset的任務跑起來

            var unloadReqEQs = StaEQPManagager.EQPDevices.FindAll(eq => (eq as clsEQ).Unload_Request);
            if (unloadReqEQs.Count == 0)
                return;
            LOG.INFO($"{unloadReqEQs.Count} EQ Unload Task Will Run..");
            foreach (var eq in unloadReqEQs)
            {
                ClsEQ_OnEqUnloadRequesting("Run Mode On", (eq as clsEQ));
                await Task.Delay(1);
            }
        }

        private static void ClsEQ_OnEqUnloadRequesting(object? sender, clsEQ unloadReqEQ)
        {
            if (SystemModes.RunMode == RUN_MODE.MAINTAIN)
            {
                LOG.INFO($"EQ {unloadReqEQ.EQName} Unload_Request But System is in Maintain Mode");
                return;
            }
            Task.Run(async () =>
            {

                var unloadStationTag = unloadReqEQ.EndPointOptions.TagID;
                var nextStationCandicates = unloadReqEQ.EndPointOptions.ValidDownStreamEndPointNames;
                var eqCandicates = StaEQPManagager.EQPDevices.FindAll(eq => nextStationCandicates.Contains(eq.EQName)).FindAll(eq => (eq as clsEQ).Load_Request);
                //找最近的
                if (eqCandicates.Count == 0)
                {
                    //TODO 放到WIP
                    AlarmManagerCenter.AddAlarm(ALARMS.EQ_UNLOAD_REQUEST_IS_NOT_ON);
                    return;
                }
                var distanceMap = AGVSMapManager.CalulateDistanseMap(unloadStationTag, eqCandicates.Select(eq => eq.EndPointOptions.TagID).ToList());
                var index = distanceMap.IndexOf(distanceMap.Min());

                EndPointDeviceAbstract destineEq = eqCandicates[index];
                var region = AGVSMapManager.MapRegions.First(reg => reg.RegionName == unloadReqEQ.EndPointOptions.Region);

                //
                AGVStatusDBHelper agv_status_db = new AGVStatusDBHelper();
                List<clsAGVStateDto> agvlist = agv_status_db.GetALL().FindAll(agv => region.AGVPriorty.Contains(agv.AGV_Name));

                var AGV = agvlist[agvlist.FindIndex(a => a.AGV_Name == region.AGVPriorty[0])];
                if (AGV.OnlineStatus != ONLINE_STATE.ONLINE | AGV.MainStatus == MAIN_STATUS.DOWN)
                {
                    LOG.WARN($"區域-{region.RegionName} 優先指派的AGV({AGV.AGV_Name}) 目前無法執行任務");
                    agvlist.Remove(AGV);
                    AGV = agvlist.FirstOrDefault(agv => agv.OnlineStatus == ONLINE_STATE.ONLINE && agv.MainStatus != MAIN_STATUS.DOWN);
                    LOG.WARN($"區域-{region.RegionName} 指派AGV({AGV.AGV_Name}) 執行任務");
                }
                await AddTask(new clsTaskDto
                {
                    Action = ACTION_TYPE.Carry,
                    Carrier_ID = "123",
                    DesignatedAGVName = AGV.AGV_Name,
                    From_Station = unloadStationTag.ToString(),
                    To_Station = destineEq.EndPointOptions.TagID.ToString(),
                    TaskName = $"*Local-{DateTime.Now.ToString("yyyyMMddHHmmssfff")}",
                    DispatcherName = "Local_Auto",
                    From_Slot = "1",
                    To_Slot = "1"
                });
            });
            //如果是OVEN:最終要搬到接受滿框進的投送板機B(), 若投送板機B非LoadReq =>搬到WIP價
            //如果是空框出的投送板機: 最終要搬到接受空框的投送板機B,, 若投送板機B非LoadReq =>搬到WIP價
            //如果是滿框出的投送板機: 最終要搬到OVEN去烤,, 若沒有OVEN是LoadReq =>搬到WIP價
            //LDULD#1 卸貨 ,只會去 OVEN或WIP
            //LDULD#2 卸貨 ,只會去 LDULD#1或WIP
        }

        public static async Task<Tuple<bool, ALARMS>> AddTask(clsTaskDto taskData, TASK_RECIEVE_SOURCE source = TASK_RECIEVE_SOURCE.LOCAL)
        {
            //if (taskData.Action == ACTION_TYPE.Load | taskData.Action == ACTION_TYPE.LoadAndPark | taskData.Action == ACTION_TYPE.Unload | taskData.Action == ACTION_TYPE.Carry)
            //{
            //    Tuple<bool, ALARMS> results = CheckEQLDULDStatus(taskData.Action, int.Parse(taskData.From_Station), int.Parse(taskData.To_Station));
            //    if (!results.Item1)
            //        return results;
            //}
            try
            {
                //    clsExecuteTaskAck response = await Http.PostAsync<clsTaskDto, clsExecuteTaskAck>($"{AppSettings.VMSHost}/api/VmsManager/ExecuteTask", taskData);
                //    taskData = response.taskData;
                //    taskData.RecieveTime = DateTime.Now;
                //    taskData.State = response.Confirm ? TASK_RUN_STATUS.WAIT : TASK_RUN_STATUS.FAILURE;
                DatabaseHelper.Add(taskData);
                return new(true, ALARMS.NONE);
            }
            catch (HttpRequestException ex)
            {
                AlarmManagerCenter.AddAlarm(ALARMS.TRANSFER_TASK_TO_VMS_BUT_ERROR_OCCUR, ALARM_SOURCE.AGVS);
                return new(false, ALARMS.TRANSFER_TASK_TO_VMS_BUT_ERROR_OCCUR);

            }
        }

        private static Tuple<bool, ALARMS> CheckEQLDULDStatus(ACTION_TYPE action, int from_tag, int to_tag)
        {
            //TODO If To EQ Is WIP

            KeyValuePair<int, MapStation> ToStation = AGVSMapManager.CurrentMap.Points.First(pt => pt.Value.TagNumber == to_tag);


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

        internal static bool Cancel(string task_name)
        {
            return DatabaseHelper.DeleteTask(task_name);
        }
    }
}
