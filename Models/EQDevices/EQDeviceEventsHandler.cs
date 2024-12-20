﻿using AGVSystem.Service;
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
using Microsoft.AspNetCore.SignalR;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Policy;

namespace AGVSystem.Models.EQDevices
{
    public partial class EQDeviceEventsHandler
    {
        private static bool _disableEntryPointWhenEQPartsReplacing => AGVSConfigulator.SysConfigs.EQManagementConfigs.DisableEntryPointWhenEQPartsReplacing;
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private static SemaphoreSlim _EqUnlaodStateDBAccessSemaphorseSlim = new SemaphoreSlim(1, 1);
        internal static IHubContext<FrontEndDataHub> HubContext { get; set; }
        internal static RackService rackService;
        private static SemaphoreSlim _RackDataBrocastSemaphoreSlim = new SemaphoreSlim(1, 1);
        private static SemaphoreSlim _EQDataBrocastSemaphoreSlim = new SemaphoreSlim(1, 1);
        private static SemaphoreSlim CarrierIDChangeHandleSemaphoreSlim = new SemaphoreSlim(1, 1);

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
            clsPortOfRack.OnPortCargoChangedToExist += HandlePortCargoChangedToExist;
            clsPortOfRack.OnPortCargoChangeToDisappear += HandlePortCargoChangeToDisappear;

            clsEQ.OnIOStateChanged += HandleEQIOStateChanged;
            clsEQ.OnUnloadRequestChanged += HandleEQUnloadRequestChanged;
            clsEQ.OnEQPortCargoChangedToExist += HandleEQPortCargoChangedToExist;
            clsEQ.OnEQPortCargoChangedToDisappear += HandleEQPortCargoChangedToDisappear;
            clsEQ.OnCSTReaderIDChanged += ClsEQ_OnCSTReaderIDChanged;

            PortStatusAbstract.CarrierIDChanged += HandlePortCarrierIDChanged;

            MaterialManagerEventHandler.OnMaterialTransferStatusChange += HandleMaterialTransferStatusChanged;
            MaterialManagerEventHandler.OnMaterialAdd += HandleMaterialAdd;
            MaterialManagerEventHandler.OnMaterialDelete += HandleMaterialDelete;
        }

        private static void ClsEQ_OnCSTReaderIDChanged(object? sender, (clsEQ eq, string newValue, string oldValue) e)
        {
            BrocastRackData();
            if (!e.eq.EndPointOptions.IsRoleAsZone)
                return;
            Task.Factory.StartNew(async () =>
            {

                string zoneID = "";
                string carrierLoc = "";

                if (e.eq.IsEQInRack(out clsRack rack, out clsPortOfRack port))
                {
                    //port.CarrierID = e.newValue;
                    zoneID = port.GetParentRack().RackOption.DeviceID;
                    carrierLoc = port.GetLocID();
                    bool isNewInstall = string.IsNullOrEmpty(e.oldValue) && !string.IsNullOrEmpty(e.newValue); //新建
                    bool isRemoved = !string.IsNullOrEmpty(e.oldValue) && string.IsNullOrEmpty(e.newValue);//移除
                    bool isChanged = e.oldValue != e.newValue && !string.IsNullOrEmpty(e.oldValue) && !string.IsNullOrEmpty(e.newValue); //變化

                    List<clsPortOfRack> repeatCarrierIDPorts = new List<clsPortOfRack>();
                    bool isRepeatdIDOfOtherPortInSystem = !string.IsNullOrEmpty(e.newValue) && TryGetPortWithID(e.newValue, out repeatCarrierIDPorts) && repeatCarrierIDPorts.Count > 1;

                    bool isAGVLoadUnloadExecuting = e.eq.CMD_Reserve_Up || e.eq.CMD_Reserve_Low;

                    if (isNewInstall) //TODO 如果 id新增是因為AGV放貨 不用報
                    {

                        if (!isAGVLoadUnloadExecuting) //手投
                        {
                            string installID = e.newValue;
                            if (e.eq.IsCSTIDReadFail)
                                installID = await AGVSConfigulator.GetTrayUnknownFlowID();
                            e.eq.PortStatus.CarrierID = port.CarrierID = installID;
                            e.eq.SetAGVAssignedCarrierID(installID);
                            await MCSCIMService.CarrierInstallCompletedReport(installID, carrierLoc, zoneID, 1);
                        }
                        else //AGV放貨
                        {
                            string installID = e.newValue;
                            if (e.eq.IsCSTIDReadFail)
                            {
                                await MCSCIMService.CarrierRemoveCompletedReport(e.eq.AGVAssignCarrierID, carrierLoc, zoneID, 1);
                                installID = await AGVSConfigulator.GetTrayUnknownFlowID();
                            }
                            else if (e.eq.IsCSTIDReadMismatch)
                            {
                                await MCSCIMService.CarrierRemoveCompletedReport(e.eq.AGVAssignCarrierID, carrierLoc, zoneID, 1);
                                installID = await AGVSConfigulator.GetTrayMissMatchFlowID();
                            }
                            e.eq.PortStatus.CarrierID = port.CarrierID = installID;
                            await MCSCIMService.CarrierInstallCompletedReport(installID, carrierLoc, zoneID, 1);
                            e.eq.SetAGVAssignedCarrierID(installID);
                        }
                    }
                    if (isRemoved) //TODO 如果 id移除是因為AGV取貨 不用報
                    {
                        if (isAGVLoadUnloadExecuting) //AGV取
                        {
                        }
                        else //台車取
                        {
                            await MCSCIMService.CarrierRemoveCompletedReport(e.eq.AGVAssignCarrierID, carrierLoc, zoneID, 1);
                        }
                        e.eq.SetAGVAssignedCarrierID("");

                    }
                    if (isChanged)
                    {
                        await MCSCIMService.CarrierRemoveCompletedReport(e.oldValue, carrierLoc, zoneID, 1).ContinueWith(async t =>
                        {
                            await MCSCIMService.CarrierInstallCompletedReport(e.newValue, carrierLoc, zoneID, 1);
                        });

                    }


                    if (isRepeatdIDOfOtherPortInSystem)
                    {
                        string carrierToRemove = e.newValue;
                        DecoupleCarrierIDHandler(carrierToRemove, repeatCarrierIDPorts, port);
                    }

                    await ShelfStatusChangeEventReport(rack).ContinueWith(async t =>
                    {
                        await Task.Delay(100);
                        await ZoneCapacityChangeEventReport(rack);
                    });
                }



            });
            //sync to zone if nessary

        }

        /// <summary>
        /// 處理貨物ID變化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static void HandlePortCarrierIDChanged(object? sender, (string newValue, string oldValue, bool isUpdateByVehicleLoadUnload) args)
        {
            Task.Factory.StartNew(async () =>
            {
                BrocastRackData();
                if (sender == null)
                    return;
                bool getSignal = await CarrierIDChangeHandleSemaphoreSlim.WaitAsync(TimeSpan.FromSeconds(1));
                if (!getSignal)
                    return;
                try
                {
                    clsPortOfRack rackPort = (sender as clsPortOfRack);
                    if (rackPort == null)
                        return;

                    if (rackPort.Properties.PortEnable == clsPortOfRack.Port_Enable.Disable)
                        return;

                    if (rackPort.IsRackPortIsEQ(out clsEQ eq) && eq.EndPointOptions.IsCSTIDReportable)
                        return;

                    string locID = rackPort.GetLocID();
                    string zoneID = rackPort.GetParentRack().RackOption.DeviceID;
                    List<clsPortOfRack> repeatCarrierIDPorts = new List<clsPortOfRack>();
                    bool isNewInstall = string.IsNullOrEmpty(args.oldValue) && !string.IsNullOrEmpty(args.newValue); //新建
                    bool isRemoved = !string.IsNullOrEmpty(args.oldValue) && string.IsNullOrEmpty(args.newValue);//移除
                    bool isChanged = args.oldValue != args.newValue && !string.IsNullOrEmpty(args.oldValue) && !string.IsNullOrEmpty(args.newValue); //變化
                    bool isRepeatdIDOfOtherPortInSystem = !string.IsNullOrEmpty(args.newValue) && TryGetPortWithID(args.newValue, out repeatCarrierIDPorts) && repeatCarrierIDPorts.Count > 1;


                    if (isNewInstall)
                    {
                        if (!args.isUpdateByVehicleLoadUnload)//若carrier id 變化是因為 agv 放貨 (在席會先ON建一個TUN帳),則不用 報 install, 因為車子會報 transfer completed.
                            await MCSCIMService.CarrierInstallCompletedReport(args.newValue, locID, zoneID, 1);
                    }
                    if (isRemoved)
                    {
                        if (!args.isUpdateByVehicleLoadUnload)
                            await MCSCIMService.CarrierRemoveCompletedReport(args.oldValue, locID, zoneID, 1);

                        //若carrier id 移除了是因為 agv 取貨 
                        if (rackPort.CargoExist || rackPort.CarrierExist)
                        {
                            bool isTraySensorOn = rackPort.MaterialExistSensorStates.Any(pair => (pair.Value == clsPortOfRack.SENSOR_STATUS.ON || pair.Value == clsPortOfRack.SENSOR_STATUS.FLASH)
                                                                                    && (pair.Key == clsPortOfRack.SENSOR_LOCATION.TRAY_1 || pair.Key == clsPortOfRack.SENSOR_LOCATION.TRAY_2));
                            rackPort.CarrierID = isTraySensorOn ? await AGVSConfigulator.GetTrayUnknownFlowID() : await AGVSConfigulator.GetRackUnknownFlowID();
                            if (rackPort.IsRackPortIsEQ(out eq) && eq.Port_Exist)
                            {
                                eq.PortStatus.CarrierID = rackPort.CarrierID;
                            }
                        }
                    }
                    if (isChanged)
                    {
                        await MCSCIMService.CarrierRemoveCompletedReport(args.oldValue, locID, zoneID, 1);
                        await Task.Delay(110);
                        await MCSCIMService.CarrierInstallCompletedReport(args.newValue, locID, zoneID, 1);
                    }

                    await Task.Delay(500);
                    await ShelfStatusChangeEventReport(rackPort.GetParentRack());
                    if (isRepeatdIDOfOtherPortInSystem)
                    {
                        string carrierToRemove = args.newValue;
                        DecoupleCarrierIDHandler(carrierToRemove, repeatCarrierIDPorts, rackPort);
                    }

                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message, ex);
                }
                finally
                {
                    CarrierIDChangeHandleSemaphoreSlim.Release();
                }
            });
        }

        private static async Task DecoupleCarrierIDHandler(string carrierToRemove, List<clsPortOfRack> repeatCarrierIDPorts, clsPortOfRack triggerPort)
        {
            await Task.Delay(1000);
            foreach (var _portToRemove in repeatCarrierIDPorts.Where(port => !port.IsRackPortIsEQ(out clsEQ eq) ? true : !eq.EndPointOptions.IsCSTIDReportable))
            {
                string locID = _portToRemove.GetLocID();
                string zoneID = _portToRemove.GetParentRack().RackOption.DeviceID;
                bool isTray = carrierToRemove.StartsWith("T");
                string DUID = isTray ? await AGVSConfigulator.GetDoubleTrayUnknownFlowID() : await AGVSConfigulator.GetDoubleRackUnknownFlowID();
                _portToRemove.VehicleLoadToPortFlag = _portToRemove.VehicleLoadToPortFlag = false;
                _portToRemove.CarrierID = DUID;

                if (_portToRemove.IsRackPortIsEQ(out clsEQ eqInPort))
                {
                    eqInPort.PortStatus.CarrierID = DUID;
                }
                //await MCSCIMService.CarrierRemoveCompletedReport(carrierToRemove, locID, zoneID, 1).ContinueWith(async t =>
                //{
                //    await MCSCIMService.CarrierInstallCompletedReport(DUID, locID, zoneID, 1);
                //});
            }
        }

        private static bool TryGetPortWithID(string carrierID, out List<clsPortOfRack> ports)
        {
            ports = new List<clsPortOfRack>();
            IEnumerable<clsPortOfRack> allPorts = StaEQPManagager.RacksList.SelectMany(rack => rack.PortsStatus);
            ports = allPorts.Where(port => port.CarrierID == carrierID || (port.IsRackPortIsEQ(out clsEQ eq) && eq.PortStatus.CarrierID == carrierID)).ToList();
            return ports.Any();
        }

        private static void HandleEQPortCargoChangedToExist(object? sender, clsEQ eq)
        {
            BrocastRackData();
            if (eq.IsEQInRack(out clsRack rack, out clsPortOfRack port) && eq.EndPointOptions.IsRoleAsZone)
            {
                Task.Factory.StartNew(async () =>
                {
                    if (eq.EndPointOptions.IsCSTIDReportable)
                    {

                    }
                    else
                    {
                        if (string.IsNullOrEmpty(eq.PortStatus.CarrierID))
                        {
                            clsPortOfRack port = rack.PortsStatus.FirstOrDefault(port => port.TagNumbers.Contains(eq.EndPointOptions.TagID));
                            string tunid = await AGVSConfigulator.GetTrayUnknownFlowID();
                            eq.PortStatus.CarrierID = tunid;
                            if (port != null)
                            {
                                port.CarrierID = tunid;
                            }
                        }
                        await ZoneCapacityChangeEventReport(rack);
                    }
                });

            }
        }

        private static void HandleEQPortCargoChangedToDisappear(object? sender, clsEQ eq)
        {
            BrocastRackData();
            if (eq.IsEQInRack(out clsRack rack, out clsPortOfRack port) && eq.EndPointOptions.IsRoleAsZone && !eq.EndPointOptions.IsCSTIDReportable)
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
            BrocastEQkData();
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


                List<MCSCIMService.ZoneData.LocationStatus> GetLocationStatus(clsRack _rack)
                {
                    var racks = StaEQPManagager.RacksList.Where(rack => rack.RackOption.DeviceID == _rack.EndPointOptions.DeviceID);
                    List<MCSCIMService.ZoneData.LocationStatus> statuslist = racks.SelectMany(rack => rack.PortsStatus.Select(port => port.Properties.EQInstall.IsUseForEQ ?
                                                                                                                    GetLocationStatusOfEQLocatinPort(ref rack, ref port) :
                                                                                                                    GetLocationStatusOfPort(ref rack, port))).ToList();
                    return statuslist.Where(status => status != null).ToList();
                }

                MCSCIMService.ZoneData.LocationStatus GetLocationStatusOfPort(ref clsRack rack, clsPortOfRack port)
                {
                    return new MCSCIMService.ZoneData.LocationStatus
                    {
                        ShelfId = port.GetLocID(),
                        CarrierID = port.CarrierID,
                        IsCargoExist = port.CarrierExist,
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
                        ShelfId = port.GetLocID(),
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
    public static class Extenion
    {
        public static bool IsEQInRack(this clsEQ eq, out clsRack rack, out clsPortOfRack port)
        {
            rack = null;
            port = null;
            int tag = eq.EndPointOptions.TagID;
            rack = StaEQPManagager.RacksList.FirstOrDefault(rack => rack.RackOption.ColumnTagMap.SelectMany(k => k.Value).Contains(tag));
            if (rack == null)
                return false;
            port = rack.PortsStatus.FirstOrDefault(port => port.TagNumbers.Contains(tag));
            return port != null;
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

        public static bool IsRackPortAssignToOrder(this clsPortOfRack rackPort)
        {
            List<string> destinesKeys = DatabaseCaches.TaskCaches.InCompletedTasks.SelectMany(order => new string[] { $"{order.From_Station}_{order.From_Slot}", $"{order.To_Station}_{order.To_Slot}" })
                                                                                  .ToList();
            return rackPort.TagNumbers.Any(tag => destinesKeys.Contains($"{tag}_{rackPort.Layer}"));
        }

        public static bool IsEQPortAssignToOrder(this clsEQ eqPort)
        {
            List<string> destinesKeys = DatabaseCaches.TaskCaches.InCompletedTasks.SelectMany(order => new string[] { $"{order.From_Station}_{order.From_Slot}", $"{order.To_Station}_{order.To_Slot}" })
                                                                                  .ToList();
            return destinesKeys.Contains($"{eqPort.EndPointOptions.TagID}_{eqPort.EndPointOptions.Height}");
        }
    }
}
