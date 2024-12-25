using EquipmentManagment.Manager;
using Microsoft.AspNetCore.SignalR;

namespace AGVSystem.Models.EQDevices
{
    public partial class EQDeviceEventsHandler
    {
        private static async Task BrocastEQkData()
        {
            try
            {
                await _EQDataBrocastSemaphoreSlim.WaitAsync();
                await HubContext?.Clients.All.SendAsync("EQDataChanged", StaEQPManagager.GetEQStates());
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _EQDataBrocastSemaphoreSlim.Release();
            }
        }
        internal static async Task BrocastRackData()
        {
            try
            {
                await _RackDataBrocastSemaphoreSlim.WaitAsync();
                var _data = rackService.GetWIPDataViewModels();
                await HubContext?.Clients.All.SendAsync("RackDataChanged", _data);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _RackDataBrocastSemaphoreSlim.Release();
            }
        }
    }
}
