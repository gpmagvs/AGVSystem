using AGVSystem.TaskManagers;
using AGVSystemCommonNet6.AGVDispatch.Messages;
using AGVSystemCommonNet6.AGVDispatch.RunMode;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.Log;

namespace AGVSystem.Models.Sys
{
    public static class SystemModes
    {
        internal static Action OnRunModeON;
        internal static Action OnRunModeOFF;
        private static RUN_MODE _RunMode = RUN_MODE.MAINTAIN;
        private static TRANSFER_MODE _TransferTaskMode = TRANSFER_MODE.MANUAL;

        internal static RUN_MODE RunMode
        {
            get => _RunMode;
            set
            {
                if (_RunMode != value)
                {
                    _RunMode = value;
                    switch (_RunMode)
                    {
                        case RUN_MODE.MAINTAIN:
                            OnRunModeOFF();
                            TransferTaskMode = TRANSFER_MODE.MANUAL;
                            break;
                        case RUN_MODE.RUN:
                            OnRunModeON();
                            break;
                        case RUN_MODE.SWITCH_TO_MAITAIN_ING:
                            break;
                        case RUN_MODE.SWITCH_TO_RUN_ING:
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        internal static HOST_CONN_MODE HostConnMode { get; set; }
        internal static HOST_OPER_MODE HostOperMode { get; set; }

        internal static TRANSFER_MODE TransferTaskMode
        {
            get => _TransferTaskMode;
            set
            {
                if (_TransferTaskMode != value)
                {
                    _TransferTaskMode = value;
                    LOG.INFO($"Transfer Mode Changed to {_TransferTaskMode}");
                }
                if (_TransferTaskMode == TRANSFER_MODE.MANUAL)
                {
                    List<string> list_waiting_task = new List<string>();
                    using (var db = new AGVSDatabase())
                    {
                        list_waiting_task = db.tables.Tasks.Where(tk => tk.State == TASK_RUN_STATUS.WAIT).Select(x => x.TaskName).ToList();
                    }
                    foreach (var task_name in list_waiting_task)
                    {
                        Task<bool> canceled = TaskManager.Cancel(task_name, $"Transfer Mode Changed to {_TransferTaskMode} canceled");
                        canceled.Wait();
                    }
                }
            }
        }
        public static bool RunModeSwitch(RUN_MODE mode, out string Message, bool forecing_change = false)
        {
            Message = "";
            bool confirm = true;
            LOG.INFO($"User Try Swich RUN_MODE To {mode}");

            if (!forecing_change)
            {
                bool isAnyTaskExecuting = DatabaseCaches.TaskCaches.RunningTasks.Count() > 0;
                if (isAnyTaskExecuting)
                {
                    Message = "尚有任務在執行中";
                    confirm = false;
                }
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
