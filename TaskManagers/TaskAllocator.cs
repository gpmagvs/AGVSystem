using AGVSystem.Models.TaskAllocation;
using AGVSystem.VMS;

namespace AGVSystem.TaskManagers
{
    public class TaskAllocator
    {

        public enum TASK_RECIEVE_SOURCE
        {
            LOCAL,
            REMOTE
        }

        private static clsForkAGVTaskFactory ForAGVTaskFactory = new clsForkAGVTaskFactory();
        private static clsYunTechForkAGVTaskFactory YunTechForkAGVTaskFactory = new clsYunTechForkAGVTaskFactory();
        //clsTaskBaseDto

        public static void AddTask(clsTaskBaseDto taskData, TASK_RECIEVE_SOURCE source = TASK_RECIEVE_SOURCE.LOCAL)
        {
            VMSEntity agv = VMSManager.VMSList.FirstOrDefault(agv => agv.Value.BaseProps.AGV_Name == taskData.AGV_Name).Value;

            if (agv.agv_model == AGVSytemCommon.clsEnums.AGV_MODEL.FORK_AGV)
            {
                List<AGVSytemCommon.AGVMessage.cls_0301_TaskDownloadHeader> taskLinks = ForAGVTaskFactory.CreateTaskLink(taskData, agv.Running_Status.Last_Visited_Node, source);

                agv.JoinTask(taskLinks);
            }
            else if (agv.agv_model == AGVSytemCommon.clsEnums.AGV_MODEL.YUNTECH_FORK_AGV)
            {

            }
        }

    }
}
