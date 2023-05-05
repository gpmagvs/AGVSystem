using AGVSystem.VMS;
using AGVSystemCommonNet6.DATABASE;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;

namespace AGVSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VMSManagerController : ControllerBase
    {
        private AGVSDbContext _dbContent;

        public VMSManagerController(AGVSDbContext dbContent)
        {
            _dbContent = dbContent;
        }

        [HttpGet("AGVOnline")]
        public async Task<IActionResult> AGVOnline(string agv_name)
        {
            (bool success, string message) result = await VMSManager.AGVOnline(agv_name);
            return Ok(new { Success = result.success, Message = result.message });
        }
        [HttpGet("AGVOffline")]
        public async Task<IActionResult> AGVOffline(string agv_name)
        {
            (bool success, string message) result = await VMSManager.AGVOffline(agv_name);
            return Ok(new { Success = result.success, Message = result.message });
        }


        [HttpGet("/ws/VMSStatus")]
        public async Task GetVMSStatus()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                var websocket_client = await HttpContext.WebSockets.AcceptWebSocketAsync();
                try
                {
                    while (websocket_client.State == System.Net.WebSockets.WebSocketState.Open)
                    {
                        Thread.Sleep(200);

                        byte[] rev_buffer = new byte[4096];

                        websocket_client.ReceiveAsync(new ArraySegment<byte>(rev_buffer), CancellationToken.None);
                        using (AGVStatusDBHelper dBHelper = new AGVStatusDBHelper())
                        {
                            var data_db = dBHelper.GetALL().OrderBy(a => a.AGV_Name).ToList();
                            await websocket_client.SendAsync(new ArraySegment<byte>(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(data_db))), System.Net.WebSockets.WebSocketMessageType.Text, true, CancellationToken.None);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Websocket Client Closed (/ws/VMSStatus):" + ex.Message);
                }

            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

    }
}
