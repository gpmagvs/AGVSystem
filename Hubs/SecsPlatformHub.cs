using AGVSystem.Models.Sys;
using AGVSystemCommonNet6.AGVDispatch.RunMode;
using AGVSystemCommonNet6.Notify;
using Microsoft.AspNetCore.SignalR;

namespace AGVSystem.Hubs
{
    public class SecsPlatformHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            NotifyServiceHelper.SUCCESS($"SECS/GEM Platform is online.");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            NotifyServiceHelper.WARNING($"SECS/GEM Platform is offline.");
            SystemModes.UpdateHosOperModeWhenHostDisconnected();
            SystemModes.HostConnMode = HOST_CONN_MODE.OFFLINE;
            SystemModes.HostOperMode = HOST_OPER_MODE.LOCAL;
            return base.OnDisconnectedAsync(exception);
        }
    }
}
