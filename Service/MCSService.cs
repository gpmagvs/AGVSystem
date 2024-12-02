
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
    public class MCSService
    {
        List<clsPortOfRack> ALLRackPorts => StaEQPManagager.RacksList.SelectMany(rack => rack.PortsStatus).ToList();
        List<clsRack> ALLRack => StaEQPManagager.RacksList.ToList();

        public MCSService()
        {
        }

        internal async Task HandleTransportCommand(clsTransportCommandDto transportCommand)
        {
            int sourceTag = -1;
            int destineTag = -1;
            int sourceSlot = 0;
            int destineSlot = 0;

            if (isDeviceIDBelongRack(transportCommand.source))
            {
                TryGetRackPort(transportCommand.source, asDestine: false, transportCommand.carrierID, out clsPortOfRack? sourceRackPort);
                sourceTag = sourceRackPort == null ? -1 : sourceRackPort.TagNumbers.FirstOrDefault();
                sourceSlot = sourceRackPort == null ? -1 : sourceRackPort.Layer;
            }
            else
            {
                TryGetEQPort(transportCommand.source, out EndPointDeviceAbstract soucePort);
                sourceTag = soucePort == null ? -1 : soucePort.EndPointOptions.TagID;
            }

            if (isDeviceIDBelongRack(transportCommand.dest))
            {
                TryGetRackPort(transportCommand.dest, asDestine: true, transportCommand.carrierID, out clsPortOfRack? destineackPort);
                destineTag = destineackPort == null ? -1 : destineackPort.TagNumbers.FirstOrDefault();
                destineSlot = destineackPort == null ? -1 : destineackPort.Layer;

            }
            else
            {
                TryGetEQPort(transportCommand.dest, out EndPointDeviceAbstract destinePort);
                destineTag = destinePort == null ? -1 : destinePort.EndPointOptions.TagID;
            }

            if (sourceTag == -1 || destineTag == -1)
                return;

            Console.WriteLine($"Created Order: From [Tag {sourceTag}_Slot {sourceSlot}] TO  [Tag {destineTag}_Slot {destineSlot}]");

            (bool confirm, ALARMS alarm_code, string message, string message_en) = await TaskManager.AddTask(new AGVSystemCommonNet6.AGVDispatch.clsTaskDto
            {
                Action = AGVSystemCommonNet6.AGVDispatch.Messages.ACTION_TYPE.Carry,
                TaskName = transportCommand.commandID,
                Carrier_ID = transportCommand.carrierID,
                From_Station = sourceTag + "",
                To_Station = destineTag + "",
                From_Slot = sourceSlot + "",
                To_Slot = destineSlot + "",
                Priority = transportCommand.priority,
                RecieveTime = DateTime.Now,
                bypass_eq_status_check = false,
            }, TaskManager.TASK_RECIEVE_SOURCE.REMOTE);
        }
        private TransportCommandDto GetCommandInfo(string commandID)
        {
            return new TransportCommandDto()
            {
                CommandID = commandID,
            };
        }

        private bool TryGetEQPort(string deviceID, out EndPointDeviceAbstract port)
        {
            port = StaEQPManagager.MainEQList.FirstOrDefault(eq => eq.EndPointOptions.DeviceID.Contains(deviceID));
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
            if (eq!=null)
                return false;
            return ALLRack.Any(rack => rack.RackOption.DeviceID == deviceID);
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
