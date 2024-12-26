
using AGVSystemCommonNet6.Configuration;
using AGVSystemCommonNet6.Microservices.MCS;
using EquipmentManagment.Manager;
using EquipmentManagment.WIP;

namespace AGVSystem.Service
{
    public class RackPortDoubleIDMonitor : IHostedService
    {
        RackService rackSerivce;
        public RackPortDoubleIDMonitor(IServiceScopeFactory factory)
        {
            this.rackSerivce = factory.CreateScope().ServiceProvider.GetRequiredService<RackService>();
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(1).ContinueWith(async t =>
            {
                while (true)
                {

                    await Task.Delay(1000);

                    List<string> currentCarrierIDExist = StaEQPManagager.RackPortsList.Where(port => !string.IsNullOrEmpty(port.CarrierID))
                                                                                      .Select(port => port.CarrierID)
                                                                                      .Distinct()
                                                                                      .ToList();
                    Dictionary<string, List<clsPortOfRack>> carrierIDOwnerStateMap = currentCarrierIDExist.ToDictionary(carrierID => carrierID, carrierID => StaEQPManagager.RackPortsList.Where(port => port.CarrierID == carrierID).ToList());

                    foreach (var doubleCarrierIDCollection in carrierIDOwnerStateMap.Where(pair => pair.Value.Count() > 1))
                    {
                        bool _isTrayDouble = doubleCarrierIDCollection.Key.StartsWith("T");

                        foreach (var ports in doubleCarrierIDCollection.Value)
                        {
                            string doubleUID = _isTrayDouble ? await AGVSConfigulator.GetDoubleTrayUnknownFlowID() : await AGVSConfigulator.GetDoubleRackUnknownFlowID();
                            await rackSerivce.AddRackCargoIDManual(ports.GetParentRack().EQName, ports.Properties.ID, doubleUID, "AutoDetectDoubleIDService", false);
                            await Task.Delay(100);
                        }
                    };
                }
            });
        }



        public async Task StopAsync(CancellationToken cancellationToken)
        {
        }
    }
}
