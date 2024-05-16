using AGVSystem.TaskManagers;
using AGVSystemCommonNet6.AGVDispatch;
using AGVSystemCommonNet6.AGVDispatch.Messages;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.MAP;
using AGVSystemCommonNet6.Notify;
using EquipmentManagment.MainEquipment;
using EquipmentManagment.Manager;

namespace AGVSystem.Models.TaskAllocation.HotRun
{
    public class RandomCarryHotRun
    {
        private HotRunScript script;

        public RandomCarryHotRun(HotRunScript script)
        {
            this.script = script;
        }

        public async Task StartAsync()
        {
            script.state = "Running";

            await NotifyServiceHelper.INFO($"隨機搬運任務HOT RUN 開始!");

            while (!script.StopFlag)
            {
                await Task.Delay(1000);
                if (TrySelectEquipmentPairTCarray(out int fromTag, out int toTag))
                {
                    string TaskName = $"HR_{ACTION_TYPE.Carry}_{DateTime.Now.ToString("yMdHHmmss")}";
                    (bool confirm, AGVSystemCommonNet6.Alarm.ALARMS alarm_code, string message) addTaskResult = await TaskManager.AddTask(new clsTaskDto
                    {
                        Action = ACTION_TYPE.Carry,
                        From_Station = fromTag.ToString(),
                        To_Station = toTag.ToString(),
                        From_Slot = "0",
                        To_Slot = "0",
                        DispatcherName = "Hot_Run",
                        Carrier_ID = $"SIM_{DateTime.Now.ToString("ddHHmmssff")}",
                        TaskName = TaskName,
                        DesignatedAGVName = "",
                        bypass_eq_status_check = true,
                    });

                    if (addTaskResult.confirm)
                    {
                        await NotifyServiceHelper.INFO($"Random Carry Task Created!");
                    }
                    else
                    {
                        script.UpdateRealTimeMessage(addTaskResult.message, false);
                    }
                }
                else
                {
                    //script.UpdateRealTimeMessage("等待有可搬運的設備...", false);
                }
            }
            script.state = "IDLE";
        }

        private bool TrySelectEquipmentPairTCarray(out int fromTag, out int toTag)
        {
            fromTag = toTag = -1;
            IEnumerable<int> tagsOfAssignedEq = new List<int>();
            var carryTasks = DatabaseCaches.TaskCaches.InCompletedTasks.Where(task => task.Action == ACTION_TYPE.Carry || task.Action == ACTION_TYPE.Unload || task.Action == ACTION_TYPE.Load);
            if (carryTasks.Any())
            {
                IEnumerable<MapPoint> assignTaskMapPoints = carryTasks.SelectMany(tk => new List<MapPoint> { tk.To_Station_Tag.GetMapPoint(), tk.From_Station_Tag.GetMapPoint() })
                                                                       .Where(pt => pt != null);
                tagsOfAssignedEq = assignTaskMapPoints.GetTagCollection();

            }

            List<EquipmentManagment.MainEquipment.clsEQ> usableEqList = StaEQPManagager.MainEQList.Where(eq => !tagsOfAssignedEq.Contains(eq.EndPointOptions.TagID)).ToList();
            Dictionary<clsEQ, IEnumerable<clsEQ>> avalidEQAndDownStreams = usableEqList.ToDictionary(eq => eq, eq => eq.DownstremEQ.Where(_downStrem => !_downStrem.IsAssignedTask()));
            avalidEQAndDownStreams = avalidEQAndDownStreams.Where(pari => pari.Value.Count() != 0)
                                                           .ToDictionary(p => p.Key, p => p.Value);

            if (avalidEQAndDownStreams.Any())
            {
                var avllidUpStreamEqCnt = avalidEQAndDownStreams.Count;
                Random _random = new Random(DateTime.Now.Second);
                int upStreamRandomIndex = _random.Next(0, avllidUpStreamEqCnt - 1);
                var selectedUpStreamEqPair = avalidEQAndDownStreams.ToList()[upStreamRandomIndex];

                Random _random2 = new Random(DateTime.Now.Second);
                int downStreamRandomIndex = _random2.Next(0, selectedUpStreamEqPair.Value.Count() - 1);
                var selectedUpStreamEq = selectedUpStreamEqPair.Key;
                var selectedDownStreamEq = selectedUpStreamEqPair.Value.ToList()[downStreamRandomIndex];

                fromTag = selectedUpStreamEq.EndPointOptions.TagID;
                toTag = selectedDownStreamEq.EndPointOptions.TagID;
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public static class Extensions
    {
        public static MapPoint? GetMapPoint(this int Tag)
        {
            return Map.AGVSMapManager.CurrentMap.Points.Values.FirstOrDefault(pt => pt.TagNumber == Tag);
        }

        public static bool IsAssignedTask(this clsEQ eQ)
        {
            int eqTagID = eQ.EndPointOptions.TagID;
            return DatabaseCaches.TaskCaches.InCompletedTasks.Any(tk => tk.From_Station_Tag == eqTagID || tk.To_Station_Tag == eqTagID);
        }
    }
}
