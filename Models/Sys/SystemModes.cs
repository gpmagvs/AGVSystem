using AGVSystem.TaskManagers;
using AGVSystemCommonNet6.AGVDispatch.Messages;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.Log;

namespace AGVSystem.Models.Sys
{
    public static class SystemModes
    {
        internal static Action OnRunModeON;
        internal static Action OnRunModeOFF;
        private static RUN_MODE _RunMode = RUN_MODE.MAINTAIN;
        internal static RUN_MODE RunMode
        {
            get => _RunMode;
            set
            {
                if (_RunMode != value)
                {
                    _RunMode = value;
                    if (_RunMode == RUN_MODE.RUN)
                        OnRunModeON();
                    else
                        OnRunModeOFF();
                }
            }
        }
        internal static HOST_CONN_MODE HostConnMode { get; set; }
        internal static HOST_OPER_MODE HostOperMode { get; set; } 

        public static bool RunModeSwitch(RUN_MODE mode, out string Message)
        {
            Message = "";
            bool confirm = true;
            LOG.INFO($"User Try Swich RUN_MODE To {mode}");

            bool isAnyTaskExecuting = TaskManager.InCompletedTaskList.Any(task => task.State == TASK_RUN_STATUS.NAVIGATING);

            if (isAnyTaskExecuting)
            {
                Message = "尚有任務在執行中";
                confirm = false;
            }

            if (confirm)
            {
                RunMode = mode;
            }
            LOG.INFO($"RUN_MODE Switch To {mode} {(confirm ? "SUCCESS" : "FAIL")} : {Message}");
            return confirm;
        }
    }
}
