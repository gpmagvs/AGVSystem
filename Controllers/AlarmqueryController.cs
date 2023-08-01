using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SQLite;
using AGVSystemCommonNet6.Alarm;
using AGVSystemCommonNet6.DATABASE;
using System.Collections.Generic;

namespace AGVSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlarmqueryController : ControllerBase
    {
        [HttpGet("Query")]
        public async Task<IActionResult> Query(int currentpage, string StartTime, string EndTime, string? AGV_Name = "ALL")
        {
            DateTime start = DateTime.Parse(StartTime);
            DateTime end = DateTime.Parse(EndTime);
            List<clsAlarmDto>? alarms = AlarmManagerCenter.Query(start, end, AGV_Name);
            return Ok(new { count = alarms.Count, alarms });
        }
        public class Alarmquery_options
        {
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }

            public string AGV_Name { get; set; } = "ALL";
        }

    }
}
