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
        [HttpGet("TaskQuery")]
        public async Task<IActionResult> TaskQuery(int currentpage, string StartTime, string EndTime, string? AGV_Name = "ALL", string? TaskName = "ALL", string Result = "ALL", string ActionType = "ALL",string? failurereason="ALL")
        {
            DateTime start = DateTime.Parse(StartTime);
            DateTime end = DateTime.Parse(EndTime);
            using (var taskDb = new TaskDatabaseHelper())
            {
                taskDb.TaskQuery(out int count, currentpage, start, end, AGV_Name, TaskName, Result, ActionType, failurereason,out List<clsTaskDto>? tasks);

                tasks.ForEach(task =>
                {
                    if (task.From_Station_Tag != -1)
                    {
                        var fromPoint = AGVSMapManager.GetMapPointByTag(task.From_Station_Tag);
                        task.From_Station_Display = fromPoint == null ? task.From_Station_Tag + "" : fromPoint.Graph.Display;
                    }
                    if (task.To_Station_Tag != -1)
                    {
                        var toPoint = AGVSMapManager.GetMapPointByTag(task.To_Station_Tag);
                        task.To_Station_Display = toPoint == null ? task.To_Station_Tag + "" : toPoint.Graph.Display;
                    }
                });

                return Ok(new { count, tasks });
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
        public class Taskquery_options
        {
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }

            public string AGV_Name { get; set; } = "ALL";
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

    }
}
