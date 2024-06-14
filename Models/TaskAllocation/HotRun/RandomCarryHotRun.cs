﻿using AGVSystem.TaskManagers;
using AGVSystemCommonNet6;
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
            script.StopFlag = false;
            while (!script.StopFlag)
            {
                await Task.Delay(1000);

                if (TaskUplimitReach())
                    continue;

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
            script.UpdateRealTimeMessage("", false, notification: false);
            await NotifyServiceHelper.INFO($"隨機搬運任務 HOT RUN 已結束");


        }

        private bool TaskUplimitReach()
        {
            int onliningVehicleCnt = DatabaseCaches.Vehicle.VehicleStates.Where(vehicle => vehicle.OnlineStatus == clsEnums.ONLINE_STATE.ONLINE).Count();
            return DatabaseCaches.TaskCaches.InCompletedTasks.Where(t => t.TaskName.Contains("HR_")).Count() >= onliningVehicleCnt;
        }

        private bool TrySelectEquipmentPairTCarray(out int fromTag, out int toTag)
        {
            fromTag = toTag = -1;
            IEnumerable<int> tagsOfAssignedEq = new List<int>();
            var carryTasks = DatabaseCaches.TaskCaches.InCompletedTasks.Where(task => IsEqLDULDTask(task));
            if (carryTasks.Any())
            {
                IEnumerable<MapPoint> assignTaskMapPoints = carryTasks.SelectMany(tk => new List<MapPoint> { tk.To_Station_Tag.GetMapPoint(), tk.From_Station_Tag.GetMapPoint() })
                                                                       .Where(pt => pt != null);

                tagsOfAssignedEq = assignTaskMapPoints.GetTagCollection();
            }

            List<clsEQ> usableEqList = StaEQPManagager.MainEQList.Where(eq => IsEqUnloadable(eq, tagsOfAssignedEq)).ToList();
            Dictionary<clsEQ, IEnumerable<clsEQ>> avalidEQAndDownStreams = usableEqList.ToDictionary(eq => eq, eq => eq.DownstremEQ.Where(_downStrem => !_downStrem.IsMaintaining && !_downStrem.IsAssignedTask()));
            avalidEQAndDownStreams = avalidEQAndDownStreams.Where(pari => pari.Value.Count() != 0)
                                                           .ToDictionary(p => p.Key, p => p.Value.Where(eq => eq.Load_Request));

            if (avalidEQAndDownStreams.Any())
            {
                var avllidUpStreamEqCnt = avalidEQAndDownStreams.Count;
                Random _random = new Random((int)DateTime.Now.Ticks);
                int upStreamRandomIndex = _random.Next(0, avllidUpStreamEqCnt - 1);
                var selectedUpStreamEqPair = avalidEQAndDownStreams.ToList()[upStreamRandomIndex];

                if (!selectedUpStreamEqPair.Value.Any())
                    return false;

                Thread.Sleep(10);
                Random _random2 = new Random((int)DateTime.Now.Ticks);

                int downStreamRandomIndex = _random2.Next(0, selectedUpStreamEqPair.Value.Count() - 1);
                var selectedUpStreamEq = selectedUpStreamEqPair.Key;
                var selectedDownStreamEq = selectedUpStreamEqPair.Value.ToList()[downStreamRandomIndex];

                fromTag = selectedUpStreamEq.EndPointOptions.TagID;
                toTag = selectedDownStreamEq.EndPointOptions.TagID;

                Console.WriteLine($"upStreamRandomIndex:{upStreamRandomIndex} downStreamRandomIndex:{downStreamRandomIndex}");

                return true;
            }
            else
            {
                return false;
            }

            static bool IsEqUnloadable(clsEQ eq, IEnumerable<int> tagsOfAssignedEq)
            {
                return eq.Unload_Request && !eq.IsMaintaining && !tagsOfAssignedEq.Contains(eq.EndPointOptions.TagID);
            }

            static bool IsEqLDULDTask(clsTaskDto task)
            {
                return task.Action == ACTION_TYPE.Carry || task.Action == ACTION_TYPE.Unload || task.Action == ACTION_TYPE.Load;
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
