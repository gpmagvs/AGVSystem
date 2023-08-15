using AGVSystemCommonNet6.Alarm;
using AGVSystemCommonNet6.TASK;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.DATABASE.Helpers;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Composition;
using System.Threading.Tasks;
using AGVSystemCommonNet6.AGVDispatch.Model;
using Microsoft.Build.Framework;

namespace AGVSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskQueryController : ControllerBase
    {
        [HttpGet("TaskQuery")]
        public async Task<IActionResult> TaskQuery(int currentpage, string StartTime, string EndTime, string? AGV_Name = "ALL")
        {
            DateTime start = DateTime.Parse(StartTime);
            DateTime end = DateTime.Parse(EndTime);
            TaskDatabaseHelper.TaskQuery(out int count, currentpage, start, end, AGV_Name, out List<clsTaskDto>? tasks);
            return Ok(new { count, tasks });
        }
        public class Taskquery_options
        {
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }

            public string AGV_Name { get; set; } = "ALL";
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
            TrajectoryDBStoreHelper helper = new TrajectoryDBStoreHelper();
            List<clsTrajCoordination> coordinations = helper.GetTrajectory(taskID);
            return Ok(new { task_id = taskID, coordinations = coordinations });

        }

    }
}
