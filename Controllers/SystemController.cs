using AGVSystem.Models.Sys;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace AGVSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SystemController : ControllerBase
    {

        [HttpGet("OperationStates")]
        public async Task<IActionResult> OperationStates()
        {
            return Ok(new
            {
                system_run_mode = SystemModes.RunMode == RUN_MODE.RUN,
                host_online_mode = SystemModes.HostConnMode == HOST_CONN_MODE.ONLINE,
                host_remote_mode = SystemModes.HostOperMode == HOST_OPER_MODE.REMOTE
            });
        }

        [HttpGet("RunMode")]
        public async Task<IActionResult> IsRunMode()
        {
            return Ok(SystemModes.RunMode == RUN_MODE.RUN);
        }

        [HttpPost("RunMode")]
        public async Task<IActionResult> RunMode(RUN_MODE mode)
        {
            bool confirm = SystemModes.RunModeSwitch(mode, out string message);
            return Ok(new { confirm = confirm, message });
        }

        [HttpPost("HostConn")]
        public async Task<IActionResult> HostConnMode(HOST_CONN_MODE mode)
        {
            SystemModes.HostConnMode = mode;
            return Ok(new { confirm = true, message = "" });
        }


        [HttpPost("HostOperation")]
        public async Task<IActionResult> HostOperationMode(HOST_OPER_MODE mode)
        {
            SystemModes.HostOperMode = mode;
            return Ok(new { confirm = true, message = "" });
        }

        [HttpGet("Website")]
        public async Task<IActionResult> Get()
        {
            var website_config = new
            {
                AppSettings.Public,
            };
            return Ok(website_config);
        }

        [HttpGet("/ws/VMSAliveCheck")]
        public async Task AliveCheck()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                var websocket_client = await HttpContext.WebSockets.AcceptWebSocketAsync();

                while (websocket_client.State == System.Net.WebSockets.WebSocketState.Open)
                {
                    try
                    {
                        await Task.Delay(500);
                        byte[] rev_buffer = new byte[4096];
                        websocket_client.ReceiveAsync(new ArraySegment<byte>(rev_buffer), CancellationToken.None);
                        await websocket_client.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(true))), System.Net.WebSockets.WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        return;
                    }
                }
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }


    }
}
