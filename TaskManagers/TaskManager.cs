using AGVSystem.Models.BayMeasure;
using AGVSystem.Models.Sys;
using AGVSystemCommonNet6.AGVDispatch.Messages;
using AGVSystemCommonNet6.AGVDispatch.RunMode;
using AGVSystemCommonNet6.Alarm;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.DATABASE.Helpers;
using AGVSystemCommonNet6.Log;
using AGVSystemCommonNet6.TASK;
using Microsoft.EntityFrameworkCore;

namespace AGVSystem.TaskManagers
{
    public class TaskManager
    {


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

        public static async Task<(bool confirm, ALARMS alarm_code)> AddTask(clsTaskDto taskData, TASK_RECIEVE_SOURCE source = TASK_RECIEVE_SOURCE.LOCAL)
        {
            if (SystemModes.RunMode == RUN_MODE.RUN)
            {

                if (taskData.Action == ACTION_TYPE.Load | taskData.Action == ACTION_TYPE.LoadAndPark | taskData.Action == ACTION_TYPE.Unload | taskData.Action == ACTION_TYPE.Carry)
                {

                    (bool confirm, ALARMS alarm_code) results = EQTransferTaskManager.CheckEQLDULDStatus(taskData.Action, int.Parse(taskData.From_Station), int.Parse(taskData.To_Station));

                    if (!results.confirm)
                        return results;
                }
            }

            try
            {
                taskData.RecieveTime = DateTime.Now;
                await Task.Delay(200);
                using (var db = new AGVSDatabase())
                {

                    if(db.tables.Tasks.AsNoTracking().Where(task=>task.State == TASK_RUN_STATUS.WAIT| task.State== TASK_RUN_STATUS.NAVIGATING).Any(task=>task.To_Station== taskData.To_Station))
                    {
                        AlarmManagerCenter.AddAlarm(ALARMS.Source_Eq_Already_Has_Task_To_Excute, ALARM_SOURCE.AGVS);
                        return (false, ALARMS.Source_Eq_Already_Has_Task_To_Excute);
                    }
                    db.tables.Tasks.Add(taskData);
                    db.SaveChanges();
                }
                return new(true, ALARMS.NONE);
            }
            catch (Exception ex)
            {
                LOG.ERROR(ex);
                AlarmManagerCenter.AddAlarm(ALARMS.Task_Add_To_Database_Fail, ALARM_SOURCE.AGVS);
                return new(false, ALARMS.Task_Add_To_Database_Fail);
            }
        }


        internal static bool Cancel(string task_name, string reason = "")
        {
            try
            {
                using (var db = new AGVSDatabase())
                {
                    var task = db.tables.Tasks.Where(tk => tk.TaskName == task_name).FirstOrDefault();
                    if (task != null)
                    {
                        task.FinishTime = DateTime.Now;
                        task.FailureReason = reason;
                        task.State = TASK_RUN_STATUS.CANCEL;
                        db.SaveChanges();

                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                AlarmManagerCenter.AddAlarm(ALARMS.Task_Cancel_Fail);
                return false;
            }

        }


    }
}
