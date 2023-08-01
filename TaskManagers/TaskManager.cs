using AGVSystem.Models.Map;
using AGVSystem.Models.Sys;
using AGVSystemCommonNet6;
using AGVSystemCommonNet6.AGVDispatch.Messages;
using AGVSystemCommonNet6.Alarm;
using AGVSystemCommonNet6.Alarm.VMS_ALARM;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.HttpHelper;
using AGVSystemCommonNet6.Log;
using AGVSystemCommonNet6.MAP;
using AGVSystemCommonNet6.TASK;
using EquipmentManagment;
using System.Security.AccessControl;
using static AGVSystemCommonNet6.clsEnums;

namespace AGVSystem.TaskManagers
{
    public class TaskManager
    {
        public static List<clsTaskDto> TaskList => DatabaseHelper.GetALL();

        public static List<clsTaskDto> InCompletedTaskList => DatabaseHelper.GetALLInCompletedTask();
        public static List<clsTaskDto> CompletedTaskList => DatabaseHelper.GetALLCompletedTask(20);

        public static TaskDatabaseHelper DatabaseHelper = new TaskDatabaseHelper();

        public enum TASK_RECIEVE_SOURCE
        {
            LOCAL,
            MANUAL,
            REMOTE,
        }
        internal static void Initialize()
        {

        }
        public static async Task<Tuple<bool, ALARMS>> AddTask(clsTaskDto taskData, TASK_RECIEVE_SOURCE source = TASK_RECIEVE_SOURCE.LOCAL)
        {
            //if (taskData.Action == ACTION_TYPE.Load | taskData.Action == ACTION_TYPE.LoadAndPark | taskData.Action == ACTION_TYPE.Unload | taskData.Action == ACTION_TYPE.Carry)
            //{
            //    Tuple<bool, ALARMS> results = CheckEQLDULDStatus(taskData.Action, int.Parse(taskData.From_Station), int.Parse(taskData.To_Station));
            //    if (!results.Item1)
            //        return results;
            //}
            try
            {
                //    clsExecuteTaskAck response = await Http.PostAsync<clsTaskDto, clsExecuteTaskAck>($"{AppSettings.VMSHost}/api/VmsManager/ExecuteTask", taskData);
                //    taskData = response.taskData;
                //    taskData.RecieveTime = DateTime.Now;
                //    taskData.State = response.Confirm ? TASK_RUN_STATUS.WAIT : TASK_RUN_STATUS.FAILURE;
                DatabaseHelper.Add(taskData);
                return new(true, ALARMS.NONE);
            }
            catch (HttpRequestException ex)
            {
                AlarmManagerCenter.AddAlarm(ALARMS.TRANSFER_TASK_TO_VMS_BUT_ERROR_OCCUR, ALARM_SOURCE.AGVS);
                return new(false, ALARMS.TRANSFER_TASK_TO_VMS_BUT_ERROR_OCCUR);

            }
        }


        internal static bool Cancel(string task_name)
        {
            return DatabaseHelper.DeleteTask(task_name);
        }


    }
}
