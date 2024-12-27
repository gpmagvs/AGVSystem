
using AGVSystemCommonNet6.Configuration;
using AGVSystemCommonNet6.Microservices.MCS;
using EquipmentManagment.Manager;
using EquipmentManagment.WIP;
using AGVSystem.Models.EQDevices;
using EquipmentManagment.MainEquipment;
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

                    foreach (KeyValuePair<string, List<clsPortOfRack>> doubleCarrierIDCollection in carrierIDOwnerStateMap.Where(pair => pair.Value.Count() > 1))
                    {
                        bool _isTrayDouble = doubleCarrierIDCollection.Key.StartsWith("T");


                        foreach (var port in doubleCarrierIDCollection.Value)
                        {
                            bool isPortHasCstReader = port.IsRackPortIsEQ(out clsEQ eq) && eq.EndPointOptions.IsCSTIDReportable;

                            string newInstallID = "";

                            if (isPortHasCstReader)
                                newInstallID = doubleCarrierIDCollection.Key;
                            else
                                newInstallID = _isTrayDouble ? await AGVSConfigulator.GetDoubleTrayUnknownFlowID() : await AGVSConfigulator.GetDoubleRackUnknownFlowID();

                            await _ModifyCarrierID(port, newInstallID, isPortHasCstReader);


                            await Task.Delay(100);


                        }
                    };
                }
            });
        }

        private async Task _ModifyCarrierID(clsPortOfRack _port, string _newID, bool _isPortHasReader)
        {
            await rackSerivce.AddRackCargoIDManual(_port.GetParentRack().EQName, _port.Properties.ID, _newID, "AutoDetectDoubleIDService", false, _isPortHasReader);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
        }
    }
}
