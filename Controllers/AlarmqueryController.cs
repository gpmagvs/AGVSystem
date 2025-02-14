using AGVSystemCommonNet6.Alarm;
using AGVSystemCommonNet6.DATABASE.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace AGVSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlarmqueryController : ControllerBase
    {
        [HttpGet("QueryAlarm")]
        public async Task<IActionResult> AlarmQuery(int currentpage, string StartTime, string EndTime, string? alarmCodeParam, string? TaskName = "ALL", string? AGV_Name = "ALL", string AlarmType = "ALL", string? Alarm_description = "ALL")
       {
            DateTime start = DateTime.Parse(StartTime);
            DateTime end = DateTime.Parse(EndTime);
            AlarmManagerCenter.AlarmQuery(out int count, currentpage, start, end, alarmCodeParam,  AGV_Name, TaskName, Alarm_description, out List<clsAlarmDto>? alarms, AlarmType);
            return Ok(new { count, alarms });
        }
        [HttpGet("QueryAlarmWithKeyword")]
        public async Task<IActionResult> QueryAlarmWithKeyword(int currentpage, string StartTime, string EndTime, string? keyword = "")
        {
            DateTime start = DateTime.Parse(StartTime);
            DateTime end = DateTime.Parse(EndTime);
            AlarmManagerCenter.QueryAlarmWithKeyword(currentpage, start, end, keyword, out int count, out List<clsAlarmDto>? alarms);
            return Ok(new { count, alarms });
        }
        [HttpGet("SaveTocsv")]
        public async Task<IActionResult> SaveTocsv(string StartTime, string EndTime, string? TaskName = "ALL", string? AGV_Name = "ALL")
        {
            DateTime start = DateTime.Parse(StartTime);
            DateTime end = DateTime.Parse(EndTime);
            string FileName = AlarmManagerCenter.SaveTocsv(start, end, AGV_Name, TaskName);
            FileStream fileStream = new FileStream(FileName, FileMode.Open, FileAccess.Read);

            // �]�m�^�������e����
            var contentType = "application/octet-stream"; // �ήھ��ɮ������]�m�A�����e����
            var fileContentResult = new FileStreamResult(fileStream, contentType);

            // �]�m�U���ɮת��W��
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
