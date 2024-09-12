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

namespace AGVSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskQueryController : ControllerBase
    {
        AGVSDbContext dbcontext;
        public TaskQueryController(AGVSDbContext dbcontext)
        {
            this.dbcontext = dbcontext;
        }
        [HttpGet("TaskQuery")]
        public async Task<IActionResult> TaskQuery(int currentpage, string StartTime, string EndTime, string? AGV_Name = "ALL", string? TaskName = "ALL", string Result = "ALL", string ActionType = "ALL", string? failurereason = "ALL")
        {
            DateTime start = DateTime.Parse(StartTime);
            DateTime end = DateTime.Parse(EndTime);
            using (var taskDb = new TaskDatabaseHelper())
            {
                taskDb.TaskQuery(out int count, currentpage, start, end, AGV_Name, TaskName, Result, ActionType, failurereason, out List<clsTaskDto>? tasks);

                tasks.ForEach(task =>
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

                return Ok(new { count, tasks });
            }


            string _GetDisplayOfTag(int tag)
            {
                var toPoint = AGVSMapManager.GetMapPointByTag(tag);
                return toPoint == null ? tag + "" : toPoint.Graph.Display;
            }
        }
        [HttpGet("SaveTocsv")]
        public async Task<IActionResult> SaveTocsv(string StartTime, string EndTime, string? TaskName = "ALL", string? AGV_Name = "ALL")
        {
            DateTime start = DateTime.Parse(StartTime);
            DateTime end = DateTime.Parse(EndTime);
            //TaskDatabaseHelper.SaveTocsv(start, end, AGV_Name, TaskName);
            //return Ok();
            string FileName = TaskDatabaseHelper.SaveTocsv(start, end, AGV_Name, TaskName);
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
        public async Task<IActionResult> GetTasks(DateTime start, DateTime end, string agv_name = "")
        {
            TaskDatabaseHelper dbhelper = new TaskDatabaseHelper();
            List<clsTaskDto> tasks = dbhelper.GetTasksByTimeInterval(start, end);
            if (agv_name != "")
                return Ok(tasks.FindAll(task => task.DesignatedAGVName == agv_name));
            else
                return Ok(tasks);
        }
        [HttpGet("GetTrajectory")]
        public async Task<IActionResult> GetTrajectory(string taskID)
        {
            try
            {
                TrajectoryDBStoreHelper helper = new TrajectoryDBStoreHelper();
                List<clsTrajCoordination> coordinations = helper.GetTrajectory(taskID);
                return Ok(new { task_id = taskID, coordinations = coordinations });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }


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
