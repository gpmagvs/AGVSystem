using AGVSystem.Controllers;
using AGVSystem.Models.BayMeasure;
using AGVSystem.Models.Sys;
using AGVSystemCommonNet6.AGVDispatch;
using AGVSystemCommonNet6.AGVDispatch.Messages;
using AGVSystemCommonNet6.AGVDispatch.RunMode;
using AGVSystemCommonNet6.Alarm;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.DATABASE.Helpers;
using AGVSystemCommonNet6.Log;
using AGVSystemCommonNet6.Microservices.VMS;
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

        public static async Task<(bool confirm, ALARMS alarm_code, string message)> AddTask(clsTaskDto taskData, TASK_RECIEVE_SOURCE source = TASK_RECIEVE_SOURCE.LOCAL)
        {
            var _order_action = taskData.Action;
            var source_station_tag = int.Parse(taskData.From_Station);
            var destine_station_tag = int.Parse(taskData.To_Station);
            if (!taskData.bypass_eq_status_check && (_order_action == ACTION_TYPE.Load || _order_action == ACTION_TYPE.LoadAndPark
                                                   || _order_action == ACTION_TYPE.Unload || _order_action == ACTION_TYPE.Carry))
            {
                (bool confirm, ALARMS alarm_code, string message) results = (false, ALARMS.NONE, "");
                if (_order_action == ACTION_TYPE.Unload)
                {
                    results = EQTransferTaskManager.CheckUnloadStationStatus(destine_station_tag);
                    if (!results.confirm)
                        return results;
                }
                else if (_order_action == ACTION_TYPE.Load)
                {
                    results = EQTransferTaskManager.CheckLoadStationStatus(destine_station_tag);
                    if (!results.confirm)
                        return results;
                }
                else if (_order_action == ACTION_TYPE.Carry)
                {
                    results = EQTransferTaskManager.CheckUnloadStationStatus(source_station_tag);
                    if (!results.confirm)
                        return results;
                    results = EQTransferTaskManager.CheckLoadStationStatus(destine_station_tag);
                    if (!results.confirm)
                        return results;
                }
            }

            #region 若起點設定是AGV,則起點要設為

            if (taskData.From_Station.Contains("AGV") && _order_action == ACTION_TYPE.Carry)
            {
                var agv_name = taskData.From_Station;
                taskData.DesignatedAGVName = agv_name;
                var agv = VMSSerivces.AgvStatesData.FirstOrDefault(d => d.AGV_Name == agv_name);
                taskData.From_Station = agv.CurrentLocation;
            }

            #endregion

            #region 充電任務確認

            if (taskData.Action == ACTION_TYPE.Charge && taskData.DesignatedAGVName != "")
            {
                try
                {
                    if (VMSSerivces.AgvStatesData.Where(agv => agv.AGV_Name != taskData.DesignatedAGVName).Any(agv => agv.CurrentLocation == taskData.To_Station))
                    {
                        AlarmManagerCenter.AddAlarmAsync(ALARMS.Destine_Charge_Station_Has_AGV, ALARM_SOURCE.AGVS, level: ALARM_LEVEL.WARNING);
                        return (false, ALARMS.Destine_Eq_Station_Has_Task_To_Park, $"目的充電站已有AGV停駐");
                    }
                }
                catch (Exception ex)
                {
                    LOG.Critical(ex);
                }
            }

            #endregion
            try
            {
                #region AGV車款與設備允許車款確認
                (bool confirm, ALARMS alarm_code, string message) agv_type_check_result = EQTransferTaskManager.CheckEQAcceptAGVType(taskData);
                if (!agv_type_check_result.confirm)
                    return agv_type_check_result;
                #endregion


                taskData.RecieveTime = DateTime.Now;
                await Task.Delay(200);
                using (var db = new AGVSDatabase())
                {

                    if (db.tables.Tasks.AsNoTracking().Where(task => task.To_Station != "-1" && task.State == TASK_RUN_STATUS.WAIT || task.State == TASK_RUN_STATUS.NAVIGATING).Any(task => task.To_Station == taskData.To_Station))
                    {
                        if (_order_action == ACTION_TYPE.None)
                        {
                            AlarmManagerCenter.AddAlarmAsync(ALARMS.Destine_Normal_Station_Has_Task_To_Reach, ALARM_SOURCE.AGVS, level: ALARM_LEVEL.WARNING);
                            return (false, ALARMS.Destine_Normal_Station_Has_Task_To_Reach, $"站點-{taskData.To_Station} 已存在移動任務");
                        }
                        else if (_order_action == ACTION_TYPE.Park || _order_action == ACTION_TYPE.LoadAndPark)
                        {
                            AlarmManagerCenter.AddAlarmAsync(ALARMS.Destine_Eq_Station_Has_Task_To_Park, ALARM_SOURCE.AGVS);
                            return (false, ALARMS.Destine_Eq_Station_Has_Task_To_Park, $"目的地設備已有停車任務");
                        }
                        else
                        {
                            AlarmManagerCenter.AddAlarmAsync(ALARMS.Destine_Eq_Already_Has_Task_To_Excute, ALARM_SOURCE.AGVS);
                            return (false, ALARMS.Destine_Eq_Already_Has_Task_To_Excute, $"目的地設備已有搬運任務");
                        }
                    }
                    db.tables.Tasks.Add(taskData);
                    var added = await db.SaveChanges();
                }
                return new(true, ALARMS.NONE, "");
            }
            catch (Exception ex)
            {
                LOG.ERROR(ex);
                AlarmManagerCenter.AddAlarmAsync(ALARMS.Task_Add_To_Database_Fail, ALARM_SOURCE.AGVS);
                return new(false, ALARMS.Task_Add_To_Database_Fail, ex.Message);
            }
        }


        internal async static Task<bool> Cancel(string task_name, string reason = "", TASK_RUN_STATUS status = TASK_RUN_STATUS.CANCEL)
        {
            try
            {
                using (var db = new AGVSDatabase())
                {
                    var task = db.tables.Tasks.Where(tk => tk.TaskName == task_name).FirstOrDefault();
                    if (task != null)
                    {

                        if (task.Action == ACTION_TYPE.Carry)
                        {
                            if (EQTransferTaskManager.MonitoringCarrerTasks.Remove(task_name, out clsLocalAutoTransferTaskMonitor monitor))
                            {
                                monitor.sourceEQ.CancelReserve();
                                monitor.destineEQ.CancelReserve();
                            }

                        }

                        task.FinishTime = DateTime.Now;
                        task.FailureReason = reason;
                        task.State = status;
                        await db.SaveChanges();


                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                AlarmManagerCenter.AddAlarmAsync(ALARMS.Task_Cancel_Fail);
                return false;
            }

        }


        internal static bool TaskStatusChangeToWait(string task_name, string reason = "")
        {
            LOG.TRACE($"Change Task-{task_name} Status = Wait.[Reason:{reason}]");
            try
            {
                using (var db = new AGVSDatabase())
                {
                    var task = db.tables.Tasks.Where(tk => tk.TaskName == task_name).FirstOrDefault();
                    if (task != null)
                    {
                        task.FailureReason = "";
                        task.State = TASK_RUN_STATUS.WAIT;
                        db.SaveChanges();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                LOG.Critical(ex);
                return false;
            }

        }

    }
}
