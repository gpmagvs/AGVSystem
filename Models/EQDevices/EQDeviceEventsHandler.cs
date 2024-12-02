using AGVSystem.Service;
using AGVSystemCommonNet6;
using AGVSystemCommonNet6.Alarm;
using AGVSystemCommonNet6.Configuration;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.Equipment;
using AGVSystemCommonNet6.Material;
using AGVSystemCommonNet6.Microservices.MCS;
using AGVSystemCommonNet6.Microservices.ResponseModel;
using AGVSystemCommonNet6.Notify;
using EquipmentManagment.ChargeStation;
using EquipmentManagment.Device;
using EquipmentManagment.MainEquipment;
using EquipmentManagment.Manager;
using EquipmentManagment.WIP;
using NLog;
using System.Collections.Concurrent;
using System.Linq;

namespace AGVSystem.Models.EQDevices
{
    public static class Extenion
    {
        public static bool IsEQInRack(this clsEQ eq, out clsRack rack)
        {
            rack = null;
            int tag = eq.EndPointOptions.TagID;
            rack = StaEQPManagager.RacksList.FirstOrDefault(rack => rack.RackOption.ColumnTagMap.SelectMany(k => k.Value).Contains(tag));
            return rack != null;
        }

        public static bool IsRackPortIsEQ(this clsPortOfRack rackPort, out clsEQ eq)
        {
            eq = null;
            if (rackPort.Layer > 0)
                return false;
            eq = StaEQPManagager.MainEQList.FirstOrDefault(eq => rackPort.TagNumbers.Contains(eq.EndPointOptions.TagID));
            return eq != null;
        }

        public static string GetLocID(this clsPortOfRack portOfRack)
        {
            string zoneID = portOfRack.GetParentRack().RackOption.DeviceID;
            string portPrefix = AGVSConfigulator.SysConfigs.SECSGem.CarrierLOCPrefixName;
            return $"{portPrefix}_{zoneID}_{portOfRack.Properties.PortNo}";
        }
    }
    public partial class EQDeviceEventsHandler
    {
        private static bool _disableEntryPointWhenEQPartsReplacing => AGVSConfigulator.SysConfigs.EQManagementConfigs.DisableEntryPointWhenEQPartsReplacing;
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private static SemaphoreSlim _EqUnlaodStateDBAccessSemaphorseSlim = new SemaphoreSlim(1, 1);


        private static ConcurrentDictionary<int, EqUnloadState> _EqUnloadStateRecordTempStore = new ConcurrentDictionary<int, EqUnloadState>();

        internal static void Initialize()
        {
            EndPointDeviceAbstract.OnEQDisconnected += HandleDeviceDisconnected;
            EndPointDeviceAbstract.OnEQConnected += HandleDeviceReconnected;
            EndPointDeviceAbstract.OnEQInputDataSizeNotEnough += HandleEQInputDataSizeNotEnough;
            EndPointDeviceAbstract.OnPartsStartReplacing += HandleEQStartPartsReplace;
            EndPointDeviceAbstract.OnPartsEndReplacing += HandleEQFinishPartsReplace;
            EndPointDeviceAbstract.OnDeviceMaintainStart += HandleDeviceMaintainStart;
            EndPointDeviceAbstract.OnDeviceMaintainFinish += HandleDeviceMaintainFinish;

            clsChargeStation.OnBatteryNotConnected += HandleChargeStationBatteryNotConnectEvent;
            clsChargeStation.OnBatteryChargeFull += HandleChargeFullEvent;
            clsChargeStation.OnChargeStationTemperatureOverThreshoad += HandleChargeStationTemperatureOverThreshoad;
            clsChargeStation.OnChargeStationTemperatureRestoreUnderThreshoad += HandleChargeStationTemperatureRestoreUnderThreshoad;

            ChargerIOSynchronizer.OnEMO += ChargerIOSynchronizer_OnEMO;
            ChargerIOSynchronizer.OnAirError += ChargerIOSynchronizer_OnAirError;
            ChargerIOSynchronizer.OnSmokeDetected += ChargerIOSynchronizer_OnSmokeDetected;
            ChargerIOSynchronizer.OnTemperatureModuleError += ChargerIOSynchronizer_OnTemperatureErrorDetected;

            clsPortOfRack.OnRackPortSensorFlash += HandlePortOfRackSensorFlash;
            clsPortOfRack.OnRackPortSensorStatusChanged += HandlePortOfRackSensorStatusChanged;
            clsPortOfRack.OnPortCargoChangedToExist += HandlePortCargoInstalled;
            clsPortOfRack.OnPortCargoChangeToDisappear += HandlePortCargoRemoved;

            clsEQ.OnIOStateChanged += HandleEQIOStateChanged;
            clsEQ.OnUnloadRequestChanged += HandleEQUnloadRequestChanged;
            clsEQ.OnPortCargoInstalled += HandleEQPortCargoInstalled;
            clsEQ.OnPortCargoRemoved += HandleEQPortCargoRemoved;

            PortStatusAbstract.CarrierIDChanged += PortStatusAbstract_CarrierIDChanged;

            MaterialManagerEventHandler.OnMaterialTransferStatusChange += HandleMaterialTransferStatusChanged;
            MaterialManagerEventHandler.OnMaterialAdd += HandleMaterialAdd;
            MaterialManagerEventHandler.OnMaterialDelete += HandleMaterialDelete;
        }

        private static void PortStatusAbstract_CarrierIDChanged(object? sender, string e)
        {
            clsPortOfRack rackPort = (sender as clsPortOfRack);
            ShelfStatusChangeEventReport(rackPort.GetParentRack());
        }

        private static void HandleEQPortCargoInstalled(object? sender, clsEQ eq)
        {
            if (eq.IsEQInRack(out clsRack rack) && eq.EndPointOptions.IsRoleAsZone)
            {
                Task.Factory.StartNew(async () =>
                {
                    if (string.IsNullOrEmpty(eq.PortStatus.CarrierID))
                    {
                        clsPortOfRack port = rack.PortsStatus.FirstOrDefault(port => port.TagNumbers.Contains(eq.EndPointOptions.TagID));
                        string tunid = await AGVSConfigulator.GetTrayUnknownFlowID();
                        eq.PortStatus.CarrierID = tunid;
                        if (port != null)
                        {
                            port.CarrierID = tunid;
                            await MCSCIMService.CarrierInstallCompletedReport(port.CarrierID, port.GetLocID(), port.GetParentRack().RackOption.DeviceID, 1);
                        }
                    }
                    await ZoneCapacityChangeEventReport(rack);
                });
            }
        }

        private static void HandleEQPortCargoRemoved(object? sender, clsEQ eq)
        {
            if (eq.IsEQInRack(out clsRack rack) && eq.EndPointOptions.IsRoleAsZone)
                ZoneCapacityChangeEventReport(rack);
        }

        private static void HandleMaterialDelete(object? sender, clsMaterialInfo e)
        {
            MaterialManager.DeleteMaterialInfo(e.MaterialID, e.SourceStation, e.InstallStatus, e.IDStatus, e.Type);
        }

        private static void HandleMaterialAdd(object? sender, clsMaterialInfo e)
        {
            MaterialManager.AddMaterialInfo(e.MaterialID, e.TargetStation, e.InstallStatus, e.IDStatus, e.Type, e.Condition);
        }

        private static void HandleMaterialTransferStatusChanged(object? sender, clsMaterialInfo e)
        {
            MaterialManager.CreateMaterialInfo(e.MaterialID, e.ActualID, e.SourceStation, e.TargetStation, e.TaskSourceStation, e.TaskTargetStation, e.InstallStatus, e.IDStatus, e.Type, e.Condition);
        }

        private static void HandleEQFinishPartsReplace(object? sender, EndPointDeviceAbstract device)
        {
            Task.Run(async () =>
            {
                var tagOfEQInPartsReplacing = device.EndPointOptions.TagID;
                if (_disableEntryPointWhenEQPartsReplacing)
                {
                    NotifyServiceHelper.SUCCESS($"設備-{device.EQName} 結束零件更換作業，開啟道路!");
                    ChangeEnableStateOfEntryPointOfEQOfMapAndRequestVMSReload(device.EndPointOptions.TagID, true);
                }

                NotifyServiceHelper.SUCCESS($"設備-{device.EQName} 結束零件更換作業，發送設備狀態變更提示至VMS!");
                clsResponseBase response = await AGVSystemCommonNet6.Microservices.VMS.VMSSerivces.RemovePartsReplaceworkstationTag(tagOfEQInPartsReplacing);
                _logger.Trace($"EQ({device.EQName}-Tag=>{tagOfEQInPartsReplacing}) Finish Parts Replacing Notify to VMS. VMS Response= {response.ToJson()}");


            });
        }

        private static void HandleEQStartPartsReplace(object? sender, EndPointDeviceAbstract device)
        {
            Task.Run(async () =>
            {
                var tagOfEQInPartsReplacing = device.EndPointOptions.TagID;
                if (_disableEntryPointWhenEQPartsReplacing)
                {
                    NotifyServiceHelper.SUCCESS($"設備-{device.EQName} 開始零件更換作業，封閉道路!");
                    ChangeEnableStateOfEntryPointOfEQOfMapAndRequestVMSReload(device.EndPointOptions.TagID, false);
                }

                NotifyServiceHelper.SUCCESS($"設備-{device.EQName} 開始零件更換作業，發送設備狀態變更提示至VMS!");
                clsResponseBase response = await AGVSystemCommonNet6.Microservices.VMS.VMSSerivces.AddPartsReplaceworkstationTag(tagOfEQInPartsReplacing);
                _logger.Trace($"EQ({device.EQName}-Tag=>{tagOfEQInPartsReplacing}) Start Parts Replacing Notify to VMS. VMS Response= {response.ToJson()}");

            });
        }

        private static void HandleEQInputDataSizeNotEnough(object? sender, EndPointDeviceAbstract device)
        {
            _ = Task.Factory.StartNew(async () =>
            {
                await AlarmManagerCenter.ResetAlarmAsync(new clsAlarmDto() { AlarmCode = (int)ALARMS.EQ_Input_Data_Not_Enough }, false);
                await AlarmManagerCenter.AddAlarmAsync(ALARMS.EQ_Input_Data_Not_Enough, source: ALARM_SOURCE.EQP, Equipment_Name: device.EQName);
            });
        }

        internal static async void HandleDeviceDisconnected(object? sender, EndPointDeviceAbstract device)
        {
            _logger.Trace($"EQ-{device.EQName} 連線中斷({device.EndPointOptions.ConnOptions.IP}:{device.EndPointOptions.ConnOptions.Port}-{device.EndPointOptions.ConnOptions.ConnMethod})", device.EQName);

            _ = Task.Run(async () =>
            {
                await AlarmManagerCenter.AddAlarmAsync(ALARMS.EQ_Disconnect, source: ALARM_SOURCE.EQP, Equipment_Name: device.EQName);
            });
        }

        internal static async void HandleDeviceReconnected(object? sender, EndPointDeviceAbstract device)
        {
            _logger.Trace($"EQ-{device.EQName} 已連線({device.EndPointOptions.ConnOptions.IP}-{device.EndPointOptions.ConnOptions.ConnMethod})", device.EQName);
            _ = Task.Run(async () =>
            {
                await AlarmManagerCenter.SetAlarmCheckedAsync(device.EQName, ALARMS.EQ_Disconnect, "SystemAuto");
            });
        }

        internal static async void HandleEQIOStateChanged(object? sender, EndPointDeviceAbstract.IOChangedEventArgs device)
        {
            _logger.Trace($"[{device.Device.EQName}] IO-{device.IOName} Changed To {(device.IOState ? "1" : "0")}", device.Device.EQName);
        }


        internal static void HandleEQUnloadRequestChanged(object? sender, (clsEQ eq, bool unload) args)
        {
            bool isStartWaitUnload = args.unload;
            DateTime time = DateTime.Now;

            _ = Task.Run(async () =>
            {
                try
                {
                    await _EqUnlaodStateDBAccessSemaphorseSlim.WaitAsync();

                    if (isStartWaitUnload)
                    {
                        EqUnloadState state = new EqUnloadState()
                        {
                            EQTag = args.eq.EndPointOptions.TagID,
                            EQName = args.eq.EQName,
                            StartWaitUnloadTime = time,
                        };
                        _EqUnloadStateRecordTempStore.AddOrUpdate(args.eq.EndPointOptions.TagID, state, (key, oldValue) =>
                        {
                            oldValue.StartWaitUnloadTime = time;
                            return oldValue;
                        });
                        using (AGVSDatabase agvsDb = new AGVSDatabase())
                        {
                            agvsDb.tables.EqpUnloadStates.Add(state);
                            await agvsDb.SaveChanges();
                        }
                    }
                    else
                    {
                        _EqUnloadStateRecordTempStore.TryRemove(args.eq.EndPointOptions.TagID, out EqUnloadState unloadState);
                        if (unloadState != null)
                        {
                            unloadState.EndWaitUnloadTime = time;
                            using (AGVSDatabase agvsDb = new AGVSDatabase())
                            {
                                var _unloadRecord = agvsDb.tables.EqpUnloadStates.FirstOrDefault(state => state.StartWaitUnloadTime == unloadState.StartWaitUnloadTime &&
                                                                                                         state.EQTag == unloadState.EQTag);
                                if (_unloadRecord != null)
                                {
                                    _unloadRecord.EndWaitUnloadTime = time;
                                    await agvsDb.SaveChanges();
                                }
                            }
                        }
                    }


                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "HandleEQUnloadRequestChanged");
                }
                finally
                {
                    _EqUnlaodStateDBAccessSemaphorseSlim.Release();
                }
            });

        }

        private static async Task ShelfStatusChangeEventReport(clsRack rack)
        {
            await Task.Delay(1).ContinueWith(async t =>
            {
                MCSCIMService.ZoneData zoneData = GenerateZoneData(rack);
                await MCSCIMService.ShelfStatusChange(zoneData);
            });
        }
        private static async Task ZoneCapacityChangeEventReport(clsRack rack)
        {
            await Task.Delay(1).ContinueWith(async t =>
            {
                MCSCIMService.ZoneData zoneData = GenerateZoneData(rack);
                await MCSCIMService.ZoneCapacityChange(zoneData);
            });
        }

        internal static MCSCIMService.ZoneData GenerateZoneData(clsRack rack)
        {
            try
            {
                string shelfIDPrefix = AGVSConfigulator.SysConfigs.SECSGem.CarrierLOCPrefixName;
                return new MCSCIMService.ZoneData()
                {
                    ZoneName = rack.RackOption.DeviceID,
                    ZoneType = 0,
                    LocationStatusList = GetLocationStatus(rack),

                };


                List<MCSCIMService.ZoneData.LocationStatus> GetLocationStatus(clsRack rack)
                {
                    List<MCSCIMService.ZoneData.LocationStatus> statuslist = rack.PortsStatus.Select(port => port.Properties.EQInstall.IsUseForEQ ?
                                                                                                                    GetLocationStatusOfEQLocatinPort(ref rack, ref port) :
                                                                                                                    GetLocationStatusOfPort(ref rack, port)).ToList();

                    return statuslist.Where(status => status != null).ToList();
                }

                MCSCIMService.ZoneData.LocationStatus GetLocationStatusOfPort(ref clsRack rack, clsPortOfRack port)
                {
                    return new MCSCIMService.ZoneData.LocationStatus
                    {
                        ShelfId = port.GetLocID(),
                        CarrierID = port.CarrierID,
                        IsCargoExist = port.CargoExist,
                        DisabledStatus = port.Properties.PortEnable == clsPortOfRack.Port_Enable.Disable ? 1 : 0,
                        ProcessState = 0
                    };
                }

                MCSCIMService.ZoneData.LocationStatus GetLocationStatusOfEQLocatinPort(ref clsRack rack, ref clsPortOfRack port)
                {
                    EquipmentManagment.Device.Options.clsRackPortProperty.clsPortUseToEQProperty eqInstallInfo = port.Properties.EQInstall;
                    if (!StaEQPManagager.TryGetEQByEqName(eqInstallInfo.BindingEQName, out clsEQ eQ, out string errorMsg))
                        return null;

                    if (!eQ.EndPointOptions.IsRoleAsZone)
                        return null;

                    return new MCSCIMService.ZoneData.LocationStatus
                    {
                        ShelfId = $"{shelfIDPrefix}_{rack.RackOption.DeviceID}_{port.PortNo}",
                        CarrierID = eQ.PortStatus.CarrierID,
                        IsCargoExist = eQ.Port_Exist,
                        DisabledStatus = eQ.EndPointOptions.Enable ? 0 : 1,
                        ProcessState = 0
                    };
                }
            }
            catch (Exception)
            {
                return new MCSCIMService.ZoneData();

            }
        }
    }

}
