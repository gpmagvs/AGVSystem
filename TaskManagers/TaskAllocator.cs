using AGVSystemCommonNet6.AGVDispatch.Messages;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.HttpHelper;
using AGVSystemCommonNet6.TASK;
using static AGVSystemCommonNet6.clsEnums;

namespace AGVSystem.TaskManagers
{
    public class TaskAllocator
    {
        public static List<clsTaskDto> TaskList => DatabaseHelper.GetALL();
        public static List<clsTaskDto> InCompletedTaskList => DatabaseHelper.GetALLInCompletedTask();
        public static List<clsTaskDto> CompletedTaskList => DatabaseHelper.GetALLCompletedTask();

        public static TaskDatabaseHelper DatabaseHelper = new TaskDatabaseHelper();

        public enum TASK_RECIEVE_SOURCE
        {
            LOCAL,
            REMOTE
        }

        public class clsExecuteTaskAck
        {
            public bool Confirm { get; set; }
            public clsAGV AGV { get; set; }
            public clsTaskDto taskData { get; set; }
            public class clsAGV
            {
                public AGV_MODEL model { get; set; }
                public string Name { get; set; }
                public ONLINE_STATE online_state { get; set; }
                public MAIN_STATUS main_state { get; }
            }
        }


        public static void AddTask(clsTaskDto taskData, TASK_RECIEVE_SOURCE source = TASK_RECIEVE_SOURCE.LOCAL)
        {
            Task.Factory.StartNew(async () =>
            {
                
                clsExecuteTaskAck response = await Http.PostAsync<clsTaskDto, clsExecuteTaskAck>($"{AppSettings.VMSHost}/api/VmsManager/ExecuteTask", taskData);
                taskData = response.taskData;
                taskData.RecieveTime = DateTime.Now;
                taskData.State = response.Confirm ? TASK_RUN_STATUS.WAIT : TASK_RUN_STATUS.FAILURE;
                DatabaseHelper.Add(taskData);
            });
        }

        internal static bool Cancel(string task_name)
        {
            return DatabaseHelper.DeleteTask(task_name);
        }
    }
}
