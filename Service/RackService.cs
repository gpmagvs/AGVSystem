using AGVSystem.Models.EQDevices;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.Material;
using AGVSystemCommonNet6.Microservices.MCS;
using EquipmentManagment.MainEquipment;
using EquipmentManagment.Manager;
using EquipmentManagment.WIP;
using Microsoft.EntityFrameworkCore;
using NLog;

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
                Ports = wip.RackOption.MaterialInfoFromEquipment ? wip.GetPortStatusWithEqInfo().ToList() : wip.PortsStatus.ToList(),
                ColumnsTagMap = wip.RackOption.ColumnTagMap,
                IsOvenAsRacks = wip.RackOption.MaterialInfoFromEquipment
            }).OrderBy(wip => wip.WIPName).ToList();
        }


        internal async Task<(bool confirm, string removedCarrierID, string message)> RemoveRackCargoID(string wIPID, string portID, string triggerBy, bool isByAgvUnloadend)
        {
            if (TryGetPort(wIPID, portID, out clsPortOfRack port))
            {
                int tag = port.TagNumbers.FirstOrDefault();
                int slot = port.Properties.Row;
                return await RemoveRackCargoID(tag, slot, triggerBy, isByAgvUnloadend);

            }
            else
                return (false, "", "Port Not Found");


        }
        internal async Task<(bool confirm, string removedCarrierID, string message)> RemoveRackCargoID(int tagNumber, int slot, string triggerBy, bool isByAgvUnloadend)
        {
            try
            {
                string removedCarrierID = "";
                await dbSemaphoreSlim.WaitAsync();

                if (!TryGetPort(tagNumber, slot, out clsPortOfRack port))
                    return (false, "", "Port Not Found"); ;

                bool isEqAsZonePortHasCSTIDReader = port.IsRackPortIsEQ(out clsEQ eq) && eq.EndPointOptions.IsRoleAsZone && eq.EndPointOptions.IsCSTIDReportable;
                if (isEqAsZonePortHasCSTIDReader)
                    return (false, eq.PortStatus.CarrierID, "該Port具有Carrier ID Reader 功能，無法修改帳料資訊");

                removedCarrierID = port.CarrierID + "";

                port.VehicleUnLoadFromPortFlag = isByAgvUnloadend;
                port.CarrierID = string.Empty;


                if (eq != null && eq.EndPointOptions.IsRoleAsZone)
                    eq.PortStatus.CarrierID = string.Empty;

                if (TryGetStationStatus(tagNumber, slot, out clsStationStatus portStatus))
                {
                    portStatus.MaterialID = string.Empty;
                    await _dbContext.SaveChangesAsync();
                }
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
                dbSemaphoreSlim.Release();
            }
        }
        internal async Task<(bool confirm, string message)> AddRackCargoID(string WIPID, string PortID, string cargoID, string triggerBy, bool isByAgvLoadend)
        {
            if (TryGetPort(WIPID, PortID, out clsPortOfRack port))
            {
                int tag = port.TagNumbers.FirstOrDefault();
                int slot = port.Properties.Row;
                return await AddRackCargoID(tag, slot, cargoID, triggerBy, isByAgvLoadend);
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
        internal async Task<(bool confirm, string message)> AddRackCargoID(int tagNumber, int slot, string cargoID, string triggerBy, bool isByAgvLoadend)
        {
            try
            {
                await dbSemaphoreSlim.WaitAsync();
                if (!TryGetPort(tagNumber, slot, out clsPortOfRack port))
                    return (false, "Port Not Found");

                bool isEqAsZonePortHasCSTIDReader = port.IsRackPortIsEQ(out clsEQ eq) && eq.EndPointOptions.IsRoleAsZone && eq.EndPointOptions.IsCSTIDReportable;
                if (isEqAsZonePortHasCSTIDReader)
                    return (false, "該Port具有Carrier ID Reader 功能，無法修改帳料資訊");

                if (eq != null && eq.EndPointOptions.IsRoleAsZone)
                    eq.PortStatus.CarrierID = cargoID;
                port.VehicleLoadToPortFlag = isByAgvLoadend;
                port.CarrierID = cargoID;
                if (TryGetStationStatus(tagNumber, slot, out clsStationStatus portStatus))
                {
                    portStatus.MaterialID = cargoID;
                    await _dbContext.SaveChangesAsync();
                }
                _logger.Info($"WIP:{port.GetParentRack().EQName} Port-{port.Properties.ID} Cargo ID Changed to {cargoID}(Trigger By:{triggerBy})");

                return (true, "");
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return (false, ex.Message);
            }
            finally
            {
                dbSemaphoreSlim.Release();
            }
        }

        internal async Task<(bool confirm, string message)> PortUsableSwitch(string wIPID, string portID, bool usable)
        {
            if (TryGetPort(wIPID, portID, out clsPortOfRack port))
            {
                StaEQPManagager.RacksOptions.TryGetValue(wIPID, out var wipOption);
                var portPropertyStore = wipOption.PortsOptions.FirstOrDefault(p => p.ID == port.Properties.ID);
                portPropertyStore.PortUsable = port.Properties.PortUsable = usable ? clsPortOfRack.PORT_USABLE.USABLE : clsPortOfRack.PORT_USABLE.NOT_USABLE;
                StaEQPManagager.SaveEqConfigs();
                return (true, "");
            }
            else
                return (false, "Port Not Found");
        }
        private bool TryGetStationStatus(int tagNumber, int slot, out clsStationStatus status)
        {
            status = _dbContext.StationStatus.FirstOrDefault(data => data.StationTag == tagNumber.ToString() && data.StationRow == slot);
            return status != null;
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

    }
}
