
using AGVSystem.Controllers;
using AGVSystem.Models.EQDevices;
using AGVSystem.Models.TaskAllocation.HotRun;
using AGVSystem.TaskManagers;
using AGVSystemCommonNet6;
using AGVSystemCommonNet6.AGVDispatch;
using AGVSystemCommonNet6.Alarm;
using AGVSystemCommonNet6.Alarm.SECS_Alarm_Code;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.Microservices.AudioPlay;
using AGVSystemCommonNet6.Microservices.MCS;
using EquipmentManagment.Device;
using EquipmentManagment.MainEquipment;
using EquipmentManagment.Manager;
using EquipmentManagment.WIP;
using Microsoft.EntityFrameworkCore;
using NLog;
using System.Linq;
using static AGVSystemCommonNet6.Alarm.SECS_Alarm_Code.SECSHCACKAlarmCodeMapper;
using static AGVSystemCommonNet6.Microservices.MCS.MCSCIMService;

namespace AGVSystem.Service.MCS
{
    public partial class MCSService
    {
        List<clsPortOfRack> ALLRackPorts => StaEQPManagager.RacksList.SelectMany(rack => rack.PortsStatus).ToList();
        List<clsRack> ALLRack => StaEQPManagager.RacksList.ToList();
        AGVSDbContext dbContext;
        internal static Logger logger = LogManager.GetCurrentClassLogger();
        Dictionary<string, clsEQ> MainEQMap => StaEQPManagager.MainEQList.Where(eq => !eq.EndPointOptions.IsRoleAsZone)
                                                                         .ToDictionary(eq => eq.EndPointOptions.DeviceID, eq => eq);
        private static SemaphoreSlim TransportCommandHandleSemaphoreSlim = new SemaphoreSlim(1, 1);
        public string MCSOrderRecievedAudioFilaPath => Path.Combine(Environment.CurrentDirectory, $"Audios/mcs_transfer_command_recieved.wav");

        public MCSService(AGVSDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        internal async Task HandleTransportCommand(clsTransportCommandDto transportCommand)
        {
            try
            {
                await TransportCommandHandleSemaphoreSlim.WaitAsync();

                logger.Info($"Start handle MCS transport command:{transportCommand.ToJson()}");
                AudioPlayService.PlaySpecficAudio(MCSOrderRecievedAudioFilaPath, 1.5);
                clsTaskDto order = null;
                try
                {
                    bool _isSourceAGV = false;
                    string sourceAGVName = "";
                    int sourceTag = -1;
                    int destineTag = -1;
                    int sourceSlot = -1;
                    int destineSlot = -1;
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
                            TryGetEQPort(transportCommand.source, transportCommand.carrierID, isAsSource: true, out clsEQ soucePort);
                            sourceTag = soucePort == null ? -1 : soucePort.EndPointOptions.TagID;
                            sourceSlot = soucePort == null ? -1 : soucePort.EndPointOptions.Height;
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
                        TryGetEQPort(transportCommand.dest, transportCommand.carrierID, isAsSource: false, out clsEQ destinePort);
                        destineTag = destinePort == null ? -1 : destinePort.EndPointOptions.TagID;
                        destineSlot = destinePort == null ? -1 : destinePort.EndPointOptions.Height;
                    }

                    if (!_isSourceAGV && (sourceTag == -1 || destineTag == -1 || sourceSlot == -1 || destineSlot == -1))
                    {
                        Exception ex = new Exception("找不到來源或起點");
                        logger.Error(ex);
                        throw ex;
                    }

                    order = new AGVSystemCommonNet6.AGVDispatch.clsTaskDto
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
                    };
                    logger.Info($"Created Order: From [Tag {sourceTag}_Slot {sourceSlot}] TO  [Tag {destineTag}_Slot {destineSlot}]:\r\n new clsTaskDto object created = {order.ToJson()}");

                    (bool confirm, ALARMS alarm_code, string message, string message_en) = await TaskManager.AddTask(order, TaskManager.TASK_RECIEVE_SOURCE.REMOTE);

                    if (!confirm)
                    {
                        logger.Warn($"Add Task Fail:[{alarm_code}] {message}-{message_en}");
                        SECSHCACKAlarmCodeMapper alarmCodeMapper = new AlarmCodeMapperBaseOnGPMSpec();
                        //SECSHCACKAlarmCodeMapper alarmCodeMapper = new AlarmCodeMapperBaseOnGPMSpec();
                        MapResult mapresult = alarmCodeMapper.GetHCACKReturnCode(alarm_code);
                        Exception ex = new AddOrderFailException(message, alarm_code, order, mapresult);
                        logger.Error(ex);
                        throw ex;
                    }
                    else
                        logger.Info($"Add Task Success! Task ID = {order.TaskName}");
                }
                catch (HasIDbutNoCargoException ex)
                {
                    logger.Error(ex);
                    throw ex;
                }
                catch (ZoneIsFullException ex)
                {
                    logger.Error(ex);
                    throw ex;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                TransportCommandHandleSemaphoreSlim.Release();
            }
        }

        private bool TryGetEQPort(string deviceID, string carrierID, bool isAsSource, out clsEQ port)
        {
            return MainEQMap.TryGetValue(deviceID, out port);
            //TODO 要考慮作為來源而且是 eq 作為 zone的情境 _

            //find eq first 
            port = StaEQPManagager.MainEQList.FirstOrDefault(eq => eq.EndPointOptions.DeviceID == deviceID);
            if (port != null)
                return true;

            if (isAsSource)
            {
                port = StaEQPManagager.MainEQList.Where(eq => eq.EndPointOptions.IsRoleAsZone)
                                                 .FirstOrDefault(eq => eq.EndPointOptions.DeviceID.Contains(deviceID) && eq.PortStatus.CarrierID == carrierID);
                if (port != null)
                    return true;
            }
            port = StaEQPManagager.MainEQList.FirstOrDefault(eq => eq.EndPointOptions.DeviceID.Contains(deviceID) && (!isAsSource ? (!eq.Port_Exist && string.IsNullOrEmpty(eq.PortStatus.CarrierID)) : true));

            if (port != null && isAsSource && port.PortStatus.CarrierID == carrierID && !port.Port_Exist)
            {
                var ex = new HasIDbutNoCargoException($"[{port.EQName}] 無貨物");
                logger.Error(ex.Message);
                throw ex;
            }

            return port != null;
        }
        private bool TryGetRackPort(string deviceID, bool asDestine, string carrierID, out clsPortOfRack? port)
        {
            port = null;
            clsRack rack = ALLRack.FirstOrDefault(rack => rack.RackOption.DeviceID == deviceID && rack.PortsStatus.Any(port => asDestine ? true : port.CarrierID == carrierID || (port.IsRackPortIsEQ(out clsEQ eq) && eq.CSTIDReadValue == carrierID)));
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

                if (port == null)
                {
                    var ex = new ZoneIsFullException($"zone-{deviceID} is full.");
                    logger.Error(ex.Message);
                    throw ex;
                }


            }
            else
            {
                //Rack 是來源地[取貨], 找有貨的Port且ID = carrierID
                //有帳無料
                port = rack.PortsStatus.FirstOrDefault(p => p.CarrierID == carrierID || (p.IsRackPortIsEQ(out clsEQ eq) && eq.CSTIDReadValue == carrierID));
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
            return !MainEQMap.TryGetValue(deviceID, out clsEQ eq);
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

        internal async Task<MCSCIMController.clsResult> HandleTransportCancelAsync(string commandID)
        {
            clsTaskDto taskOrder = dbContext.Tasks.FirstOrDefault(order => order.TaskName == commandID);
            if (taskOrder != null)
            {
                TransportCommandDto _transportCommandDto = new TransportCommandDto()
                {
                    CommandID = commandID,
                    CarrierID = taskOrder.Carrier_ID,
                    CarrierLoc = taskOrder.soucePortID,
                    CarrierZoneName = taskOrder.sourceZoneID,
                };
                await MCSCIMService.TransferCancelInitiatedReport(_transportCommandDto).ContinueWith(async t =>
                {
                    taskOrder.State = AGVSystemCommonNet6.AGVDispatch.Messages.TASK_RUN_STATUS.CANCEL;
                    taskOrder.FailureReason = $"任務已被MCS取消(Canceled By MCS)";
                    await MCSCIMService.TransferCancelCompletedReport(_transportCommandDto);
                });
            }
            return new MCSCIMController.clsResult();
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
            public bool simulation { get; set; } = false;
        }

    }
}
