using AGVSystem.Models.Map;
using AGVSystem.TaskManagers;
using AGVSystemCommonNet6;
using AGVSystemCommonNet6.AGVDispatch;
using AGVSystemCommonNet6.AGVDispatch.Messages;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.MAP;
using AGVSystemCommonNet6.Notify;
using EquipmentManagment.Device;
using EquipmentManagment.MainEquipment;
using EquipmentManagment.Manager;
using EquipmentManagment.WIP;

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
                (bool success,TransferEQPairSelectResult result ) = await TrySelectEquipmentPairTCarray();
                if (success)
                {
                    string TaskName = $"HR_{ACTION_TYPE.Carry}_{DateTime.Now.ToString("yMdHHmmss")}";
                    (bool confirm, AGVSystemCommonNet6.Alarm.ALARMS alarm_code, string message) addTaskResult = await TaskManager.AddTask(new clsTaskDto
                    {
                        Action = ACTION_TYPE.Carry,
                        From_Station = result.FromTag.ToString(),
                        To_Station = result.ToTag.ToString(),
                        From_Slot = result.IsFromRack ? "1" : "0",
                        To_Slot = result.IsToRack? "1" : "0",
                        DispatcherName = "Hot_Run",
                        Carrier_ID = $"SIM_{DateTime.Now.ToString("ddHHmmssff")}",
                        TaskName = TaskName,
                        DesignatedAGVName = "",
                        bypass_eq_status_check = true,
                    });

                    if (addTaskResult.confirm)
                    {
                        await NotifyServiceHelper.INFO($"Random Carry Task Created!");
                        MonitorOrderExecutedTimeout(TaskName);
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

        private async Task MonitorOrderExecutedTimeout(string taskName)
        {
            CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            while (!IsOrderExecuting())
            {
                try
                {
                    await Task.Delay(1000, cts.Token);
                }
                catch (TaskCanceledException ex)
                {
                    //cancel the task
                    await TaskManager.Cancel(taskName, reason: "Hot Run Cancel It Because Waiting for Executing Spened Time Too Long");
                    return;
                }
            }

            bool IsOrderExecuting()
            {
                clsTaskDto orderInExecuting = DatabaseCaches.TaskCaches.RunningTasks.FirstOrDefault(tk => tk.TaskName == taskName);
                return orderInExecuting != null;
            }

        }

        private bool TaskUplimitReach()
        {
            int onliningVehicleCnt = DatabaseCaches.Vehicle.VehicleStates.Where(vehicle => vehicle.OnlineStatus == clsEnums.ONLINE_STATE.ONLINE).Count();
            return DatabaseCaches.TaskCaches.InCompletedTasks.Where(t => t.TaskName.Contains("HR_")).Count() >= onliningVehicleCnt + 1;
        }
        private class TransferEQPairSelectResult
        {
            public int FromTag { get; set; }
            public int ToTag { get; set; }
            public bool IsFromRack { get; set; }
            public bool IsToRack { get; set; }

        }
        private async Task<(bool success, TransferEQPairSelectResult result)> TrySelectEquipmentPairTCarray()
        {
            TransferEQPairSelectResult result = new TransferEQPairSelectResult()
            {
                IsFromRack = false,
                IsToRack = false,
                FromTag = -1,
                ToTag = -1
            };

            IEnumerable<int> tagsOfAssignedEq = new List<int>();
            var carryTasks = DatabaseCaches.TaskCaches.InCompletedTasks.Where(task => IsEqLDULDTask(task));
            if (carryTasks.Any())
            {
                IEnumerable<MapPoint> assignTaskMapPoints = carryTasks.SelectMany(tk => new List<MapPoint> { tk.To_Station_Tag.GetMapPoint(), tk.From_Station_Tag.GetMapPoint() })
                                                                       .Where(pt => pt != null);

                tagsOfAssignedEq = assignTaskMapPoints.GetTagCollection();
            }
            List<EndPointDeviceAbstract> usableEqList = new List<EndPointDeviceAbstract>();
            Dictionary<EndPointDeviceAbstract, IEnumerable<EndPointDeviceAbstract>> avalidEQAndDownStreams = new();

            //List<clsRack> usableRackList = StaEQPManagager.RacksList.Where(rack => !tagsOfAssignedEq.Contains(rack.EndPointOptions.TagID)).ToList();
            List<clsEQ> usableMainEqList = StaEQPManagager.MainEQList.Where(eq => IsEqUnloadable(eq, tagsOfAssignedEq)).ToList();

            Dictionary<clsEQ, IEnumerable<clsEQ>> avalidMainEQAndDownStreams = usableMainEqList.ToDictionary(eq => eq, eq => eq.DownstremEQ.Where(_downStrem => !_downStrem.IsMaintaining && !_downStrem.IsAssignedTask()));
            avalidMainEQAndDownStreams = avalidMainEQAndDownStreams.Where(pari => pari.Value.Count() != 0)
                                                                   .ToDictionary(p => p.Key, p => p.Value.Where(eq => eq.Load_Request));

            if (DateTime.Now.Second % 30 == 0)//間隔一段時間才加WIP任務不然如果WIP很多會被WIP的任務塞爆
            {
                foreach (Dictionary<int, int[]>? item in StaEQPManagager.RacksList.Select(rack => rack.RackOption.ColumnTagMap))
                {

                    var downstreamEqs = StaEQPManagager.MainEQList.Where(eq => !tagsOfAssignedEq.Contains(eq.EndPointOptions.TagID))
                                                                  .Where(eq => eq.Load_Request && eq.EndPointOptions.Accept_AGV_Type == EquipmentManagment.Device.Options.VEHICLE_TYPE.FORK)
                                                                  .ToList();

                    foreach (var tags in item.Values)
                    {
                        var tag = tags.First();

                        if (tagsOfAssignedEq.Contains(tag))
                            continue;
                        if (AGVSMapManager.CurrentMap.Points.Values.First(pt => pt.TagNumber == tag).StationType != MapPoint.STATION_TYPE.Buffer)
                            continue;

                        avalidEQAndDownStreams.Add(new clsEQ(new EquipmentManagment.Device.Options.clsEndPointOptions
                        {
                            TagID = tag,
                        }),
                        downstreamEqs
                        );
                    }
                }
            }

            foreach (var item in avalidMainEQAndDownStreams)
            {
                avalidEQAndDownStreams.Add(item.Key, item.Value);
            }


            if (avalidEQAndDownStreams.Any())
            {
                var avllidUpStreamEqCnt = avalidEQAndDownStreams.Count;
                Random _random = new Random((int)DateTime.Now.Ticks);
                int upStreamRandomIndex = _random.Next(0, avllidUpStreamEqCnt - 1);
                var selectedUpStreamEqPair = avalidEQAndDownStreams.ToList()[upStreamRandomIndex];

                if (!selectedUpStreamEqPair.Value.Any())
                {
                    return (false,new ());
                }
                await Task.Delay(120);
                Random _random2 = new Random((int)DateTime.Now.Ticks);

                int downStreamRandomIndex = _random2.Next(0, selectedUpStreamEqPair.Value.Count() - 1);
                EndPointDeviceAbstract selectedUpStreamEq = selectedUpStreamEqPair.Key;
                var selectedDownStreamEq = selectedUpStreamEqPair.Value.ToList()[downStreamRandomIndex];
                
                result.FromTag= selectedUpStreamEq.EndPointOptions.TagID;
                result.ToTag= selectedDownStreamEq.EndPointOptions.TagID;

                int _fromTag = result.FromTag;
                int _toTagg = result.ToTag;

                result.IsFromRack = AGVSMapManager.CurrentMap.Points.Values.First(pt => pt.TagNumber == _fromTag).StationType != MapPoint.STATION_TYPE.EQ;
                result.IsToRack = AGVSMapManager.CurrentMap.Points.Values.First(pt => pt.TagNumber == _toTagg).StationType != MapPoint.STATION_TYPE.EQ;


                Console.WriteLine($"upStreamRandomIndex:{upStreamRandomIndex} downStreamRandomIndex:{downStreamRandomIndex}");

                return (true,result);
            }
            else
            {
                return (false,new());
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
