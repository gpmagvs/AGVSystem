
using AGVSystem.Models.EQDevices;
using AGVSystemCommonNet6.Alarm;
using AGVSystemCommonNet6.Configuration;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.Material;
using EquipmentManagment.Device.Options;
using EquipmentManagment.MainEquipment;
using EquipmentManagment.Manager;
using EquipmentManagment.WIP;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NLog.Config;

namespace AGVSystem.Service
{
    public class EquipmentInitStartupService : IHostedService
    {
        IServiceScopeFactory _ServiceScopeFactory;
        IHubContext<FrontEndDataHub> hubContext;
        RackService rackService;
        public EquipmentInitStartupService(IServiceScopeFactory factory, IHubContext<FrontEndDataHub> hubContext)
        {
            _ServiceScopeFactory = factory;
            rackService = factory.CreateScope().ServiceProvider.GetRequiredService<RackService>();
            this.hubContext = hubContext;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                Models.EQDevices.EQDeviceEventsHandler.HubContext = hubContext;
                Models.EQDevices.EQDeviceEventsHandler.rackService = rackService;

                clsEQ.WirteOuputEnabled = !AGVSConfigulator.SysConfigs.BaseOnKGSWebAGVSystem;
                string eqConfigsStoreFolder = AGVSConfigulator.SysConfigs.PATHES_STORE[SystemConfigs.PATH_ENUMS.EQ_CONFIGS_FOLDER_PATH];
                StaEQPManagager.OnWIPConfigLoaded += (sender, wipOptions) =>
                {
                    //CheckWIPConfigration(wipOptions);
                };

                StaEQPManagager.OnEqConfigLoaded += (sender, eqOptions) =>
                {
                    CheckEQDeviceID(eqOptions);
                };

                StaEQPManagager.InitializeAsync(new clsEQManagementConfigs
                {
                    EQConfigPath = $"{eqConfigsStoreFolder}//EQConfigs.json",
                    WIPConfigPath = $"{eqConfigsStoreFolder}//WIPConfigs.json",
                    ChargeStationConfigPath = $"{eqConfigsStoreFolder}//ChargStationConfigs.json",
                    EQGroupConfigPath = $"{eqConfigsStoreFolder}//EQGroupConfigs.json",
                });
                List<clsStationStatus> cargoIDStored = await QueryCargoIDStoredFromDataBase();
                RestoreCargoID(cargoIDStored, out List<string> needRemoveIDList);
                await RemoveUselessMaterialIDOfDatabaseAsync(needRemoveIDList);
                await clsStationInfoManager.ScanWIP_EQ();
                StaEQPManagager.DevicesConnectToAsync();
            }
            catch (Exception ex)
            {
                AlarmManagerCenter.AddAlarmAsync(ALARMS.SYSTEM_EQP_MANAGEMENT_INITIALIZE_FAIL_WITH_EXCEPTION);
            }
        }

        private async Task RemoveUselessMaterialIDOfDatabaseAsync(List<string> needRemoveIDList)
        {
            try
            {
                using AGVSDbContext _dbContext = _ServiceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<AGVSDbContext>();
                foreach (var item in _dbContext.StationStatus.Where(raw => needRemoveIDList.Contains(raw.MaterialID)).ToList())
                {
                    item.MaterialID = "";
                }
                ;
                int _changed = await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {

            }
        }

        private void CheckEQDeviceID(Dictionary<string, clsEndPointOptions> eqOptions)
        {
            List<string> deviceIDList = eqOptions.Values.Select(v => v.DeviceID).Distinct().ToList();

            List<string> duplicateDeviceIDList = deviceIDList.Where(_deviceID => eqOptions.Where(pair => pair.Value.DeviceID == _deviceID).Count() > 1)
                                                             .ToList();

            foreach (var _deviceID in duplicateDeviceIDList)
            {
                var toModify = eqOptions.Where(pair => pair.Value.DeviceID == _deviceID).Skip(1);
                foreach (var item in toModify)
                {
                    item.Value.DeviceID = "SYL" + item.Key;
                }
                ;
            }
        }

        private void CheckWIPConfigration(Dictionary<string, clsRackOptions> wipOptions)
        {
            List<string> deviceIDList = wipOptions.Values.Select(v => v.DeviceID).Distinct().ToList();

            List<string> duplicateDeviceIDList = deviceIDList.Where(_deviceID => wipOptions.Where(pair => pair.Value.DeviceID == _deviceID).Count() > 1)
                                                             .ToList();

            foreach (var _deviceID in duplicateDeviceIDList)
            {
                var toModify = wipOptions.Where(pair => pair.Value.DeviceID == _deviceID).Skip(1);
                foreach (var item in toModify)
                {
                    item.Value.DeviceID = "SYS00" + item.Key;
                }
                ;
            }
        }

        private void ClsEQ_OnCSTReaderIDChanged(object? sender, (clsEQ, string newValue, string oldValue) e)
        {
            throw new NotImplementedException();
        }

        private void RestoreCargoID(List<clsStationStatus> cargoIDStored, out List<string> needRemoveIDList)
        {
            needRemoveIDList = new List<string>();
            var allRackPorts = StaEQPManagager.RacksList.SelectMany(rack => rack.PortsStatus);
            var wipPortIDList = StaEQPManagager.RacksList.SelectMany(rack => rack.RackOption.PortsOptions.Select(p => rack.EQName + "_" + p.ID)).ToList();

            foreach (var item in cargoIDStored)
            {
                string _name = item.StationName;
                string _materialID = item.MaterialID;
                var rackport = StaEQPManagager.RacksList.Select(rack => rack.GetPortByKeyWithRackName(_name))
                                                        .FirstOrDefault(rack => rack != null);
                if (rackport != null)
                {
                    bool isEqAsZonePortAndHasCstReader = rackport.IsRackPortIsEQ(out clsEQ eq) && eq.EndPointOptions.IsRoleAsZone && eq.EndPointOptions.IsCSTIDReportable;
                    if (!isEqAsZonePortAndHasCstReader)
                    {

                        if (rackport.Properties.EQInstall.IsUseForEQ)
                        {
                            needRemoveIDList.Add(_materialID.ToString());
                            _materialID = "";
                        }

                        rackport.CarrierID = _materialID;
                        rackport.InstallTime = item.UpdateTime;
                        rackport.SourceTag = item.SourceEqTag;
                        rackport.SourceSlot = item.SourceEqSlot;
                        if (eq != null && eq.EndPointOptions.IsRoleAsZone)
                        {
                            eq.PortStatus.CarrierID = _materialID;
                            eq.PortStatus.InstallTime = item.UpdateTime;
                            eq.PortStatus.SourceTag = item.SourceEqTag;
                            eq.PortStatus.SourceSlot = item.SourceEqSlot;
                        }
                    }
                }
            }


            //foreach (var eqAsZone in StaEQPManagager.MainEQList.Where(eq => eq.EndPointOptions.IsRoleAsZone && !eq.EndPointOptions.IsCSTIDReportable))
            //{
            //    var storedInfo = cargoIDStored.FirstOrDefault(st => st.StationTag + "" == eqAsZone.EndPointOptions.TagID + "");
            //    if (storedInfo == null)
            //        continue;

            //    clsPortOfRack rackPort = allRackPorts.FirstOrDefault(rackPort => rackPort.IsRackPortIsEQ(out var eq) && eq.EndPointOptions.TagID == eqAsZone.EndPointOptions.TagID);
            //    if (rackPort == null)
            //        continue;

            //    eqAsZone.PortStatus.CarrierID = storedInfo.MaterialID;
            //    rackPort.CarrierID = storedInfo.MaterialID;
            //}

        }
        public async Task StopAsync(CancellationToken cancellationToken)
        {
        }


        private async Task<List<clsStationStatus>> QueryCargoIDStoredFromDataBase()
        {
            try
            {
                using AGVSDbContext _dbContext = _ServiceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<AGVSDbContext>();
                return await _dbContext.StationStatus.AsNoTracking()
                                                     .Where(d => !string.IsNullOrEmpty(d.MaterialID))
                                                     .ToListAsync();
            }
            catch (Exception ex)
            {
                AlarmManagerCenter.AddAlarmAsync(ALARMS.SYSTEM_EQP_MANAGEMENT_INITIALIZE_FAIL_WITH_EXCEPTION);
                return new List<clsStationStatus>();
            }
        }
    }
}
