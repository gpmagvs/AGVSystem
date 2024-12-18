using AGVSystemCommonNet6.Alarm;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.DATABASE.Helpers;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Composition;
using System.Threading.Tasks;
using AGVSystemCommonNet6.AGVDispatch.Model;
using Microsoft.Build.Framework;
using AGVSystemCommonNet6.AGVDispatch;
using AGVSystem.Models.Map;
using AGVSystemCommonNet6.Vehicle_Control.VCSDatabase;
using Microsoft.Build.ObjectModelRemoting;
using AGVSystemCommonNet6.ViewModels;
using AGVSystemCommonNet6.AGVDispatch.Messages;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using static AGVSystemCommonNet6.DATABASE.Helpers.TrajectoryDBStoreHelper;

namespace AGVSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskQueryController : ControllerBase
    {
        AGVSDbContext dbcontext;
        readonly IMemoryCache cache;
        public TaskQueryController(AGVSDbContext dbcontext, IMemoryCache cache)
        {
            this.dbcontext = dbcontext;
            this.cache = cache;
        }

        [HttpPost]
        public async Task<IActionResult> TaskQuery([FromBody] TaskQueryCondition conditions)
        {
            using var taskDBHelper = new TaskDatabaseHelper();
            (int total, List<clsTaskDto> tasksQueryOut, int CompleteNum, int FailNum, int CancelNum) = taskDBHelper.TaskQuery(conditions);
            tasksQueryOut.ForEach(task =>
            {
                if (task.From_Station_Tag != -1)
                {
                    task.From_Station_Display = _GetDisplayOfTag(task.From_Station_Tag);
                }
                if (task.To_Station_Tag != -1)
                {
                    task.To_Station_Display = _GetDisplayOfTag(task.To_Station_Tag);
                }
                task.StartLocationDisplay = _GetDisplayOfTag(task.StartLocationTag);
            });

            string _GetDisplayOfTag(int tag)
            {
                var toPoint = AGVSMapManager.GetMapPointByTag(tag);
                return toPoint == null ? tag + "" : toPoint.Graph.Display;
            }

            return Ok(new { count = total, tasks = tasksQueryOut, CompleteNum, FailNum, CancelNum });
        }

        [HttpGet("SaveTocsv")]
        public async Task<IActionResult> SaveTocsv(string StartTime, string EndTime, string? TaskName = "", TASK_RUN_STATUS Result = TASK_RUN_STATUS.UNKNOWN, string? AGV_Name = "")
        {
            DateTime start = DateTime.Parse(StartTime);
            DateTime end = DateTime.Parse(EndTime);
            //TaskDatabaseHelper.SaveTocsv(start, end, AGV_Name, TaskName);
            //return Ok();
            string FileName = TaskDatabaseHelper.SaveTocsv(start, end, AGV_Name, TaskName, Result);
            FileStream fileStream = new FileStream(FileName, FileMode.Open, FileAccess.Read);
            // 設置回應的內容類型
            var contentType = "application/octet-stream"; // 或根據檔案類型設置適當的內容類型
            var fileContentResult = new FileStreamResult(fileStream, contentType);
            // 設置下載檔案的名稱
            fileContentResult.FileDownloadName = "filename.ext";
            return fileContentResult;
        }

        [HttpGet("GetTaskStateByID")]
        public async Task<IActionResult> GetTaskStateByID(string TaskName)
        {
            TaskDatabaseHelper dbhelper = new TaskDatabaseHelper();
            return Ok(await dbhelper.GetTaskStateByID(TaskName));
        }


        [HttpGet("GetTasks")]
        public async Task<IActionResult> GetTasks(DateTime start, DateTime end, string? agv_name = "", string? taskID = "")
        {
            string dataKey = $"GetTask_{agv_name}_{taskID}_{start.ToString()}_{end.ToString()}";
            if (cache.TryGetValue(dataKey, out object dataCached))
            {
                return Ok(dataCached);
            }
            else
            {
                bool _specifiedTaskID = !string.IsNullOrEmpty(taskID);
                TaskDatabaseHelper dbhelper = new TaskDatabaseHelper();
                List<clsTaskDto> tasks = dbhelper.GetTasksByTimeInterval(start, end, taskID);
                List<clsTaskDto> returnData;

                if (!string.IsNullOrEmpty(agv_name) && !_specifiedTaskID)
                    returnData = tasks.FindAll(task => task.DesignatedAGVName == agv_name);
                else
                    returnData = tasks;
                cache.Set<List<clsTaskDto>>(dataKey, returnData, TimeSpan.FromMinutes(10));
                return Ok(tasks);
            }
        }
        [HttpGet("GetTrajectory")]
        public async Task<IActionResult> GetTrajectory(string taskID)
        {
            try
            {
                string cacheDataKey = $"Trajectory_{taskID}";
                if (cache.TryGetValue(cacheDataKey, out object cacheData))
                {
                    return Ok(cacheData);
                }
                else
                {

                    TrajectoryDBStoreHelper helper = new TrajectoryDBStoreHelper();
                    List<clsTrajCoordination> coordinations = helper.GetTrajectory(taskID);
                    var dataWrap = new { task_id = taskID, coordinations = coordinations };
                    cache.Set<object>(cacheDataKey, dataWrap, TimeSpan.FromMinutes(30));
                    return Ok(dataWrap);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }


        }
        [HttpGet("GetTrajectorys")]
        public async Task<IActionResult> GetTrajectorys(string taskID)
        {
            try
            {
                TrajectoryDBStoreHelper helper = new TrajectoryDBStoreHelper();
                List<List<clsTrajCoordination>> coordinations = helper.GetTrajectorys(taskID);
                var dataWrap = new { task_id = taskID, coordinations = coordinations };
                return Ok(dataWrap);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }


        }

        [HttpGet("GetTrajectorysWithTimeRange")]
        public async Task<List<clsTaskTrajecotroyViewModel>> GetTrajectorysWithTimeRange(DateTime from, DateTime to)
        {
            TrajectoryDBStoreHelper helper = new TrajectoryDBStoreHelper();
            return await helper.GetTrajectorysWithTimeRange(from, to);
        }

        [HttpGet("DeleteTask")]
        public async Task<IActionResult> DeleteTask(string taskID)
        {
            var taskFound = dbcontext.Tasks.FirstOrDefault(tk => tk.TaskName == taskID);
            if (taskFound != null)
            {
                dbcontext.Tasks.Remove(taskFound);
                await dbcontext.SaveChangesAsync();
            }
            return Ok();
        }
        [HttpPost("CreateTodayTaskHistory")]
        public async Task<IActionResult> CreateTodayTaskHistory()
        {
            TaskDatabaseHelper.AutoExportYesterdayHistoryToDestine();
            return Ok();
        }
        [HttpPost("ExportTaskHistoryDataOfDay")]
        public async Task<IActionResult> ExportTaskHistoryDataOfDay(DateTime date)
        {
            try
            {
                TaskDatabaseHelper.ExportSpeficDateHistoryToDestine(date);
                return Ok(new { confirm = true, message = "" });
            }
            catch (Exception ex)
            {
                return Ok(new { confirm = false, message = ex.Message });
            }
        }
        [HttpPost("ExportTaskHistoryDataOfDays")]
        public async Task<IActionResult> ExportTaskHistoryDataOfDay(DateTime from_date, DateTime to_date)
        {
            try
            {
                int totalDays = (to_date - from_date).Days + 1;
                List<string> filePathes = new List<string>();
                for (int i = 0; i < totalDays; i++)
                {
                    DateTime _date = from_date.AddDays(i);
                    string path = TaskDatabaseHelper.ExportSpeficDateHistoryToDestine(_date);
                    filePathes.Add(path);
                }
                string message = $"<div style=\"width:400px\">{filePathes.Last()} <br/>與其他 {filePathes.Count - 1} 個檔案<div>";
                return Ok(new { confirm = true, message = message });
            }
            catch (Exception ex)
            {
                return Ok(new { confirm = false, message = ex.Message });
            }
        }
    }
}
