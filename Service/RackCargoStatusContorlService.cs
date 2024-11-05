using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.Material;
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

    }
}
