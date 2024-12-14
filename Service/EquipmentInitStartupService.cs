﻿
using AGVSystem.Models.EQDevices;
using AGVSystemCommonNet6.Alarm;
using AGVSystemCommonNet6.Configuration;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.Material;
using EquipmentManagment.MainEquipment;
using EquipmentManagment.Manager;
using EquipmentManagment.WIP;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace AGVSystem.Service
{
    public class EquipmentInitStartupService : IHostedService
    {
        private readonly AGVSDbContext _dbContext;
        IHubContext<FrontEndDataHub> hubContext;
        RackService rackService;
        public EquipmentInitStartupService(IServiceScopeFactory factory, IHubContext<FrontEndDataHub> hubContext)
        {
            _dbContext = factory.CreateScope().ServiceProvider.GetRequiredService<AGVSDbContext>();
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

                StaEQPManagager.InitializeAsync(new clsEQManagementConfigs
                {
                    EQConfigPath = $"{eqConfigsStoreFolder}//EQConfigs.json",
                    WIPConfigPath = $"{eqConfigsStoreFolder}//WIPConfigs.json",
                    ChargeStationConfigPath = $"{eqConfigsStoreFolder}//ChargStationConfigs.json",
                    EQGroupConfigPath = $"{eqConfigsStoreFolder}//EQGroupConfigs.json",
                });
                List<clsStationStatus> cargoIDStored = await QueryCargoIDStoredFromDataBase();
                RestoreCargoID(cargoIDStored);
                await clsStationInfoManager.ScanWIP_EQ();
                StaEQPManagager.DevicesConnectToAsync();
            }
            catch (Exception ex)
            {
                AlarmManagerCenter.AddAlarmAsync(ALARMS.SYSTEM_EQP_MANAGEMENT_INITIALIZE_FAIL_WITH_EXCEPTION);
            }
        }

        private void ClsEQ_OnCSTReaderIDChanged(object? sender, (clsEQ, string newValue, string oldValue) e)
        {
            throw new NotImplementedException();
        }

        private void RestoreCargoID(List<clsStationStatus> cargoIDStored)
        {
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
                        rackport.CarrierID = _materialID;
                }
            }


            foreach (var eqAsZone in StaEQPManagager.MainEQList.Where(eq => eq.EndPointOptions.IsRoleAsZone && !eq.EndPointOptions.IsCSTIDReportable))
            {
                var storedInfo = cargoIDStored.FirstOrDefault(st => st.StationTag + "" == eqAsZone.EndPointOptions.TagID + "");
                if (storedInfo == null)
                    continue;

                clsPortOfRack rackPort = allRackPorts.FirstOrDefault(rackPort => rackPort.IsRackPortIsEQ(out var eq) && eq.EndPointOptions.TagID == eqAsZone.EndPointOptions.TagID);
                if (rackPort == null)
                    continue;

                eqAsZone.PortStatus.CarrierID = storedInfo.MaterialID;
                rackPort.CarrierID = storedInfo.MaterialID;
            }

        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
        }


        private async Task<List<clsStationStatus>> QueryCargoIDStoredFromDataBase()
        {
            try
            {
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
