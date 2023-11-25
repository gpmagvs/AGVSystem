using AGVSystem.Models.WebsocketMiddleware;
using AGVSystemCommonNet6.Alarm;
using AGVSystemCommonNet6.DATABASE;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace AGVSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlarmController : ControllerBase
    {
        private readonly AGVSDbContext _dbContext;

        public AlarmController(AGVSDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("/UncheckedAlarm")]
        public async Task Alarms()
        {
            await WebsocketMiddleware.ClientRequest(HttpContext);
        }
        [HttpGet("SystemAlarmReset")]
        public async Task<IActionResult> SystemAlarmReset()
        {
            var sys_alarms = _dbContext.SystemAlarms.Where(alarm => alarm.Source == ALARM_SOURCE.AGVS && !alarm.Checked);
            if (!sys_alarms.Any())
                return Ok();

            foreach (var alarm in sys_alarms)
            {
                alarm.Checked = true;
                _dbContext.SystemAlarms.Update(alarm);
            }
            await _dbContext.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("EquipmentAlarmReset")]
        public async Task<IActionResult> EquipmentAlarmReset()
        {
            var sys_alarms = _dbContext.SystemAlarms.ToList().FindAll(alarm => alarm.Source == ALARM_SOURCE.EQP && !alarm.Checked);
            foreach (var alarm in sys_alarms)
            {
                alarm.Checked = true;
                _dbContext.SystemAlarms.Update(alarm);
            }
            _dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
