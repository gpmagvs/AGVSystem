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


                        Dictionary<clsPortOfRack, List<UnloadPortFinder.PortInfoDto>> RackPortUnloadToCandicatesKeyPairs = StaEQPManagager.RackPortsList.Where(port => port.Properties.PortUsable == clsPortOfRack.PORT_USABLE.USABLE)
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
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }

                }
            });

        }

        private bool TryGetRackPortToStore(clsEQ sourceEQ, out clsPortOfRack port)
        {
            port = null;
            if (sourceEQ == null || !sourceEQ.EndPointOptions.ValidDownStreamWIPNames.Any())
                return false;

            HashSet<string> validWipNames = new HashSet<string>(sourceEQ.EndPointOptions.ValidDownStreamWIPNames);

            List<clsPortOfRack> validPorts = StaEQPManagager.RackPortsList.Where(port => validWipNames.Contains(port.GetParentRack().EndPointOptions.Name))
                                                                                                        .Where(port => port.Properties.PortUsable == clsPortOfRack.PORT_USABLE.USABLE)
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

    }
}
