using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.Material;
using AGVSystemCommonNet6.Microservices.MCS;
using EquipmentManagment.Manager;
using EquipmentManagment.WIP;
using NLog;

namespace AGVSystem.Service
{
    public class RackCargoStatusContorlService
    {
        private static SemaphoreSlim dbSemaphoreSlim = new SemaphoreSlim(1, 1);
        private readonly AGVSDbContext _dbContext;
        private Logger _logger = LogManager.GetCurrentClassLogger();
        public RackCargoStatusContorlService(AGVSDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        internal async Task RemoveRackCargoID(string wIPID, string portID, string triggerBy)
        {
            if (TryGetPort(wIPID, portID, out clsPortOfRack port))
            {
                int tag = port.TagNumbers.FirstOrDefault();
                int slot = port.Properties.Row;
                await RemoveRackCargoID(tag, slot, triggerBy);
            }
        }
        internal async Task RemoveRackCargoID(int tagNumber, int slot, string triggerBy)
        {
            try
            {
                await dbSemaphoreSlim.WaitAsync();
                if (TryGetPort(tagNumber, slot, out clsPortOfRack port))
                {
                    port.CarrierID = string.Empty;
                    _logger.Info($"WIP:{port.GetParentRack().EQName} Port-{port.Properties.ID} Cargo ID Removed. (Trigger By:{triggerBy})");
                    if (TryGetStationStatus(tagNumber, slot, out clsStationStatus portStatus))
                    {
                        portStatus.MaterialID = string.Empty;
                    }
                    await _dbContext.SaveChangesAsync();

                    var rackData=StaEQPManagager.GetRackDataForMCS();

                    await MCSCIMService.ShelfStatusChange(rackData);
                }
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
        internal async Task AddRackCargoID(string WIPID, string PortID, string cargoID, string triggerBy)
        {
            if (TryGetPort(WIPID, PortID, out clsPortOfRack port))
            {
                int tag = port.TagNumbers.FirstOrDefault();
                int slot = port.Properties.Row;
                await AddRackCargoID(tag, slot, cargoID, triggerBy);
            }
        }
        internal async Task AddRackCargoID(int tagNumber, int slot, string cargoID, string triggerBy)
        {
            try
            {
                await dbSemaphoreSlim.WaitAsync();
                if (TryGetPort(tagNumber, slot, out clsPortOfRack port))
                {
                    port.CarrierID = cargoID;
                    _logger.Info($"WIP:{port.GetParentRack().EQName} Port-{port.Properties.ID} Cargo ID Changed to {cargoID}(Trigger By:{triggerBy})");
                    if (TryGetStationStatus(tagNumber, slot, out clsStationStatus portStatus))
                    {
                        portStatus.MaterialID = cargoID;
                    }
                    await _dbContext.SaveChangesAsync();
                    await MCSCIMService.ShelfStatusChange();
                }
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
