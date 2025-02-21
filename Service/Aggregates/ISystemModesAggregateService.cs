using AGVSystemCommonNet6.AGVDispatch.RunMode;

namespace AGVSystem.Service.Aggregates
{
    public interface ISystemModesAggregateService
    {
        Task<(bool, string)> MaintainRunSwitch(RUN_MODE mode, bool forecing_change = false);

        Task<(bool, string)> HostOnlineOfflineModeSwitch(HOST_CONN_MODE mode, bool bypassRunModeCheck = false);

        Task<(bool, string)> HostOnlineRemoteLocalModeSwitch(HOST_OPER_MODE mode);

    }
}
