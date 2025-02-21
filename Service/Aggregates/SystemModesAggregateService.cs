using AGVSystem.Models.EQDevices;
using AGVSystem.Models.Sys;
using AGVSystemCommonNet6.AGVDispatch.RunMode;
using AGVSystemCommonNet6.Alarm;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.Microservices.MCS;
using AGVSystemCommonNet6.Microservices.VMS;
using AGVSystemCommonNet6.Notify;
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

        public async Task<(bool, string)> HostOnlineOfflineModeSwitch(HOST_CONN_MODE mode, bool bypassRunModeCheck = false)
        {
            (bool confirm, string message) response = new(false, "[HostConnMode] Fail");

            if (mode == HOST_CONN_MODE.ONLINE && !bypassRunModeCheck && SystemModes.RunMode != RUN_MODE.RUN)
                return (false, "請切換為'運轉模式'後再嘗試 ONLINE (Please switch to 'RUN mode' and then try again ONLINE.)");

            if (mode == HOST_CONN_MODE.ONLINE)
                response = await MCSCIMService.Online(3);
            else
                response = await MCSCIMService.Offline(1);
            if (response.confirm == true || mode == HOST_CONN_MODE.OFFLINE)
            {
                SystemModes.HostConnMode = mode;
                if (SystemModes.HostConnMode == HOST_CONN_MODE.OFFLINE)
                {
                    SystemModes.HostOperMode = HOST_OPER_MODE.LOCAL;
                    response.confirm = true;
                }
            }
            TryPostCurrentHostModeToCIM();
            return response;
        }

        public async Task<(bool, string)> HostOnlineRemoteLocalModeSwitch(HOST_OPER_MODE mode)
        {

            if (mode == HOST_OPER_MODE.REMOTE && SystemModes.HostConnMode != HOST_CONN_MODE.ONLINE)
            {
                // Try online first
                (bool autoOnlineSuccsee, string message) = await HostOnlineOfflineModeSwitch(HOST_CONN_MODE.ONLINE);
                if (!autoOnlineSuccsee || SystemModes.HostConnMode != HOST_CONN_MODE.ONLINE)
                    return (false, $"HostConnMode is not ONLINE {(string.IsNullOrEmpty(message) ? "" : $"({message})")}");
            }

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
            else if (mode == HOST_OPER_MODE.LOCAL)
            {
                SystemModes.HostOperMode = HOST_OPER_MODE.LOCAL;
                SystemModes.HostConnMode = HOST_CONN_MODE.OFFLINE;
                return (true, "Host Connection Error, Now is OFFLine/Local");
            }

            bool _AnyMCSTransferOrderRunning()
            {
                return DatabaseCaches.TaskCaches.InCompletedTasks.Any(order => order.isFromMCS);
            }
            TryPostCurrentHostModeToCIM();
            return response;
        }


        internal async Task<(bool confirm, string message)> HostDisconnectNotify(ALARMS alarmCode = ALARMS.HostCommunicationError, string source = "HOST")
        {
            ALARM_LEVEL alarmLevel = alarmCode == ALARMS.SecsPlatformNotRun ? ALARM_LEVEL.WARNING : ALARM_LEVEL.ALARM;
            await AlarmManagerCenter.AddAlarmAsync(alarmCode, level: alarmLevel, Equipment_Name: source);
            SystemModes.UpdateHosOperModeWhenHostDisconnected();
            try
            {
                await systemStatusDbStoreService.ModifyHostModeStored(HOST_CONN_MODE.OFFLINE, HOST_OPER_MODE.LOCAL);
                SystemModes.HostConnMode = HOST_CONN_MODE.OFFLINE;
                SystemModes.HostOperMode = HOST_OPER_MODE.LOCAL;
                HostOnlineOfflineModeSwitch(HOST_CONN_MODE.OFFLINE, true);
                return (true, "OK");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }

        }

        internal async Task<(bool confirm, string message)> HosConnectionRestoredNotify()
        {
            if (SystemModes.HosOperModeWhenHostDisconnected == HOST_OPER_MODE.REMOTE)
            {
                SystemModes.UpdateHosOperModeWhenHostDisconnected();
                TryRestoreToRemoteAutomaticallyAsync();
            }
            await AlarmManagerCenter.SetAlarmCheckedAsync("HOST", ALARMS.HostCommunicationError);
            await AlarmManagerCenter.SetAlarmCheckedAsync("SECS Platform", ALARMS.SecsPlatformNotRun);
            return (true, "OK");
        }

        private async Task TryRestoreToRemoteAutomaticallyAsync()
        {
            await Task.Run(async () =>
            {

                string autoRemoteRejectionMessgge = string.Empty;
                //確認是否有任務
                if (DatabaseCaches.TaskCaches.RunningTasks.Any(order => order.Action == AGVSystemCommonNet6.AGVDispatch.Messages.ACTION_TYPE.Carry))
                    autoRemoteRejectionMessgge += "尚有任務仍在進行中";

                //確認是否有AGV載著貨異常貨OR IDLE

                if (GetVehicleNotRunWithCargo(out List<string> agvNames))
                {
                    autoRemoteRejectionMessgge += ";須將AGV車上的貨物移除";
                }


                if (!String.IsNullOrEmpty(autoRemoteRejectionMessgge))
                {
                    _ = hubContext.Clients.All.SendAsync("TrySwitchToRemoteWhenHostReConnectedButConditionNotAllow", autoRemoteRejectionMessgge);
                    return;
                }

                logger.Trace("[Host狀態自動賦歸] Start try online->remote after host connection restored.");
                (bool onlineSuccess, string message) = await HostOnlineOfflineModeSwitch(HOST_CONN_MODE.ONLINE);
                logger.Trace($"[Host狀態自動賦歸] Switch host 'Online' result : {(onlineSuccess ? "Success" : $"Fail,{message}")}");
                if (!onlineSuccess)
                {
                    NotifyServiceHelper.WARNING($"[Host狀態自動賦歸] Host 重新 Online 未成功:{message}");
                    return;
                }
                NotifyServiceHelper.SUCCESS($"[Host狀態自動賦歸] Host 重新 Online成功!");

                (bool remoteSuccess, message) = await HostOnlineRemoteLocalModeSwitch(HOST_OPER_MODE.REMOTE);
                logger.Trace($"[Host狀態自動賦歸] Switch host 'Remote' result : {(remoteSuccess ? "Success" : $"Fail,{message}")}");
                if (remoteSuccess)
                    NotifyServiceHelper.SUCCESS($"[Host狀態自動賦歸] Host 重新 Remote成功!");
                else
                    NotifyServiceHelper.WARNING($"[Host狀態自動賦歸] Host 重新 Remote 未成功:{message}");
                //confirm remote able
            });

            bool GetVehicleNotRunWithCargo(out List<string> agvNames)
            {
                agvNames = new();
                var withCargoAgvStates = DatabaseCaches.Vehicle.VehicleStates.Where(agv => agv.CargoStatus == 1);
                agvNames = withCargoAgvStates.Select(state => state.AGV_Name).ToList();
                return agvNames.Any();
            }
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
