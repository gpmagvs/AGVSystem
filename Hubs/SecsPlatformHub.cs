using AGVSystem.Models.Sys;
using AGVSystem.Service.Aggregates;
using AGVSystemCommonNet6.AGVDispatch.RunMode;
using AGVSystemCommonNet6.Notify;
using Microsoft.AspNetCore.SignalR;

namespace AGVSystem.Hubs
{
    public class SecsPlatformHub : Hub
    {
        SystemModesAggregateService _systemModesAggregate;
        public SecsPlatformHub(SystemModesAggregateService systemModesAggregate)
        {
            _systemModesAggregate = systemModesAggregate;
        }
        public override Task OnConnectedAsync()
        {
            NotifyServiceHelper.SUCCESS($"SECS/GEM Platform is online.");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            NotifyServiceHelper.WARNING($"SECS/GEM Platform is offline.");
            _systemModesAggregate.HostDisconnectNotify();
            return base.OnDisconnectedAsync(exception);
        }
    }
}
