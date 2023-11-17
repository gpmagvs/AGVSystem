using AGVSystemCommonNet6;
using AGVSystemCommonNet6.AGVDispatch;
using AGVSystemCommonNet6.AGVDispatch.Messages;
using AGVSystemCommonNet6.Alarm;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.DATABASE.Helpers;
using AGVSystemCommonNet6.Log;

using EquipmentManagment.MainEquipment;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;

namespace AGVSystem.TaskManagers
{
    public class clsLocalAutoTransferTaskMonitor : IDisposable
    {
        private clsTaskDto taskOrder;
        public clsEQ sourceEQ { get; }
        public clsEQ destineEQ { get; }
        private TaskDatabaseHelper taskDatabase;
        private AGVStatusDBHelper agvStatuDatabase;
        private TASK_RUN_STATUS _orderState = TASK_RUN_STATUS.WAIT;
        internal Action OnMonitorEnd;
        private bool disposedValue;

        public TASK_RUN_STATUS orderState
        {
            get => _orderState;
            set
            {
                if (_orderState != value)
                {
                    _orderState = value;
                    LOG.INFO($"[LocalAutoTransferTaskMonitor] Order State Changed To : {value}");
                }
            }
        }
        public clsLocalAutoTransferTaskMonitor(clsTaskDto taskOrder, clsEQ sourceEQ, clsEQ destineEQ)
        {
            this.taskOrder = taskOrder;
            this.sourceEQ = sourceEQ;
            this.destineEQ = destineEQ;
        }

        public async Task Start()
        {
            await Task.Delay(1000);
            await Task.Factory.StartNew(async () =>
            {
                taskDatabase = new TaskDatabaseHelper();
                agvStatuDatabase = new AGVStatusDBHelper();


                async Task<clsTaskDto> GetTaskOrderFromDB()
                {
                    using (var db = new AGVSDatabase())
                    {
                        var _taskOrderInDB = db.tables.Tasks.Where(tk => tk.State == TASK_RUN_STATUS.WAIT | tk.State == TASK_RUN_STATUS.NAVIGATING).FirstOrDefault(tk => tk.TaskName == taskOrder.TaskName && tk.DesignatedAGVName != "");
                        return _taskOrderInDB;
                    }
                };

                //等待任務指派給AGV
                LOG.WARN($"[LocalAutoTransferTaskMonitor] Waiting Task Dispatch To AGV ({taskOrder.TaskName})");
                while (GetTaskOrderFromDB().Result == null)
                {
                    ALARMS EQStatusMonitoringResultAlarmCode = MonitorEQsStatusIO();
                    if (EQStatusMonitoringResultAlarmCode != ALARMS.NONE)
                    {
                        bool canceled = await TaskManager.Cancel(taskOrder.TaskName, EQStatusMonitoringResultAlarmCode.ToString());
                        LOG.WARN($"[LocalAutoTransferTaskMonitor] {taskOrder.TaskName} Monitor End: {EQStatusMonitoringResultAlarmCode}");
                        RaiseMonitorEndInvloke();
                        return;
                    }
                    await Task.Delay(1);
                }

                var _taskOrderInDB = await GetTaskOrderFromDB();
                LOG.INFO($"[LocalAutoTransferTaskMonitor] Task-{taskOrder.TaskName} already dispatched To  {_taskOrderInDB.DesignatedAGVName}");
                //如果AGV正要去充電=>取消AGV充電任務
                var agv_state = agvStatuDatabase.GetAGVStateByAGVName(_taskOrderInDB.DesignatedAGVName);
                if (agv_state.TaskRunAction == ACTION_TYPE.Charge)
                {
                    bool canceled = await TaskManager.Cancel(agv_state.TaskName, $"New Auto Transfer Task Request is rasised");
                }
                //等待AGV開始執行此任務
                LOG.WARN($"[LocalAutoTransferTaskMonitor] Waiting {_taskOrderInDB.DesignatedAGVName} Executing Task-{taskOrder.TaskName}");

                while (agvStatuDatabase.GetAGVStateByAGVName(_taskOrderInDB.DesignatedAGVName).TaskName != taskOrder.TaskName)
                {
                    await Task.Delay(1000);
                    ALARMS EQStatusMonitoringResultAlarmCode = MonitorEQsStatusIO();
                    orderState = await taskDatabase.GetTaskStateByID(taskOrder.TaskName);
                    if (EQStatusMonitoringResultAlarmCode != ALARMS.NONE)
                    {
                        bool canceled = await TaskManager.Cancel(taskOrder.TaskName, EQStatusMonitoringResultAlarmCode.ToString());
                        LOG.WARN($"[LocalAutoTransferTaskMonitor] {taskOrder.TaskName} Monitor End: {EQStatusMonitoringResultAlarmCode}");
                        RaiseMonitorEndInvloke();
                        return;
                    }
                    if (IsOrderFinish(orderState))
                    {
                        LOG.WARN($"[LocalAutoTransferTaskMonitor] {taskOrder.TaskName} Monitor End ,Current Order State ={orderState},AGV Main Status= {agv_state.MainStatus}");
                        RaiseMonitorEndInvloke();
                        return;
                    }
                }
                agv_state = agvStatuDatabase.GetALL().FirstOrDefault(agv => agv.TaskName == taskOrder.TaskName);
                LOG.INFO($"[LocalAutoTransferTaskMonitor] {agv_state.AGV_Name} Prepare to Executing Task-{taskOrder.TaskName}!!!!");
                //var agv_state = agvStatuDatabase.GetAGVStateByName(taskOrder.DesignatedAGVName);
                if (agv_state.MainStatus == clsEnums.MAIN_STATUS.RUN)
                {
                    var runningActionType = await taskDatabase.GetTaskActionTypeByID(agv_state.TaskName);
                    if (runningActionType == ACTION_TYPE.Charge)
                    {
                        LOG.INFO($"[LocalAutoTransferTaskMonitor] Detected AGV Charge Task is running!!");
                        bool canceled = await TaskManager.Cancel(agv_state.TaskName, $"New Auto Transfer Task Request is rasised");
                        LOG.INFO($"[LocalAutoTransferTaskMonitor] {taskOrder.TaskName} cancel request is {(canceled ? "commited!" : "fail....")}");
                    }
                }

                await Task.Factory.StartNew(async () =>
                {
                    LOG.INFO($"[LocalAutoTransferTaskMonitor] {taskOrder.TaskName} Monitor Start! ,Current Order State ={orderState}");
                    while (true)
                    {
                        try
                        {
                            await Task.Delay(1000);
                            orderState = await taskDatabase.GetTaskStateByID(taskOrder.TaskName);
                            agv_state = agvStatuDatabase.GetAGVStateByAGVName(agv_state.AGV_Name);

                            if (IsOrderFinish(orderState) | agv_state.MainStatus == clsEnums.MAIN_STATUS.DOWN)
                            {
                                break;
                            }
                            if (taskOrder.TaskName != agv_state.TaskName)
                                continue;

                            destineEQ.ReserveLow();
                            sourceEQ.ReserveLow();
                            LOG.INFO($"Reserved {sourceEQ.EQName} and {destineEQ.EQName}");
                            if (agv_state.TransferProcess == TRANSFER_PROCESS.GO_TO_SOURCE_EQ | agv_state.TransferProcess == TRANSFER_PROCESS.GO_TO_DESTINE_EQ) //移動途中
                            {
                                if (agv_state.MainStatus == clsEnums.MAIN_STATUS.DOWN)
                                {
                                    //TaskManager.TaskStatusChangeToWait(taskOrder.TaskName, "AGV Down when moving.");
                                    LOG.INFO($"[LocalAutoTransferTaskMonitor] {taskOrder.TaskName} AGV_Down when moving, wait {agv_state.AGV_Name} task-executable...");
                                    bool confirmed = await WaitAGVExecutable(agv_state.AGV_Name);
                                    if (!confirmed)
                                    {
                                        break;
                                    }
                                    LOG.INFO($"[LocalAutoTransferTaskMonitor] {taskOrder.TaskName} AGV restored to ONLINE/IDLE contine");
                                }
                            }
                            ALARMS EQStatusMonitoringResultAlarmCode = MonitorEQsStatusIO();

                            if (EQStatusMonitoringResultAlarmCode == ALARMS.Source_Eq_Unload_Request_Off && agv_state.TransferProcess == TRANSFER_PROCESS.GO_TO_SOURCE_EQ) //前往取貨途中
                            {
                                AlarmManagerCenter.AddAlarmAsync(ALARMS.Source_Eq_Unload_Request_Off, level: ALARM_LEVEL.WARNING, Equipment_Name: sourceEQ.EQName, location: agv_state.CurrentLocation, taskName: taskOrder.TaskName);
                                await TaskManager.Cancel(taskOrder.TaskName, $"Source EQ Unload_Request OFF", TASK_RUN_STATUS.FAILURE);
                                break;
                            }
                            if (EQStatusMonitoringResultAlarmCode == ALARMS.Destine_Eq_Load_Request_Off)
                            {
                                AlarmManagerCenter.AddAlarmAsync(ALARMS.Destine_Eq_Load_Request_Off, level: ALARM_LEVEL.WARNING, Equipment_Name: sourceEQ.EQName, location: agv_state.CurrentLocation, taskName: taskOrder.TaskName);
                                await TaskManager.Cancel(taskOrder.TaskName, $"Destine EQ Load_Request OFF", TASK_RUN_STATUS.FAILURE);
                                break;
                            }
                            if (EQStatusMonitoringResultAlarmCode == ALARMS.Destine_Eq_Status_Down | EQStatusMonitoringResultAlarmCode == ALARMS.Source_Eq_Status_Down)
                            {
                                bool isSourceEQ = !sourceEQ.Eqp_Status_Down;
                                string eqName = isSourceEQ ? sourceEQ.EQName : destineEQ.EQName;
                                AlarmManagerCenter.AddAlarmAsync(EQStatusMonitoringResultAlarmCode, level: ALARM_LEVEL.WARNING, Equipment_Name: eqName, location: agv_state.CurrentLocation, taskName: taskOrder.TaskName);
                                await TaskManager.Cancel(taskOrder.TaskName, isSourceEQ ? "Source EQ Status Down" : $"Destine EQ Status Down", TASK_RUN_STATUS.FAILURE);
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            AlarmManagerCenter.AddAlarmAsync(ALARMS.SYSTEM_ERROR, location: agv_state.CurrentLocation, taskName: taskOrder.TaskName);

                        }

                    }
                    LOG.WARN($"[LocalAutoTransferTaskMonitor] {taskOrder.TaskName} Monitor End ,Current Order State ={orderState},AGV Main Status= {agv_state.MainStatus}");
                    sourceEQ.CancelReserve();
                    destineEQ.CancelReserve();
                    EQTransferTaskManager.UnloadEQQueueing.Remove(sourceEQ);
                    LOG.WARN($"[LocalAutoTransferTaskMonitor] {taskOrder.TaskName} OFF reserve signal of {sourceEQ.EQName} and {destineEQ.EQName}");
                    RaiseMonitorEndInvloke();
                });
            });

        }
        private ALARMS MonitorEQsStatusIO()
        {
            if (!sourceEQ.Unload_Request) //前往取貨途中
            {
                return ALARMS.Source_Eq_Unload_Request_Off;
            }
            else if (!destineEQ.Load_Request)
            {
                return ALARMS.Destine_Eq_Load_Request_Off;
            }
            else if (!sourceEQ.Eqp_Status_Down | !sourceEQ.Eqp_Status_Down)
            {
                bool isSourceEQ = !sourceEQ.Eqp_Status_Down;
                return isSourceEQ ? ALARMS.Source_Eq_Status_Down : ALARMS.Destine_Eq_Status_Down;
            }
            else
            {
                return ALARMS.NONE;
            }
        }

        private void RaiseMonitorEndInvloke()
        {
            if (OnMonitorEnd != null)
            {
                OnMonitorEnd();
            }
        }
        private async Task<bool> WaitAGVExecutable(string aGV_Name)
        {
            CancellationTokenSource cst = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            while (true)
            {
                await Task.Delay(1000);
                var agv_state = agvStatuDatabase.GetAGVStateByAGVName(aGV_Name);
                if (agv_state.OnlineStatus == clsEnums.ONLINE_STATE.ONLINE && agv_state.MainStatus == clsEnums.MAIN_STATUS.IDLE)
                    return true;
                else if (cst.IsCancellationRequested)
                    return false;
            }
        }

        private bool IsOrderFinish(TASK_RUN_STATUS state)
        {
            return state == TASK_RUN_STATUS.ACTION_FINISH | state == TASK_RUN_STATUS.CANCEL | state == TASK_RUN_STATUS.FAILURE | state == TASK_RUN_STATUS.NO_MISSION;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 處置受控狀態 (受控物件)
                }

                // TODO: 釋出非受控資源 (非受控物件) 並覆寫完成項
                // TODO: 將大型欄位設為 Null
                disposedValue = true;
            }
        }

        // // TODO: 僅有當 'Dispose(bool disposing)' 具有會釋出非受控資源的程式碼時，才覆寫完成項
        // ~clsLocalAutoTransferTaskMonitor()
        // {
        //     // 請勿變更此程式碼。請將清除程式碼放入 'Dispose(bool disposing)' 方法
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // 請勿變更此程式碼。請將清除程式碼放入 'Dispose(bool disposing)' 方法
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
