using AGVSystem.Models.Automation;
using AGVSystem.Models.Map;
using AGVSystem.TaskManagers;
using AGVSystemCommonNet6.AGVDispatch.Messages;
using AGVSystemCommonNet6.AGVDispatch.RunMode;
using AGVSystemCommonNet6.AGVDispatch;
using AGVSystemCommonNet6.Alarm;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.MAP;
using EquipmentManagment.MainEquipment;
using EquipmentManagment.Manager;
using EquipmentManagment.WIP;
using Microsoft.Extensions.Options;
using static AGVSystem.Models.Automation.AutoTransferSettings;
using static EquipmentManagment.MainEquipment.clsEQ;
using AGVSystem.Models.Sys;
using NLog;
using AGVSystemCommonNet6;
using AGVSystem.Models.TaskAllocation.HotRun;
using static AGVSystem.Service.LocalAutomationTransfer.UnloadPortFinder;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace AGVSystem.Service.LocalAutomationTransfer
{
    public class LocalAutomationTransferService : BackgroundService
    {
        public HashSet<clsEQ> WaitingUnloadEQ { get; private set; } = new HashSet<clsEQ>();
        public HashSet<clsEQ> WaitingLoadEQ { get; private set; } = new HashSet<clsEQ>();

        public bool AutoRunning { get; private set; } = false;

        private AutoTransferSetting _settings => settingService.settings;

        private Logger logger = LogManager.GetCurrentClassLogger();

        LocalAutomationTransferSettingService settingService;
        HashSet<PortInfoDto> transferToBufferUnloadPorts = new HashSet<PortInfoDto>();

        public LocalAutomationTransferService(LocalAutomationTransferSettingService settingService)
        {
            this.settingService = settingService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(10);
                    try
                    {
                        if (SystemModes.RunMode == RUN_MODE.MAINTAIN || SystemModes.TransferTaskMode == TRANSFER_MODE.MANUAL)
                        {
                            transferToBufferUnloadPorts.Clear();
                            WaitingLoadEQ.Clear();
                            WaitingUnloadEQ.Clear();
                            AutoRunning = false;
                            continue;
                        }
                        Dictionary<PortInfoDto, List<PortInfoDto>> UnloadToPortCandicates = new Dictionary<PortInfoDto, List<PortInfoDto>>();



                        Dictionary<clsEQ, List<UnloadPortFinder.PortInfoDto>> EqUnloadToCandicatesKeyPairs = StaEQPManagager.MainEQList
                        .Where(eq => eq.IsCreateUnloadTaskAble())
                        .Where(eq => !IsEqOrderRunningAsSourceAndEqNotUnloadDone(eq))
                        .ToDictionary(eq => eq, eq =>
                        {
                            var _unloadPortsFinder = new LoadPortFinder(eq, StaEQPManagager.MainEQList, StaEQPManagager.RackPortsList);
                            return _unloadPortsFinder.FindPorts();
                        });

                        var noLoadPortToUseCandicateKeypairs = EqUnloadToCandicatesKeyPairs.Where(p => p.Key.IsCreateUnloadTaskAble())
                                                                                            .Where(p => !p.Value.Any());

                        foreach (var item in noLoadPortToUseCandicateKeypairs)
                        {
                            if (TryGetRackPortToStore(item.Key, out clsPortOfRack port))
                            {
                                var unloadPort = item.Key;
                                clsTaskDto _toBufferOrder = new clsTaskDto
                                {
                                    Action = ACTION_TYPE.Carry,
                                    TaskName = $"*Local_ToBuffer{DateTime.Now.ToString("yyMMddHHmmssffff")}",
                                    From_Station = unloadPort.EndPointOptions.TagID.ToString(),
                                    From_Slot = unloadPort.EndPointOptions.Height.ToString(),
                                    To_Station = port.TagNumbers.First().ToString(),
                                    To_Slot = port.Properties.Row.ToString(),
                                    DispatcherName = "LocalAuto",
                                    RecieveTime = DateTime.Now,
                                };

                                (bool confirm, ALARMS alarm_code, string message, string message_en) = await TaskManager.AddTask(_toBufferOrder, TaskManager.TASK_RECIEVE_SOURCE.LOCAL_Auto);
                                if (confirm)
                                {
                                    WaitingUnloadEQ.Add(unloadPort);
                                }
                                await Task.Delay(300);
                            }
                        }


                        Dictionary<clsPortOfRack, List<UnloadPortFinder.PortInfoDto>> RackPortUnloadToCandicatesKeyPairs = StaEQPManagager.RackPortsList
                       .Where(port => port.CargoExist && !string.IsNullOrEmpty(port.CarrierID) && port.StoredRackContentType != RACK_CONTENT_STATE.UNKNOWN)
                       .Where(port => !IsRackPortOrderRunningAsSourceAndEqNotUnloadDone(port))
                       .ToDictionary(port => port, port =>
                       {
                           var _unloadPortsFinder = new LoadPortFinder(null, port, StaEQPManagager.MainEQList, StaEQPManagager.RackPortsList);
                           return _unloadPortsFinder.FindPorts();
                       });

                        EqUnloadToCandicatesKeyPairs = EqUnloadToCandicatesKeyPairs.Where(p => p.Value.Any())
                                                                                   .OrderBy(p => p.Key.UnloadRequestRaiseTime)
                                                                                   .ToDictionary(p => p.Key, p => p.Value);

                        foreach (var item in EqUnloadToCandicatesKeyPairs)
                        {
                            UnloadToPortCandicates.Add(new PortInfoDto() { tagNumber = item.Key.EndPointOptions.TagID, slot = item.Key.EndPointOptions.Height, portEntity = item.Key, portType = PORT_TYPE.EQ }, item.Value);
                        }



                        foreach (var item in RackPortUnloadToCandicatesKeyPairs)
                        {
                            UnloadToPortCandicates.Add(new PortInfoDto() { tagNumber = item.Key.TagNumbers.FirstOrDefault(), slot = item.Key.Properties.Row, portEntity = item.Key, portType = PORT_TYPE.WIPPROT }, item.Value);
                        }

                        UnloadToPortCandicates = UnloadToPortCandicates.OrderBy(p => p.Key.startWaitUnloadTime).ToDictionary(p => p.Key, p => p.Value);

                        foreach (var item in UnloadToPortCandicates)
                        {
                            var loadPortInfo = item.Value.Where(i => !i.IsOrderRunning()).FirstOrDefault();
                            if (loadPortInfo == null)
                            {
                                continue;
                            }
                            int sourceTag = item.Key.tagNumber;
                            int sourceSlot = item.Key.slot;

                            clsEQ loadPort = StaEQPManagager.GetEQByTag(loadPortInfo.tagNumber, loadPortInfo.slot);
                            clsTaskDto order = new clsTaskDto
                            {
                                Action = ACTION_TYPE.Carry,
                                TaskName = $"*Local_{DateTime.Now.ToString("yyMMddHHmmssffff")}",
                                From_Station = sourceTag.ToString(),
                                From_Slot = sourceSlot.ToString(),
                                To_Station = loadPortInfo.tagNumber.ToString(),
                                To_Slot = loadPortInfo.slot.ToString(),
                                DispatcherName = "LocalAuto",
                                RecieveTime = DateTime.Now,
                            };
                            (bool confirm, ALARMS alarm_code, string message, string message_en) = await TaskManager.AddTask(order, TaskManager.TASK_RECIEVE_SOURCE.LOCAL_Auto);
                            await Task.Delay(1000);
                        }

                        AutoRunning = true;
                        //Dictionary<clsEQ, List<UnloadPortFinder.PortInfoDto>> eqUnloads = StaEQPManagager.MainEQList.ToDictionary(eq => eq, eq =>
                        //{
                        //    var _unloadPortsFinder = new UnloadPortFinder(eq, StaEQPManagager.MainEQList, StaEQPManagager.RackPortsList);
                        //    return _unloadPortsFinder.FindPorts();
                        //});

                        //eqUnloads = eqUnloads.OrderBy(p => p.Key.LoadRequestRaiseTime).ToDictionary(p => p.Key, p => p.Value);
                        //foreach (var item in eqUnloads)
                        //{
                        //    clsEQ loadPort = item.Key;
                        //    List<UnloadPortFinder.PortInfoDto> unloadPorts = item.Value;
                        //    HashSet<PortInfoDto> unloadPortsSorted = unloadPorts.OrderBy(p => p.startWaitUnloadTime).ToHashSet();
                        //    if (IsLoadPortLoadable(loadPort))
                        //    {
                        //        //找一個產生任務 
                        //        PortInfoDto unloadPort = unloadPortsSorted.FirstOrDefault();
                        //        if (unloadPort != null && !unloadPort.IsOrderRunning())
                        //        {
                        //            clsTaskDto order = new clsTaskDto
                        //            {
                        //                Action = ACTION_TYPE.Carry,
                        //                TaskName = $"*Local_{DateTime.Now.ToString("yyMMddHHmmssffff")}",
                        //                From_Station = unloadPort.tagNumber.ToString(),
                        //                From_Slot = unloadPort.slot.ToString(),
                        //                To_Station = loadPort.EndPointOptions.TagID.ToString(),
                        //                To_Slot = loadPort.EndPointOptions.Height.ToString(),
                        //                DispatcherName = "LocalAuto",
                        //                RecieveTime = DateTime.Now,
                        //            };
                        //            (bool confirm, ALARMS alarm_code, string message, string message_en) = await TaskManager.AddTask(order, TaskManager.TASK_RECIEVE_SOURCE.LOCAL_Auto);
                        //            await Task.Delay(1000);
                        //            if (confirm)
                        //            {
                        //                var toBufferUnPorts = unloadPortsSorted.SkipWhile(p => p == unloadPort);
                        //                foreach (var _toBufferPort in toBufferUnPorts)
                        //                {
                        //                    if (!transferToBufferUnloadPorts.Any(p => p.GetHashCode() == _toBufferPort.GetHashCode()))
                        //                        transferToBufferUnloadPorts.Add(_toBufferPort);
                        //                }
                        //            }
                        //            continue;
                        //        }
                        //    }
                        //    else
                        //    {
                        //        foreach (var _toBufferPort in unloadPortsSorted)
                        //        {
                        //            if (!transferToBufferUnloadPorts.Any(p => p.GetHashCode() == _toBufferPort.GetHashCode()))
                        //                transferToBufferUnloadPorts.Add(_toBufferPort);
                        //        }

                        //    }
                        //}

                        //if (transferToBufferUnloadPorts.Any())
                        //{
                        //    var toBufferPortsList = transferToBufferUnloadPorts.ToList();
                        //    foreach (var unloadPort in toBufferPortsList)
                        //    {
                        //        if (TryGetRackPortToStore(StaEQPManagager.GetEQByTag(unloadPort.tagNumber, unloadPort.slot), out clsPortOfRack rackPort))
                        //        {

                        //            clsTaskDto _toBufferOrder = new clsTaskDto
                        //            {
                        //                Action = ACTION_TYPE.Carry,
                        //                TaskName = $"*Local_ToBuffer{DateTime.Now.ToString("yyMMddHHmmssffff")}",
                        //                From_Station = unloadPort.tagNumber.ToString(),
                        //                From_Slot = unloadPort.slot.ToString(),
                        //                To_Station = rackPort.TagNumbers.First().ToString(),
                        //                To_Slot = rackPort.Properties.Row.ToString(),
                        //                DispatcherName = "LocalAuto",
                        //                RecieveTime = DateTime.Now,
                        //            };

                        //            (bool confirm, ALARMS alarm_code, string message, string message_en) = await TaskManager.AddTask(_toBufferOrder, TaskManager.TASK_RECIEVE_SOURCE.LOCAL_Auto);
                        //            bool removed = transferToBufferUnloadPorts.Remove(unloadPort);
                        //            await Task.Delay(300);
                        //        }
                        //    }
                        //}
                        //await BufferToEQTransportPairWork();
                        //await EqTransportPairWork();

                    }
                    catch (Exception ex)
                    {
                        continue;
                    }

                }
                AutoRunning = false;


                async Task BufferToEQTransportPairWork()
                {
                    try
                    {

                        var hasCargoPorts = StaEQPManagager.RackPortsList.Where(port => port.CargoExist && !string.IsNullOrEmpty(port.CarrierID));
                        var StoredByUpStreamEqPorts = hasCargoPorts.Where(port => port.SourceTag > 0 && port.SourceSlot >= 0);
                        StoredByUpStreamEqPorts = StoredByUpStreamEqPorts.OrderBy(port => port.InstallTime);
                        //所有下游可入料的PORT

                        Dictionary<clsPortOfRack, clsEQ> rackPortToEqPairs = StoredByUpStreamEqPorts.ToDictionary(port => port, port => _GetLoadableDownstreamEq(port))
                                                                                       .Where(pair => pair.Value != null)
                                                                                       .ToDictionary(pair => pair.Key, pair => pair.Value);

                        foreach (var item in rackPortToEqPairs)
                        {
                            clsPortOfRack port = item.Key;
                            clsEQ eq = item.Value;

                            if (WaitingLoadEQ.Contains(eq))
                                continue;

                            int sourceTag = port.TagNumbers.First();
                            int sourceSlot = port.Properties.Row;

                            int destinTag = eq.EndPointOptions.TagID;
                            int destineSlot = eq.EndPointOptions.Height;

                            var wipToEqOrder = new clsTaskDto
                            {
                                TaskName = $"*Local-{DateTime.Now.ToString("yyyyMMddHHmmssffff")}",
                                Action = ACTION_TYPE.Carry,
                                DesignatedAGVName = "",
                                From_Station = sourceTag.ToString(),
                                From_Slot = sourceSlot + "",
                                To_Station = destinTag.ToString(),
                                To_Slot = destineSlot + "",
                                DispatcherName = "Local_Auto",
                                Priority = 80,
                                Height = destineSlot
                            };

                            (bool confirm, ALARMS alarm_code, string message, string message_en) = await TaskManager.AddTask(wipToEqOrder, wipToEqOrder.isFromMCS ? TaskManager.TASK_RECIEVE_SOURCE.REMOTE : TaskManager.TASK_RECIEVE_SOURCE.Local_MANUAL);
                            if (confirm)
                            {
                                WaitingLoadEQ.Add(eq);
                            }

                        }


                        clsEQ _GetLoadableDownstreamEq(clsPortOfRack rackPort)
                        {
                            clsEQ sourceEQ = StaEQPManagager.GetEQByTag(rackPort.SourceTag, rackPort.SourceSlot);
                            var ValidDownStreamEqList = GetValidDownStreamEqList(sourceEQ);
                            return ValidDownStreamEqList.FirstOrDefault();
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }

                async Task EqTransportPairWork()
                {
                    List<clsEQ> unload_req_eq_list = StaEQPManagager.MainEQList.Where(eq => !IsEQDisabled(eq))
                                                                               .Where(eq => eq.lduld_type == EQLDULD_TYPE.ULD || eq.lduld_type == EQLDULD_TYPE.LDULD)
                                                                                .Where(eq => eq.IsCreateUnloadTaskAble() && !WaitingUnloadEQ.TryGetValue(eq, out clsEQ _eq))
                                                                                .Where(eq => !IsEqOrderRunningAsSourceAndEqNotUnloadDone(eq))
                                                                                .OrderBy(eq => eq.UnloadRequestRaiseTime).ToList();
                    bool IsEQDisabled(clsEQ eq)
                    {
                        var mapPt = AGVSMapManager.CurrentMap.Points.Values.FirstOrDefault(pt => pt.TagNumber == eq.EndPointOptions.TagID);
                        if (mapPt == null)
                            return true;
                        return !mapPt.Enable;
                    }
                    if (unload_req_eq_list.Count > 0)
                    {
                        foreach (clsEQ sourceEQ in unload_req_eq_list)
                        {

                            if (WaitingUnloadEQ.Contains(sourceEQ))
                                continue;
                            List<clsEQ> downstreamLoadableEQList = GetValidDownStreamEqList(sourceEQ);

                            if (downstreamLoadableEQList.Count == 0)
                            {
                                if (!TryGetRackPortToStore(sourceEQ, out clsPortOfRack port))
                                    continue;
                                else
                                {
                                    int destinTag = port.TagNumbers.First();
                                    int destineSlot = port.Properties.Row;
                                    var eqToWipOrder = new clsTaskDto
                                    {
                                        Action = ACTION_TYPE.Carry,
                                        DesignatedAGVName = "",
                                        From_Station = sourceEQ.EndPointOptions.TagID.ToString(),
                                        To_Station = destinTag.ToString(),
                                        TaskName = $"*Local-{DateTime.Now.ToString("yyyyMMddHHmmssffff")}",
                                        DispatcherName = "Local_Auto",
                                        From_Slot = sourceEQ.EndPointOptions.Height + "",
                                        To_Slot = destineSlot + "",
                                        Priority = 80,
                                        Height = destineSlot
                                    };
                                    if (!await AddOrderToDatabase(sourceEQ, eqToWipOrder))
                                        await Task.Delay(1000);
                                    continue;
                                }
                            }
                            clsEQ destineEQ = downstreamLoadableEQList.OrderBy(eq => _CalculateDistanceFromSourceToDestine(eq, sourceEQ)).First();


                            var taskOrder = new clsTaskDto
                            {
                                Action = ACTION_TYPE.Carry,
                                DesignatedAGVName = "",
                                From_Station = sourceEQ.EndPointOptions.TagID.ToString(),
                                To_Station = destineEQ.EndPointOptions.TagID.ToString(),
                                TaskName = $"*Local-{DateTime.Now.ToString("yyyyMMddHHmmssffff")}",
                                DispatcherName = "Local_Auto",
                                From_Slot = sourceEQ.EndPointOptions.Height + "",
                                To_Slot = destineEQ.EndPointOptions.Height + "",
                                Priority = 80,
                                Height = destineEQ.EndPointOptions.Height
                            };
                            if (!await AddOrderToDatabase(sourceEQ, taskOrder))
                                await Task.Delay(1000);
                        }

                    }

                }

                double _CalculateDistanceFromSourceToDestine(clsEQ destineEQ, clsEQ sourceEQ)
                {
                    MapPoint destinePt = AGVSMapManager.CurrentMap.Points.Values.FirstOrDefault(pt => pt.TagNumber == destineEQ.EndPointOptions.TagID);
                    MapPoint sourcePt = AGVSMapManager.CurrentMap.Points.Values.FirstOrDefault(pt => pt.TagNumber == sourceEQ.EndPointOptions.TagID);
                    double diffX = destinePt.X - sourcePt.X;
                    double diffY = destinePt.Y - sourcePt.Y;
                    return Math.Sqrt(diffX * diffX + diffY * diffY);
                }

                List<clsEQ> GetValidDownStreamEqList(clsEQ sourceEQ)
                {
                    try
                    {
                        if (sourceEQ == null)
                            return new List<clsEQ>();
                        List<clsEQ> downstreamLoadableEQList = sourceEQ.DownstremEQ.FindAll(downstrem_eq => !WaitingLoadEQ.Contains(downstrem_eq) && downstrem_eq.IsCreateLoadTaskAble());
                        List<string> waitOrRunningEQTagStrList = DatabaseCaches.TaskCaches.InCompletedTasks.Select(task => task.To_Station)
                                                                                                            .ToList();
                        downstreamLoadableEQList = downstreamLoadableEQList.Where(item => !waitOrRunningEQTagStrList.Contains(item.EndPointOptions.TagID.ToString())).ToList();

                        if (sourceEQ.EndPointOptions.CheckRackContentStateIOSignal)
                        {
                            downstreamLoadableEQList = downstreamLoadableEQList.Where(downstreamEQ => downstreamEQ.EndPointOptions.IsFullEmptyUnloadAsVirtualInput ? true : sourceEQ.Full_RACK_To_LDULD == downstreamEQ.Full_RACK_To_LDULD || sourceEQ.Empty_RACK_To_LDULD == downstreamEQ.Empty_RACK_To_LDULD)
                                                                               .ToList();
                        }

                        return downstreamLoadableEQList;

                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }

                async Task<bool> AddOrderToDatabase(clsEQ sourceEQ, clsTaskDto taskOrder)
                {
                    var taskAddedResult = await TaskManager.AddTask(taskOrder, taskOrder.isFromMCS ? TaskManager.TASK_RECIEVE_SOURCE.REMOTE : TaskManager.TASK_RECIEVE_SOURCE.Local_MANUAL);
                    if (taskAddedResult.confirm)
                    {
                        //UnloadEQQueueing.Add(sourceEQ, destineEQ); ;
                        WaitingUnloadEQ.Add(sourceEQ);
                        logger.Info($"[Local Auto EQ Transfer] Task-{taskOrder.TaskName}-(From={taskOrder.From_Station} To={taskOrder.To_Station}>> Execute AGV={taskOrder.DesignatedAGVName}) is added.");
                    }
                    else
                    {
                        logger.Error($"[Local Auto EQ Transfer] Task-{taskOrder.TaskName}-(From={taskOrder.From_Station} To={taskOrder.To_Station}>> Execute AGV={taskOrder.DesignatedAGVName}) add FAILURE,{taskAddedResult.alarm_code}");
                        AlarmManagerCenter.AddAlarmAsync(new clsAlarmDto
                        {
                            Time = DateTime.Now,
                            AlarmCode = (int)taskAddedResult.alarm_code,
                            Description_Zh = taskAddedResult.alarm_code.ToString(),
                            Description_En = taskAddedResult.alarm_code.ToString(),
                            Level = ALARM_LEVEL.ALARM,
                            Task_Name = taskOrder.TaskName,
                            Source = ALARM_SOURCE.AGVS,
                            Equipment_Name = taskOrder.DesignatedAGVName,
                        });
                    }

                    return taskAddedResult.confirm;
                }
            });

        }


        private bool IsLoadPortLoadable(clsEQ loadPort)
        {

            int tag = loadPort.EndPointOptions.TagID;
            int slot = loadPort.EndPointOptions.Height;

            bool isWatingForExecute = DatabaseCaches.TaskCaches.WaitExecuteTasks.Any((order => (order.From_Station_Tag == tag && order.GetFromSlotInt() == slot)
                                                                  || (order.To_Station_Tag == tag && order.GetToSlotInt() == slot)));

            bool isExecutingAndWaitLoadUnload = DatabaseCaches.TaskCaches.RunningTasks.Any((order => (order.From_Station_Tag == tag && order.GetFromSlotInt() == slot && order.currentProgress != VehicleMovementStage.Traveling_To_Source && order.currentProgress != VehicleMovementStage.WorkingAtSource)
                                                                                        || (order.To_Station_Tag == tag && order.GetToSlotInt() == slot)));

            return loadPort.IsCreateLoadTaskAble() && !isWatingForExecute && !isExecutingAndWaitLoadUnload;
        }

        private bool TryGetRackPortToStore(clsEQ sourceEQ, out clsPortOfRack port)
        {
            port = null;
            if (sourceEQ == null || !sourceEQ.EndPointOptions.ValidDownStreamWIPNames.Any())
                return false;

            HashSet<string> validWipNames = new HashSet<string>(sourceEQ.EndPointOptions.ValidDownStreamWIPNames);

            List<clsPortOfRack> validPorts = StaEQPManagager.RackPortsList.Where(port => validWipNames.Contains(port.GetParentRack().EndPointOptions.Name))
                                                                                                        .Where(port => !port.CargoExist && string.IsNullOrEmpty(port.CarrierID))
                                                                                                        .Where(port => !IsRackPortOrderRunningAsSourceAndEqNotUnloadDone(port))
                                                                                                        .ToList();
            //calulate distance and order 
            MapPoint sourceEqMapPt = sourceEQ.EndPointOptions.TagID.GetMapPoint();
            HashSet<clsPortOfRack> validPortsOrderedByDistance = validPorts.OrderBy(port => port.TagNumbers.First().GetMapPoint().CalculateDistance(sourceEqMapPt) * _GetWeightOfRackPortToLoad(port)).ToHashSet();
            port = validPortsOrderedByDistance.FirstOrDefault();
            return port != null;
        }

        private double _GetWeightOfRackPortToLoad(clsPortOfRack portOfRack)
        {
            double _weight = 1;
            int tag = portOfRack.TagNumbers.FirstOrDefault();
            if (IsAnyVehicleParkAtTag(tag))
                _weight = _weight * 1000;
            _weight = _weight * (portOfRack.Properties.Row + 1);
            return _weight;
        }


        private bool IsAnyVehicleParkAtTag(int tag)
        {
            HashSet<clsAGVStateDto> vehicleStates = new HashSet<clsAGVStateDto>(DatabaseCaches.Vehicle.VehicleStates);
            return vehicleStates.Any(vehicle => vehicle.CurrentLocation == tag.ToString());
        }

        /// <summary>
        /// 設備是否有任務進行中:條件 任一訂單包含起點
        /// </summary>
        /// <param name="eq"></param>
        /// <returns></returns>
        private bool IsEqOrderRunningAsSourceAndEqNotUnloadDone(clsEQ eq)
        {

            List<clsTaskDto> list = new();
            list.AddRange(DatabaseCaches.TaskCaches.InCompletedTasks);
            list.AddRange(DatabaseCaches.TaskCaches.RunningTasks);
            var order = list.FirstOrDefault(order => order.From_Station == eq.EndPointOptions.TagID + "" && order.From_Slot == eq.EndPointOptions.Height + "");
            if (order == null || order.currentProgress == VehicleMovementStage.Traveling_To_Destine || order.currentProgress == VehicleMovementStage.WorkingAtDestination)
                return false;
            return true;
        }

        private bool IsRackPortOrderRunningAsSourceAndEqNotUnloadDone(clsPortOfRack rackPort)
        {
            if (rackPort.TagNumbers.Contains(2))
            {

            }
            if (OrderCheck(DatabaseCaches.TaskCaches.InCompletedTasks, rackPort) || OrderCheck(DatabaseCaches.TaskCaches.RunningTasks, rackPort))
                return true;

            return false;

            bool OrderCheck(List<clsTaskDto> taskCollection, clsPortOfRack rackPort)
            {
                var order = taskCollection.FirstOrDefault(order => order.From_Station == rackPort.TagNumbers.FirstOrDefault() + "" && order.From_Slot == rackPort.Properties.Row + "");
                bool isPortAsSource = order != null && (order.currentProgress != VehicleMovementStage.Traveling_To_Destine && order.currentProgress != VehicleMovementStage.WorkingAtDestination);

                //作為終點
                bool isPortAsDestine = null != taskCollection.FirstOrDefault(order => order.To_Station == rackPort.TagNumbers.FirstOrDefault() + "" && order.From_Slot == rackPort.Properties.Row + "");
                return isPortAsSource || isPortAsDestine;
            }

        }

        internal int TryRemoveWaitUnloadEQ(int unloadEq_tag, int unloadEq_slot_height)
        {
            return WaitingUnloadEQ.RemoveWhere(eq => eq.EndPointOptions.TagID == unloadEq_tag && eq.EndPointOptions.Height == unloadEq_slot_height);
        }
        internal int TryRemoveWaitLoadEQ(int unloadEq_tag, int unloadEq_slot_height)
        {
            return WaitingLoadEQ.RemoveWhere(eq => eq.EndPointOptions.TagID == unloadEq_tag && eq.EndPointOptions.Height == unloadEq_slot_height);
        }
    }
}
