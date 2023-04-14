using AGVSystem.Models.TaskAllocation;
using AGVSystem.VMS;
using AGVSystemCommonNet6.DATABASE;
using AGVSytemCommonNet6.HttpHelper;
using AGVSytemCommonNet6.TASK;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using static AGVSytemCommonNet6.clsEnums;

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
                if (response.Confirm)
                {
                    taskData.RecieveTime = DateTime.Now;
                    DatabaseHelper.AddTask(taskData);
                }
            });
        }

        internal static bool Cancel(string task_name)
        {
            return DatabaseHelper.DeleteTask(task_name);
        }
    }
}
