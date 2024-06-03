
using AGVSystemCommonNet6.DATABASE;
using EquipmentManagment.ChargeStation;
using EquipmentManagment.Manager;

namespace AGVSystem.Service
{
    public class VehicleLocationMonitorBackgroundService : BackgroundService
    {

        private Dictionary<string, int> _VehicleCurrentTagStore = new Dictionary<string, int>();

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine($"VehicleLocationMonitorBackgroundService Start ExecuteAsync");

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000);

                try
                {
                    foreach (AGVSystemCommonNet6.clsAGVStateDto vehicleState in DatabaseCaches.Vehicle.VehicleStates)
                    {
                        int currentTag = int.Parse(vehicleState.CurrentLocation);
                        string vehicleName = vehicleState.AGV_Name;

                        if (_VehicleCurrentTagStore.TryGetValue(vehicleName, out int previousTag))
                        {
                            if (previousTag != currentTag)
                            {
                                _VehicleCurrentTagStore[vehicleName] = currentTag;
                                TryUpdateChargeStationVehicleUsing(currentTag, vehicleName, out clsChargeStation chargeStation);
                                Console.WriteLine($"{vehicleName} Tag Change to {currentTag} (Previous={previousTag})");
                            }
                        }
                        else
                        {
                            _VehicleCurrentTagStore.Add(vehicleName, currentTag);
                            Console.WriteLine($"{vehicleName} Tag Change to {currentTag}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message} {ex.StackTrace}");
                }

            }

        }

        private bool TryUpdateChargeStationVehicleUsing(int currentTag, string vehicleName, out clsChargeStation chargeStation)
        {
            chargeStation = null;
            Dictionary<int, clsChargeStation> _ChargeStationMap = StaEQPManagager.ChargeStations.ToDictionary(v => v.EndPointOptions.TagID, v => v);
            if (_ChargeStationMap.TryGetValue(currentTag, out chargeStation))
            {
                chargeStation.UpdateUserVehicleName(vehicleName);
            }
            return chargeStation != null;
        }
    }
}
