
using AGVSystem.Models.EQDevices;
using AGVSystem.Models.TaskAllocation.HotRun;
using AGVSystem.TaskManagers;
using AGVSystemCommonNet6.Alarm;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.Microservices.MCS;
using EquipmentManagment.Device;
using EquipmentManagment.MainEquipment;
using EquipmentManagment.Manager;
using EquipmentManagment.WIP;
using System.Linq;
using static AGVSystemCommonNet6.Microservices.MCS.MCSCIMService;

namespace AGVSystem.Service
{

    public class HasIDbutNoCargoException : Exception
    {
        public HasIDbutNoCargoException() : base() { }
        public HasIDbutNoCargoException(string message) : base(message)
        {

        }
    }

    public class AddOrderFailException : Exception
    {
        public readonly ALARMS alarmCode;
        public AddOrderFailException(string message, ALARMS alarmCode) : base(message)
        {
            this.alarmCode = alarmCode;
        }
    }
    public class MCSService
    {
        List<clsPortOfRack> ALLRackPorts => StaEQPManagager.RacksList.SelectMany(rack => rack.PortsStatus).ToList();
        List<clsRack> ALLRack => StaEQPManagager.RacksList.ToList();

        public MCSService()
        {
        }

        internal async Task HandleTransportCommand(clsTransportCommandDto transportCommand)
        {
            try
            {
                bool _isSourceAGV = false;
                string sourceAGVName = "";
                int sourceTag = -1;
                int destineTag = -1;
                int sourceSlot = 0;
                int destineSlot = 0;
                _isSourceAGV = isSourceAGV(transportCommand.source, out sourceAGVName);
                if (!_isSourceAGV)
                {
                    if (isDeviceIDBelongRack(transportCommand.source))
                    {
                        TryGetRackPort(transportCommand.source, asDestine: false, transportCommand.carrierID, out clsPortOfRack? sourceRackPort);
                        sourceTag = sourceRackPort == null ? -1 : sourceRackPort.TagNumbers.FirstOrDefault();
                        sourceSlot = sourceRackPort == null ? -1 : sourceRackPort.Layer;
                    }
                    else
                    {
                        TryGetEQPort(transportCommand.source, transportCommand.carrierID, isAsSource: true, out EndPointDeviceAbstract soucePort);
                        sourceTag = soucePort == null ? -1 : soucePort.EndPointOptions.TagID;
                    }
                }
                if (isDeviceIDBelongRack(transportCommand.dest))
                {
                    TryGetRackPort(transportCommand.dest, asDestine: true, transportCommand.carrierID, out clsPortOfRack? destineackPort);
                    destineTag = destineackPort == null ? -1 : destineackPort.TagNumbers.FirstOrDefault();
                    destineSlot = destineackPort == null ? -1 : destineackPort.Layer;

                }
                else
                {
                    TryGetEQPort(transportCommand.dest, transportCommand.carrierID, isAsSource: false, out EndPointDeviceAbstract destinePort);
                    destineTag = destinePort == null ? -1 : destinePort.EndPointOptions.TagID;
                }

                if (!_isSourceAGV && (sourceTag == -1 || destineTag == -1))
                    return;

                Console.WriteLine($"Created Order: From [Tag {sourceTag}_Slot {sourceSlot}] TO  [Tag {destineTag}_Slot {destineSlot}]");

                (bool confirm, ALARMS alarm_code, string message, string message_en) = await TaskManager.AddTask(new AGVSystemCommonNet6.AGVDispatch.clsTaskDto
                {
                    Action = AGVSystemCommonNet6.AGVDispatch.Messages.ACTION_TYPE.Carry,
                    TaskName = transportCommand.commandID,
                    Carrier_ID = transportCommand.carrierID,
                    From_Station = _isSourceAGV ? sourceAGVName : sourceTag + "",
                    To_Station = destineTag + "",
                    From_Slot = sourceSlot + "",
                    To_Slot = destineSlot + "",
                    DesignatedAGVName = _isSourceAGV ? sourceAGVName : "",
                    Priority = transportCommand.priority,
                    RecieveTime = DateTime.Now,
                    bypass_eq_status_check = false,
                    isFromMCS = true,
                    DispatcherName = "MCS"
                }, TaskManager.TASK_RECIEVE_SOURCE.REMOTE);

                if (!confirm)
                {
                    throw new AddOrderFailException(message, alarm_code);
                }
            }
            catch (HasIDbutNoCargoException ex)
            {
                throw ex;
            }
        }


        private TransportCommandDto GetCommandInfo(string commandID)
        {
            return new TransportCommandDto()
            {
                CommandID = commandID,
            };
        }

        private bool TryGetEQPort(string deviceID, string carrierID, bool isAsSource, out EndPointDeviceAbstract port)
        {
            port = StaEQPManagager.MainEQList.FirstOrDefault(eq => eq.EndPointOptions.DeviceID.Contains(deviceID));
            if (port != null && isAsSource && (port as clsEQ).PortStatus.CarrierID == carrierID && !(port as clsEQ).Port_Exist)
            {
                throw new HasIDbutNoCargoException($"[{port.EQName}] 無貨物");
            }

            return port != null;
        }
        private bool TryGetRackPort(string deviceID, bool asDestine, string carrierID, out clsPortOfRack? port)
        {
            port = null;
            clsRack rack = ALLRack.FirstOrDefault(rack => rack.RackOption.DeviceID == deviceID);
            if (rack == null)
                return false;

            if (asDestine)
            {//Rack 是目的地[放貨], 找空的Port

                //考慮多座Rack有相同 DeviceID 
                IEnumerable<clsRack> racks = ALLRack.Where(rack => rack.RackOption.DeviceID == deviceID);
                List<clsPortOfRack> allPorts = new List<clsPortOfRack>();
                for (int row = 0; row < 3; row++)
                {
                    foreach (var _rack in racks)
                    {
                        allPorts.AddRange(_rack.PortsStatus.Where(p => p.Layer == row).ToList());
                    }
                }

                allPorts = allPorts.OrderByDescending(p => p.TagNumbers.All(tag => tag.GetMapPoint().StationType == AGVSystemCommonNet6.MAP.MapPoint.STATION_TYPE.Buffer))
                                   .ToList();
                //Filter : 不可以是轉換架,不可以有貨物,不可以有被指派任務
                port = allPorts.Where(p => NotTransferStationPort(p) && !p.CargoExist && !HasOrderAssigned(p)).FirstOrDefault(); //TODO 可以更優化，找PORT的邏輯 , 比如從最低層開始找
            }
            else
            {
                //Rack 是來源地[取貨], 找有貨的Port且ID = carrierID
                //有帳無料
                var port_HasIDButNoCargo = rack.PortsStatus.FirstOrDefault(p => p.CarrierID == carrierID && !p.CargoExist);
                if (port_HasIDButNoCargo != null)
                    throw new HasIDbutNoCargoException($"[{port_HasIDButNoCargo.GetLocID()}] 無貨物");
                port = rack.PortsStatus.FirstOrDefault(p => p.CargoExist && p.CarrierID == carrierID);
            }
            return port != null;
        }


        private bool NotTransferStationPort(clsPortOfRack p)
        {
            if (p.Layer > 0)
                return true;
            return p.TagNumbers.All(tag => tag.GetMapPoint().StationType != AGVSystemCommonNet6.MAP.MapPoint.STATION_TYPE.Buffer_EQ);
        }

        private bool HasOrderAssigned(clsPortOfRack rackPort)
        {
            if (rackPort == null)
                return true;

            List<string> runningGoalKeys = new List<string>();
            runningGoalKeys.AddRange(DatabaseCaches.TaskCaches.InCompletedTasks.Select(order => order.From_Station + "_" + order.From_Slot));
            runningGoalKeys.AddRange(DatabaseCaches.TaskCaches.InCompletedTasks.Select(order => order.To_Station + "_" + order.To_Slot));
            return rackPort.TagNumbers.Any(tag => runningGoalKeys.Contains($"{tag}_{rackPort.Layer}"));
        }
        private bool isDeviceIDBelongRack(string deviceID)
        {
            clsEQ eq = StaEQPManagager.MainEQList.FirstOrDefault(eq => eq.EndPointOptions.DeviceID.Contains(deviceID));
            if (eq != null)
                return false;
            return ALLRack.Any(rack => rack.RackOption.DeviceID == deviceID);
        }

        private bool isSourceAGV(string sourceID, out string agvName)
        {
            agvName = null;
            var agvState = DatabaseCaches.Vehicle.VehicleStates.FirstOrDefault(agv => agv.AGV_ID == sourceID);
            if (agvState == null)
                return false;
            agvName = agvState.AGV_Name;
            return agvName != null;
        }
        //string commandID, string carrierID, string carrierLoc, string? carrierZoneName, string dest
        public class clsTransportCommandDto
        {
            public string commandID { get; set; } = string.Empty;
            public string carrierID { get; set; } = string.Empty;
            public string carrierLoc { get; set; } = string.Empty;
            public string carrierZoneName { get; set; } = string.Empty;
            public string source { get; set; } = string.Empty;
            public string dest { get; set; } = string.Empty;
            public ushort priority { get; set; } = 0;
            public string lotID { get; set; } = string.Empty;
        }

    }
}
