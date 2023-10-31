using AGVSystemCommonNet6;
using AGVSystemCommonNet6.AGVDispatch.Messages;
using AGVSystemCommonNet6.Alarm;
using AGVSystemCommonNet6.DATABASE.Helpers;
using AGVSystemCommonNet6.Log;
using AGVSystemCommonNet6.TASK;
using EquipmentManagment.MainEquipment;
using Newtonsoft.Json.Linq;

namespace AGVSystem.TaskManagers
{
    public class clsLocalAutoTransferTaskMonitor
    {
        private clsTaskDto taskOrder;
        public clsEQ sourceEQ { get; }
        public clsEQ destineEQ { get; }
        private TaskDatabaseHelper taskDatabase;
        private AGVStatusDBHelper agvStatuDatabase;
        private TASK_RUN_STATUS _orderState = TASK_RUN_STATUS.WAIT;

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

        public async void Start()
        {
            await Task.Delay(1000);
            taskDatabase = new TaskDatabaseHelper();
            agvStatuDatabase = new AGVStatusDBHelper();
            var agv_state = agvStatuDatabase.GetAGVStateByName(taskOrder.DesignatedAGVName);
            if (agv_state.MainStatus == AGVSystemCommonNet6.clsEnums.MAIN_STATUS.RUN)
            {
                var runningActionType = await taskDatabase.GetTaskActionTypeByID(agv_state.TaskName);
                if (runningActionType == ACTION_TYPE.Charge)
                {
                    LOG.INFO($"[LocalAutoTransferTaskMonitor] Detected AGV Charge Task is running!!");
                    bool canceled = TaskManager.Cancel(agv_state.TaskName, $"New Auto Transfer Task Request is rasised");
                    LOG.INFO($"[LocalAutoTransferTaskMonitor] {taskOrder.TaskName} cancel request is {(canceled ? "commited!" : "fail....")}");
                }
            }

            await Task.Factory.StartNew(async () =>
            {
                LOG.INFO($"[LocalAutoTransferTaskMonitor] {taskOrder.TaskName} Monitor Start! ,Current Order State ={orderState}");
                while (true)
                {
                    await Task.Delay(1000);
                    orderState = await taskDatabase.GetTaskStateByID(taskOrder.TaskName);
                    agv_state = agvStatuDatabase.GetAGVStateByName(taskOrder.DesignatedAGVName);

                    if (IsOrderFinish(orderState) | agv_state.MainStatus == clsEnums.MAIN_STATUS.DOWN)
                    {
                        break;
                    }
                    if (taskOrder.TaskName != agv_state.TaskName)
                        continue;

                    destineEQ.ReserveLow();
                    sourceEQ.ReserveLow();
                    LOG.INFO($"Reserved {sourceEQ.EQName} and {destineEQ.EQName}");

                    if (!sourceEQ.Unload_Request && agv_state.TransferProcess == TRANSFER_PROCESS.GO_TO_SOURCE_EQ)
                    {
                        AlarmManagerCenter.AddAlarm(ALARMS.Source_Eq_Unload_Request_Off, level: ALARM_LEVEL.WARNING, Equipment_Name: sourceEQ.EQName, location: agv_state.CurrentLocation, taskName: taskOrder.TaskName);
                        TaskManager.Cancel(taskOrder.TaskName, $"Source EQ Unload_Request OFF");
                        break;
                    }
                    if (!destineEQ.Load_Request)
                    {
                        AlarmManagerCenter.AddAlarm(ALARMS.Destine_Eq_Load_Request_Off, level: ALARM_LEVEL.WARNING, Equipment_Name: sourceEQ.EQName, location: agv_state.CurrentLocation, taskName: taskOrder.TaskName);
                        TaskManager.Cancel(taskOrder.TaskName, $"Destine EQ Load_Request OFF");
                        break;
                    }
                    if (!sourceEQ.Eqp_Status_Down | !sourceEQ.Eqp_Status_Down)
                    {
                        bool isSourceEQ = !sourceEQ.Eqp_Status_Down;
                        string eqName = isSourceEQ ? sourceEQ.EQName : destineEQ.EQName;
                        AlarmManagerCenter.AddAlarm(isSourceEQ ? ALARMS.Source_Eq_Status_Down : ALARMS.Destine_Eq_Status_Down, level: ALARM_LEVEL.WARNING, Equipment_Name: eqName, location: agv_state.CurrentLocation, taskName: taskOrder.TaskName);
                        TaskManager.Cancel(taskOrder.TaskName, isSourceEQ ? "Source EQ Status Down" : $"Destine EQ Status Down");
                        break;
                    }
                }
                LOG.INFO($"[LocalAutoTransferTaskMonitor] {taskOrder.TaskName} Monitor End ,Current Order State ={orderState},AGV Main Status= {agv_state.MainStatus}");
                sourceEQ.CancelReserve();
                destineEQ.CancelReserve();
                EQTransferTaskManager.UnloadEQQueueing.Remove(sourceEQ);
                LOG.INFO($"[LocalAutoTransferTaskMonitor] {taskOrder.TaskName} OFF reserve signal of {sourceEQ.EQName} and {destineEQ.EQName}");
            });
        }

        private bool IsOrderFinish(TASK_RUN_STATUS state)
        {
            return state == TASK_RUN_STATUS.ACTION_FINISH | state == TASK_RUN_STATUS.CANCEL | state == TASK_RUN_STATUS.FAILURE | state == TASK_RUN_STATUS.NO_MISSION;
        }
    }
}
