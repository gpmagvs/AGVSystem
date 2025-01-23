using AGVSystem.Models.EQDevices;
using AGVSystem.Models.Sys;
using AGVSystemCommonNet6.AGVDispatch.RunMode;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.Microservices.MCS;
using AGVSystemCommonNet6.Microservices.VMS;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using NLog;

namespace AGVSystem.Service.Aggregates
{
    public class SystemModesAggregateService : ISystemModesAggregateService
    {
        Logger logger = LogManager.GetCurrentClassLogger();

        public readonly SystemStatusDbStoreService systemStatusDbStoreService;
        public readonly IHubContext<FrontEndDataHub> hubContext;
        public readonly RackService rackService;
        public SystemModesAggregateService(SystemStatusDbStoreService systemStatusDbStoreService, RackService rackService, IHubContext<FrontEndDataHub> hubContext)
        {
            this.systemStatusDbStoreService = systemStatusDbStoreService;
            this.rackService = rackService;
            this.hubContext = hubContext;
        }

        public async Task<(bool, string)> MaintainRunSwitch(RUN_MODE mode, bool forecing_change = false)
        {
            string message = "";
            var _previousMode = SystemModes.RunMode;
            SystemModes.RunMode = mode == RUN_MODE.MAINTAIN ? RUN_MODE.SWITCH_TO_MAITAIN_ING : RUN_MODE.SWITCH_TO_RUN_ING;
            //AGVS先確認
            bool agvs_confirm = SystemModes.RunModeSwitch(mode, out message, forecing_change);
            if (!agvs_confirm)
            {
                SystemModes.RunMode = _previousMode;
                return (false, message);
            }
            logger.Info($"[Run Mode Switch] 等待VMS回覆 {mode}模式請求");
            (bool confirm, string message) vms_response = await VMSSerivces.RunModeSwitch(mode, forecing_change);
            if (vms_response.confirm == false)
            {
                SystemModes.RunMode = _previousMode;
                message = vms_response.message;
                return (false, message);
            }
            else
            {
                message = vms_response.message;
                SystemModes.RunMode = mode;
                await systemStatusDbStoreService.ModifyRunModeStored(mode);
                return (true, message);
            }
        }
        public async Task<(bool, string)> HostOnlineOfflineModeSwitch(HOST_CONN_MODE mode)
        {
            (bool confirm, string message) response = new(false, "[HostConnMode] Fail");

            if (mode == HOST_CONN_MODE.ONLINE)
                response = await MCSCIMService.Online();
            else
                response = await MCSCIMService.Offline();
            if (response.confirm == true)
            {
                SystemModes.HostConnMode = mode;
                if (SystemModes.HostConnMode == HOST_CONN_MODE.OFFLINE)
                    SystemModes.HostOperMode = HOST_OPER_MODE.LOCAL;
            }
            TryPostCurrentHostModeToCIM();
            return response;
        }

        private async Task TryPostCurrentHostModeToCIM()
        {
            try
            {
                int modeTransfer = 0;
                if (SystemModes.HostConnMode == HOST_CONN_MODE.OFFLINE)
                    modeTransfer = 0;
                else if (SystemModes.HostOperMode == HOST_OPER_MODE.LOCAL)
                    modeTransfer = 1;
                else
                    modeTransfer = 2;
                await GPMCIMService.HostModeState(modeTransfer);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        public async Task<(bool, string)> HostOnlineRemoteLocalModeSwitch(HOST_OPER_MODE mode)
        {
            if (SystemModes.HostConnMode != HOST_CONN_MODE.ONLINE)
                return (false, $"HostConnMode is not ONLINE");
            (bool confirm, string message) response = new(false, "[HostOperationMode] Fail");
            if (mode == HOST_OPER_MODE.LOCAL)
            {
                if (_AnyMCSTransferOrderRunning())
                    return (false, $"有Host任務執行中，無法切換至LOCAL");
                response = await MCSCIMService.OnlineRemote2OnlineLocal();
            }
            else
                response = await MCSCIMService.OnlineLocalToOnlineRemote();
            if (response.confirm == true)
            {
                SystemModes.HostOperMode = mode;
                await NotifyAbnormalyRackPortsStatus();
            }
            bool _AnyMCSTransferOrderRunning()
            {
                return DatabaseCaches.TaskCaches.InCompletedTasks.Any(order => order.isFromMCS);
            }
            TryPostCurrentHostModeToCIM();
            return response;
        }

        private async Task NotifyAbnormalyRackPortsStatus()
        {
            await Task.Delay(1).ContinueWith(async t =>
            {
                List<RackPortAbnoramlStatus> abnormalPorts = rackService.GetAbnormalPortsInfo();
                if (!abnormalPorts.Any())
                    return;

                await Task.Delay(1000);
                await hubContext.Clients.All.SendAsync("RackPortStatusAbnormalNotify", abnormalPorts);
            });
        }
    }
}
