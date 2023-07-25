using AGVSystemCommonNet6.Alarm;
using AGVSystemCommonNet6.TASK;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AGVSystemCommonNet6.DATABASE;

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
            TaskDatabaseHelper.TaskQuery(out int count, currentpage, start, end, AGV_Name, out List<clsTaskDto>? Task);
            return Ok(new { count, Task });
        }
        public class Taskquery_options
        {
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }

            public string AGV_Name { get; set; } = "ALL";
        }
    }
}
