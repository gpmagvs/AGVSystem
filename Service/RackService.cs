using AGVSystem.Models.EQDevices;
using AGVSystemCommonNet6.Configuration;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.Material;
using AGVSystemCommonNet6.Microservices.MCS;
using EquipmentManagment.MainEquipment;
using EquipmentManagment.Manager;
using EquipmentManagment.WIP;
using Microsoft.EntityFrameworkCore;
using NLog;
using System;

namespace AGVSystem.Service
{
    public class RackService
    {
        private static SemaphoreSlim dbSemaphoreSlim = new SemaphoreSlim(1, 1);
        private readonly AGVSDbContext _dbContext;
        private Logger _logger = LogManager.GetCurrentClassLogger();
        public RackService(IServiceScopeFactory factory)
        {
            _dbContext = factory.CreateScope().ServiceProvider.GetRequiredService<AGVSDbContext>();
        }

        internal List<ViewModel.WIPDataViewModel> GetWIPDataViewModels()
        {
            Random random = new Random(DateTime.Now.Second);

            return StaEQPManagager.RacksList.Select(wip => new ViewModel.WIPDataViewModel()
            {
                WIPName = wip.EQName,
                Columns = wip.RackOption.Columns,
                Rows = wip.RackOption.Rows,
                DeviceID = string.IsNullOrEmpty(wip.RackOption.DeviceID) ? $"SYS-{random.NextInt64(1, 12222222)}" : wip.RackOption.DeviceID,
                Ports = wip.GetPortStatusWithEqInfo().ToList(),
                ColumnsTagMap = wip.RackOption.ColumnTagMap,
                IsOvenAsRacks = wip.RackOption.MaterialInfoFromEquipment
            }).OrderBy(wip => wip.WIPName).ToList();
        }


        internal async Task<(bool confirm, string removedCarrierID, string message)> RemoveRackCargoIDManual(string wIPID, string portID, string triggerBy, bool isByAgvUnloadend)
        {
            if (TryGetPort(wIPID, portID, out clsPortOfRack port))
            {
                int tag = port.TagNumbers.FirstOrDefault();
                int slot = port.Properties.Row;
                var result = await RemoveRackCargoID(tag, slot, triggerBy, isByAgvUnloadend);

                if (result.confirm && port.CargoExist || (port.IsRackPortIsEQ(out clsEQ eq) && eq.EndPointOptions.IsRoleAsZone && eq.Port_Exist))
                {
                    _ = Task.Delay(400).ContinueWith(async t =>
                    {
                        bool isRackSensorOn = port.MaterialExistSensorStates.Any(pair => (pair.Value == clsPortOfRack.SENSOR_STATUS.ON || pair.Value == clsPortOfRack.SENSOR_STATUS.FLASH)
                                                                                        && (pair.Key == clsPortOfRack.SENSOR_LOCATION.RACK_1 || pair.Key == clsPortOfRack.SENSOR_LOCATION.RACK_2));

                        string tunid = isRackSensorOn ? await AGVSConfigulator.GetRackUnknownFlowID() : await AGVSConfigulator.GetTrayUnknownFlowID();
                        await AddRackCargoIDManual(wIPID, portID, tunid, triggerBy, isByAgvUnloadend);
                    });
                }

                return result;

            }
            else
                return (false, "", "Port Not Found");


        }
        internal async Task<(bool confirm, string removedCarrierID, string message)> RemoveRackCargoID(int tagNumber, int slot, string triggerBy, bool isByAgvUnloadend)
        {
            try
            {
                string removedCarrierID = "";

                if (!TryGetPort(tagNumber, slot, out clsPortOfRack port))
                    return (false, "", "Port Not Found"); ;
                string locID = port.GetLocID();
                string zoneName = port.GetParentRack().RackOption.DeviceID;

                bool isEqAsZonePortHasCSTIDReader = port.IsRackPortIsEQ(out clsEQ eq) && eq.EndPointOptions.IsRoleAsZone && eq.EndPointOptions.IsCSTIDReportable;
                if (isEqAsZonePortHasCSTIDReader)
                    return (false, eq.PortStatus.CarrierID, "該Port具有Carrier ID Reader 功能，無法修改帳料資訊");

                removedCarrierID = port.CarrierID + "";

                port.VehicleUnLoadFromPortFlag = isByAgvUnloadend;
                port.CarrierID = string.Empty;


                if (eq != null)
                    eq.PortStatus.CarrierID = string.Empty;
                UpdateMaterialIDStoreOfDataBase(tagNumber, slot, string.Empty);

                if (!isByAgvUnloadend)
                    MCSCIMService.CarrierRemoveCompletedReport(removedCarrierID, locID, zoneName, 0);
                EQDeviceEventsHandler.BrocastRackData();
                await Task.Delay(200);
                await EQDeviceEventsHandler.ZoneCapacityChangeEventReport(port.GetParentRack());
                await Task.Delay(200);
                await EQDeviceEventsHandler.ShelfStatusChangeEventReport(port.GetParentRack());
                _logger.Info($"WIP:{port.GetParentRack().EQName} Port-{port.Properties.ID} Cargo ID Removed. (Trigger By:{triggerBy})");
                return (true, removedCarrierID, "");

            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return (false, "", ex.Message);

            }
            finally
            {
            }
        }


        internal async Task<(bool confirm, string message)> AddRackCargoIDManual(string WIPID, string PortID, string cargoID, string triggerBy, bool isByAgvLoadend, bool bypassHasCSTReaderCheck = false)
        {
            if (TryGetPort(WIPID, PortID, out clsPortOfRack port))
            {
                int tag = port.TagNumbers.FirstOrDefault();
                int slot = port.Properties.Row;
                return await AddRackCargoID(tag, slot, cargoID, triggerBy, isByAgvLoadend, bypassHasCSTReaderCheck);
            }
            else
                return (false, "Port Not Found");

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tagNumber"></param>
        /// <param name="slot"></param>
        /// <param name="cargoID"></param>
        /// <param name="triggerBy"></param>
        /// <param name="isByAgvLoadend">是不是因為車子放貨到port修改帳籍</param>
        /// <returns></returns>
        internal async Task<(bool confirm, string message)> AddRackCargoID(int tagNumber, int slot, string cargoID, string triggerBy, bool isByAgvLoadend, bool bypassHasCSTReaderCheck = false)
        {
            try
            {
                if (!TryGetPort(tagNumber, slot, out clsPortOfRack port))
                    return (false, "Port Not Found");

                bool isEqAsZonePortHasCSTIDReader = port.IsRackPortIsEQ(out clsEQ eq) && eq.EndPointOptions.IsRoleAsZone && eq.EndPointOptions.IsCSTIDReportable;
                if (!bypassHasCSTReaderCheck && isEqAsZonePortHasCSTIDReader)
                    return (false, "該Port具有Carrier ID Reader 功能，無法修改帳料資訊");

                string locID = port.GetLocID();
                string zoneName = port.GetParentRack().RackOption.DeviceID;

                string oldCarrierID = port.CarrierID;
                if (!string.IsNullOrEmpty(oldCarrierID))
                {
                    MCSCIMService.CarrierRemoveCompletedReport(oldCarrierID, locID, zoneName, 0);
                    port.CarrierID = "";
                    if (eq != null)
                        eq.PortStatus.CarrierID = "";
                    await Task.Delay(300);
                    await EQDeviceEventsHandler.ShelfStatusChangeEventReport(port.GetParentRack());
                    await Task.Delay(300);
                }
                DateTime installTime = DateTime.Now;
                port.VehicleLoadToPortFlag = isByAgvLoadend;
                port.CarrierID = cargoID;
                port.InstallTime = installTime;

                if (eq != null)
                {
                    eq.PortStatus.CarrierID = cargoID;
                    eq.PortStatus.InstallTime = installTime;
                }
                UpdateMaterialIDStoreOfDataBase(tagNumber, slot, cargoID);
                if (!isByAgvLoadend)
                    MCSCIMService.CarrierInstallCompletedReport(cargoID, locID, zoneName, 0);
                _logger.Info($"WIP:{port.GetParentRack().EQName} Port-{port.Properties.ID} Cargo ID Changed to {cargoID}(Trigger By:{triggerBy})");
                EQDeviceEventsHandler.BrocastRackData();
                await Task.Delay(200);
                await EQDeviceEventsHandler.ZoneCapacityChangeEventReport(port.GetParentRack());
                await Task.Delay(200);
                await EQDeviceEventsHandler.ShelfStatusChangeEventReport(port.GetParentRack());
                return (true, "");
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return (false, ex.Message);
            }
            finally
            {
            }
        }

        internal async Task<(bool confirm, string message)> PortUsableSwitch(string wIPID, string portID, bool usable)
        {
            if (TryGetPort(wIPID, portID, out clsPortOfRack port))
            {
                StaEQPManagager.RacksOptions.TryGetValue(wIPID, out var wipOption);
                var portPropertyStore = wipOption.PortsOptions.FirstOrDefault(p => p.ID == port.Properties.ID);
                portPropertyStore.PortUsable = port.Properties.PortUsable = usable ? clsPortOfRack.PORT_USABLE.USABLE : clsPortOfRack.PORT_USABLE.NOT_USABLE;
                StaEQPManagager.SaveRackConfigs();
                return (true, "");
            }
            else
                return (false, "Port Not Found");
        }

        internal async Task<(bool confirm, string message)> PortNoRename(string wIPID, string portID, string newPortNo)
        {
            if (TryGetPort(wIPID, portID, out clsPortOfRack port))
            {
                StaEQPManagager.RacksOptions.TryGetValue(wIPID, out var wipOption);
                var portPropertyStore = wipOption.PortsOptions.FirstOrDefault(p => p.ID == port.Properties.ID);
                port.Properties.PortNo = portPropertyStore.PortNo = newPortNo;
                StaEQPManagager.SaveRackConfigs();
                return (true, "");
            }
            else
                return (false, "Port Not Found");
        }
        internal async Task UpdateMaterialIDStoreOfDataBase(clsPortOfRack port, string carrierID)
        {
            await UpdateMaterialIDStoreOfDataBase(port.TagNumbers.FirstOrDefault(), port.Properties.Column, carrierID);
        }

        private async Task UpdateMaterialIDStoreOfDataBase(int tagNumber, int slot, string materialID)
        {
            try
            {
                await dbSemaphoreSlim.WaitAsync();
                DateTime updateTime = DateTime.Now;
                foreach (var item in _dbContext.StationStatus.Where(data => data.StationTag == tagNumber.ToString() && data.StationRow == slot).ToList())
                {
                    item.MaterialID = materialID;
                    item.UpdateTime = updateTime;
                    await _dbContext.SaveChangesAsync();
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            finally
            {
                dbSemaphoreSlim.Release();
            }
        }
        private bool TryGetPort(int tagNumber, int slot, out clsPortOfRack port)
        {
            port = null;
            var rack = StaEQPManagager.RacksList.FirstOrDefault(rack => rack.RackOption.ColumnTagMap.SelectMany(tgs => tgs.Value).Contains(tagNumber));


            if (rack == null)
                return false;
            port = rack.PortsStatus.FirstOrDefault(port => port.TagNumbers.Contains(tagNumber) && port.Properties.Row == slot);
            return port != null;
        }

        private bool TryGetPort(string rackName, string portID, out clsPortOfRack port)
        {
            port = null;
            var rack = StaEQPManagager.RacksList.FirstOrDefault(rack => rack.RackOption.Name == rackName);
            if (rack == null)
                return false;

            port = rack.PortsStatus.FirstOrDefault(port => port.Properties.ID == portID);

            return port != null;
        }

        internal List<RackPortAbnoramlStatus> GetAbnormalPortsInfo()
        {
            List<RackPortAbnoramlStatus> abnormalPorts = new List<RackPortAbnoramlStatus>();
            //goal 找出有帳無料、有料無帳的儲格們
            List<clsPortOfRack> hasIDButNoCargoPorts = StaEQPManagager.RackPortsList.Where(port => _IsCarrierIDExist(port) && !_IsCargoExist(port))
                                                                                    .ToList();
            List<clsPortOfRack> hasCargoButNoIDPorts = StaEQPManagager.RackPortsList.Where(port => !_IsCarrierIDExist(port) && _IsCargoExist(port))
                                                                                    .ToList();

            abnormalPorts.AddRange(hasIDButNoCargoPorts.Select(port => new RackPortAbnoramlStatus(port.GetParentRack().EQName, port.Properties.PortNo, "有帳無料")));
            abnormalPorts.AddRange(hasCargoButNoIDPorts.Select(port => new RackPortAbnoramlStatus(port.GetParentRack().EQName, port.Properties.PortNo, "有料無帳")));

            return abnormalPorts;
            bool _IsCargoExist(clsPortOfRack port)
            {
                return port.CargoExist || (port.IsRackPortIsEQ(out clsEQ eq) && eq.EndPointOptions.IsRoleAsZone && eq.Port_Exist);
            }

            bool _IsCarrierIDExist(clsPortOfRack port)
            {
                return !string.IsNullOrEmpty(port.CarrierID) || (port.IsRackPortIsEQ(out clsEQ eq) && eq.EndPointOptions.IsRoleAsZone && !string.IsNullOrEmpty(eq.PortStatus.CarrierID));
            }
        }
    }
}
