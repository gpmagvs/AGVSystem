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
        protected HotRunScript script;

        public RandomCarryHotRun(HotRunScript script)
        {
            this.script = script;
        }

        public virtual async Task StartAsync()
        {
            script.state = "Running";

            await NotifyServiceHelper.INFO($"隨機搬運任務HOT RUN 開始!");
            script.StopFlag = false;
            await HotRun();
            script.state = "IDLE";
            script.UpdateRealTimeMessage("", false, notification: false);
            await NotifyServiceHelper.INFO($"隨機搬運任務 HOT RUN 已結束");


        }

        protected virtual async Task HotRun()
        {
            while (!script.StopFlag)
            {
                await Task.Delay(1000);

                if (TaskUplimitReach())
                    continue;
                (bool success, TransferEQPairSelectResult result) = await TrySelectEquipmentPairTCarray(true, false);
                if (success)
                {
                    string TaskName = $"HR_{ACTION_TYPE.Carry}_{DateTime.Now.ToString("yMdHHmmssffff")}";
                    (bool confirm, AGVSystemCommonNet6.Alarm.ALARMS alarm_code, string message, string message_en) addTaskResult = await TaskManager.AddTask(new clsTaskDto
                    {
                        Action = ACTION_TYPE.Carry,
                        From_Station = result.FromTag.ToString(),
                        To_Station = result.ToTag.ToString(),
                        From_Slot = result.FromSlot + "",
                        To_Slot = result.ToSlot + "",
                        DispatcherName = "Hot_Run",
                        Carrier_ID = $"SIM_{DateTime.Now.ToString("ddHHmmssffff")}",
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

                await Task.Delay(200);

                (bool success2, TransferEQPairSelectResult result2) = await TrySelectEquipmentPairTCarray(false, true);
                if (success2)
                {
                    string TaskName = $"HR_{ACTION_TYPE.Carry}_{DateTime.Now.ToString("yMdHHmmssffff")}";
                    (bool confirm, AGVSystemCommonNet6.Alarm.ALARMS alarm_code, string message, string message_en) addTaskResult = await TaskManager.AddTask(new clsTaskDto
                    {
                        Action = ACTION_TYPE.Carry,
                        From_Station = result2.FromTag.ToString(),
                        To_Station = result2.ToTag.ToString(),
                        From_Slot = result2.FromSlot + "",
                        To_Slot = result2.ToSlot + "",
                        DispatcherName = "Hot_Run",
                        Carrier_ID = $"SIM_{DateTime.Now.ToString("ddHHmmssffff")}",
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
            }
        }

        private async Task MonitorOrderExecutedTimeout(string taskName)
        {
            CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));

            while (!IsOrderExecuting())
            {
                try
                {
                    await Task.Delay(100, cts.Token);
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
            public int FromSlot { get; set; } = 0;
            public int ToTag { get; set; }
            public int ToSlot { get; set; } = 0;
            public bool IsFromRack { get; set; }
            public bool IsToRack { get; set; }

        }
        private async Task<(bool success, TransferEQPairSelectResult result)> TrySelectEquipmentPairTCarray(bool onlyRackAsSource, bool onlyEqAsSource)
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

            if (onlyRackAsSource && !onlyEqAsSource)
            {
                var hasCargoPorts = StaEQPManagager.RacksList.SelectMany(rack => script.RandomHotRunSettings.IsRackPortNeedHasCargoAcutally ? rack.PortsStatus.Where(port => port.CargoExist) : rack.PortsStatus)
                                                             .Where(port => IsPortBelongPureBufferType(port))
                                                             .Where(port => script.RandomHotRunSettings.IsOnlyUseRackFirstLayer ? port.Layer == 0 : true)
                                                             .Where(port => !IsPortAssignedOrder(port, tagsOfAssignedEq)).ToList();
                if (hasCargoPorts.Any())
                {
                    Random _random = new Random((int)DateTime.Now.Ticks);
                    int randomPortIndex = _random.Next(0, hasCargoPorts.Count() - 1);
                    clsPortOfRack port = hasCargoPorts[randomPortIndex];

                    string wipName = port.GetParentRack().EQName;
                    List<int> constrainEqTags = new List<int>();
                    if (this.script.RandomHotRunSettings.RacksUpDownStarems.TryGetValue(wipName, out RandomHotRunConfiguration.RackUpDownStream updownStream_settings))
                    {
                        constrainEqTags.AddRange(updownStream_settings.DownStream);
                    }

                    var loadableEqPorts = StaEQPManagager.MainEQList.Where(eq => constrainEqTags.Contains(eq.EndPointOptions.TagID) && !IsEqBufferType(eq) && IsEqLoadable(eq, tagsOfAssignedEq)).ToList();
                    if (loadableEqPorts.Any())
                    {
                        _random = new Random((int)DateTime.Now.Ticks);
                        int downStreamRandomIndex = _random.Next(0, loadableEqPorts.Count() - 1);
                        clsEQ _loadToEqPort = loadableEqPorts[downStreamRandomIndex];
                        result.IsFromRack = true;
                        result.IsToRack = false;
                        result.FromTag = port.TagNumbers.First();
                        result.FromSlot = port.Layer;
                        result.ToTag = _loadToEqPort.EndPointOptions.TagID;
                        result.ToSlot = 0;
                        return (true, result);
                    }
                }
                else
                    return (false, null);

            }
            else if (!onlyRackAsSource && onlyEqAsSource)
            {
                if (avalidEQAndDownStreams.Any())
                {
                    var avllidUpStreamEqCnt = avalidEQAndDownStreams.Count;
                    Random _random = new Random((int)DateTime.Now.Ticks);
                    int upStreamRandomIndex = _random.Next(0, avllidUpStreamEqCnt - 1);
                    var selectedUpStreamEqPair = avalidEQAndDownStreams.ToList()[upStreamRandomIndex];

                    if (!selectedUpStreamEqPair.Value.Any())
                    {
                        return (false, new());
                    }
                    await Task.Delay(120);

                    if (script.RandomHotRunSettings.IsMainEqUnloadTransferToRackOnly)
                    {

                        clsEQ _eq = selectedUpStreamEqPair.Key as clsEQ;

                        var usableRacksNames = script.RandomHotRunSettings.RacksUpDownStarems.Where(pair => pair.Value.UpStream.Contains(_eq.EndPointOptions.TagID))
                                                                                                .Select(pair => pair.Key).ToList();
                        var noCargoPorts = StaEQPManagager.RacksList.Where(rack => usableRacksNames.Contains(rack.EQName))
                                                                    .SelectMany(rack => rack.PortsStatus.Where(port => !port.CargoExist))
                                                                    .Where(port => IsPortBelongPureBufferType(port))
                                                                    .Where(port => script.RandomHotRunSettings.IsOnlyUseRackFirstLayer ? port.Layer == 0 : true)
                                                                    .Where(port => !IsPortAssignedOrder(port, tagsOfAssignedEq)).ToList();
                        if (!noCargoPorts.Any())
                            return (false, null);

                        _random = new Random((int)DateTime.Now.Ticks);
                        int randomPortIndex = _random.Next(0, noCargoPorts.Count() - 1);
                        clsPortOfRack port = noCargoPorts[randomPortIndex];

                        result.FromTag = selectedUpStreamEqPair.Key.EndPointOptions.TagID;
                        result.ToTag = port.TagNumbers.First();
                        result.ToSlot = port.Layer;
                        result.IsFromRack = false;
                        result.IsToRack = true;

                        return (true, result);
                    }


                    Random _random2 = new Random((int)DateTime.Now.Ticks);

                    int downStreamRandomIndex = _random2.Next(0, selectedUpStreamEqPair.Value.Count() - 1);
                    EndPointDeviceAbstract selectedUpStreamEq = selectedUpStreamEqPair.Key;
                    var selectedDownStreamEq = selectedUpStreamEqPair.Value.ToList()[downStreamRandomIndex];

                    result.FromTag = selectedUpStreamEq.EndPointOptions.TagID;
                    result.ToTag = selectedDownStreamEq.EndPointOptions.TagID;

                    int _fromTag = result.FromTag;
                    int _toTagg = result.ToTag;

                    result.IsFromRack = AGVSMapManager.CurrentMap.Points.Values.First(pt => pt.TagNumber == _fromTag).StationType != MapPoint.STATION_TYPE.EQ;
                    result.IsToRack = AGVSMapManager.CurrentMap.Points.Values.First(pt => pt.TagNumber == _toTagg).StationType != MapPoint.STATION_TYPE.EQ;


                    Console.WriteLine($"upStreamRandomIndex:{upStreamRandomIndex} downStreamRandomIndex:{downStreamRandomIndex}");

                    return (true, result);
                }
                else
                {
                    return (false, new());
                }

            }

            return (false, null);



            static bool IsPortBelongPureBufferType(clsPortOfRack port)
            {
                return AGVSMapManager.GetBufferStations().Where(st => st.StationType == MapPoint.STATION_TYPE.Buffer || st.StationType == MapPoint.STATION_TYPE.Charge_Buffer).GetTagCollection().Contains(port.TagNumbers.FirstOrDefault());
            }

            static bool IsEqBufferType(clsEQ eq)
            {
                return AGVSMapManager.GetBufferStations().GetTagCollection().Contains(eq.EndPointOptions.TagID);
            }

            static bool IsEqUnloadable(clsEQ eq, IEnumerable<int> tagsOfAssignedEq)
            {
                return eq.Unload_Request && !eq.IsMaintaining && !tagsOfAssignedEq.Contains(eq.EndPointOptions.TagID);
            }

            //Load Request On and not assign order
            static bool IsEqLoadable(clsEQ eq, IEnumerable<int> tagsOfAssignedEq)
            {
                return eq.Load_Request && !eq.IsMaintaining && !tagsOfAssignedEq.Contains(eq.EndPointOptions.TagID);
            }

            static bool IsEqLDULDTask(clsTaskDto task)
            {
                return task.Action == ACTION_TYPE.Carry || task.Action == ACTION_TYPE.Unload || task.Action == ACTION_TYPE.Load;
            }
            static bool IsPortAssignedOrder(clsPortOfRack port, IEnumerable<int> tagsOfAssignedEq)
            {
                int portTag = port.TagNumbers.FirstOrDefault();
                int portLayer = port.Layer;
                string _Fromkey = $"From_Tag_{portTag}_From_Slot_{portLayer}";
                string _Tokey = $"To_Tag_{portTag}_To_Slot_{portLayer}";

                List<string> orderAssignDestineKeys = DatabaseCaches.TaskCaches.InCompletedTasks.SelectMany(order => new string[] {
                                                        $"From_Tag_{order.From_Station}_From_Slot_{order.From_Slot}",
                                                        $"To_Tag_{order.To_Station}_To_Slot_{order.To_Slot}"
                                                      }).ToList();
                return orderAssignDestineKeys.Contains(_Fromkey) || orderAssignDestineKeys.Contains(_Tokey);
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
