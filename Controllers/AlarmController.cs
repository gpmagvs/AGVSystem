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
            List<clsAlarmDto> uncheckedAlarms = AlarmManagerCenter.uncheckedAlarms;
            bool isWebsocketReq = HttpContext.WebSockets.IsWebSocketRequest;
            if (isWebsocketReq)
            {
                var websocket_client = await HttpContext.WebSockets.AcceptWebSocketAsync();
                try
                {
                    while (websocket_client.State == System.Net.WebSockets.WebSocketState.Open)
                    {
                        Thread.Sleep(200);
                        uncheckedAlarms = AlarmManagerCenter.uncheckedAlarms;
                        byte[] rev_buffer = new byte[4096];

                        websocket_client.ReceiveAsync(new ArraySegment<byte>(rev_buffer), CancellationToken.None);
                        using (AGVStatusDBHelper dBHelper = new AGVStatusDBHelper())
                        {
                            await websocket_client.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(uncheckedAlarms))), System.Net.WebSockets.WebSocketMessageType.Text, true, CancellationToken.None);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Websocket Client Closed (/UncheckedAlarm):" + ex.Message);

                }

            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }
        [HttpGet("SystemAlarmReset")]
        public async Task<IActionResult> SystemAlarmReset()
        {
            var sys_alarms = _dbContext.SystemAlarms.ToList().FindAll(alarm => alarm.Source == ALARM_SOURCE.AGVS && !alarm.Checked);
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
