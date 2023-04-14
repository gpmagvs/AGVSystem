using AGVSystem.Models.TaskAllocation;
using AGVSystem.VMS;
using AGVSystemCommonNet6.DATABASE;
using AGVSytemCommonNet6.HttpHelper;
using AGVSytemCommonNet6.TASK;
using static AGVSytemCommonNet6.clsEnums;

namespace AGVSystem.TaskManagers
{
    public class TaskAllocator
    {
        public static List<clsTaskState> TaskList = new List<clsTaskState>();
     
        public enum TASK_RECIEVE_SOURCE
        {
            LOCAL,
            REMOTE
        }

        public class clsExecuteTaskAck
        {
            public bool HasAGV { get; set; }
            public clsAGV AGV { get; set; }
            public clsTaskDispatchDto taskData { get; set; }
            public class clsAGV
            {
                public AGV_MODEL model { get; set; }
                public string Name { get; set; }
                public ONLINE_STATE online_state { get; set; }
                public MAIN_STATUS main_state { get; }
            }
        }


        public static void AddTask(clsTaskDispatchDto taskData, TASK_RECIEVE_SOURCE source = TASK_RECIEVE_SOURCE.LOCAL)
        {
            
            //var firstTask = taskDbContext.Tasks.First();
            Task.Factory.StartNew(async () =>
            {
                var response = await Http.PostAsync<clsTaskDispatchDto, clsExecuteTaskAck>($"{AppSettings.VMSHost}/api/VmsManager/ExecuteTask", taskData);
                TaskList.Add(new clsTaskState(taskData.TaskName, taskData));
            });
        }
    }
}
