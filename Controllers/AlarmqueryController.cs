using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SQLite;
using AGVSystemCommonNet6.Alarm;
using AGVSystemCommonNet6.DATABASE;
using System.Collections.Generic;
using AGVSystemCommonNet6.Alarm.VMS_ALARM;
using System.Net;

namespace AGVSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlarmqueryController : ControllerBase
    {
        [HttpGet("QueryAlarm")]
        public async Task<IActionResult> AlarmQuery(int currentpage, string StartTime, string EndTime, string? TaskName = "ALL", string? AGV_Name = "ALL", string AlarmType = "ALL")
        {
            DateTime start = DateTime.Parse(StartTime);
            DateTime end = DateTime.Parse(EndTime);
            AlarmManagerCenter.AlarmQuery(out int count, currentpage, start, end, AGV_Name, TaskName, out List<clsAlarmDto>? alarms, AlarmType);
            return Ok(new { count, alarms });
        }
        [HttpGet("SaveTocsv")]
        public async Task<IActionResult> SaveTocsv(string StartTime, string EndTime, string? TaskName = "ALL", string? AGV_Name = "ALL")
        {
            DateTime start = DateTime.Parse(StartTime);
            DateTime end = DateTime.Parse(EndTime);
            string FileName = AlarmManagerCenter.SaveTocsv(start, end, AGV_Name, TaskName);
            FileStream fileStream = new FileStream(FileName, FileMode.Open, FileAccess.Read);

            // 設置回應的內容類型
            var contentType = "application/octet-stream"; // 或根據檔案類型設置適當的內容類型
            var fileContentResult = new FileStreamResult(fileStream, contentType);

            // 設置下載檔案的名稱
            fileContentResult.FileDownloadName = "filename.ext";

            return fileContentResult;
        }
        public class Alarmquery_options
        {
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }

            public string AGV_Name { get; set; } = "ALL";
            public string TaskName { get; set; } = "ALL";
        }

    }
}
