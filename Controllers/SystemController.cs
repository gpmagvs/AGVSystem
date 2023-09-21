using AGVSystem.Models.Sys;
using AGVSystemCommonNet6;
using AGVSystemCommonNet6.AGVDispatch.RunMode;
using AGVSystemCommonNet6.Configuration;
using AGVSystemCommonNet6.Log;
using AGVSystemCommonNet6.Microservices.VMS;
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
                system_run_mode = SystemModes.RunMode,
                host_online_mode = SystemModes.HostConnMode,
                host_remote_mode = SystemModes.HostOperMode
            });
        }

        [HttpGet("RunMode")]
        public async Task<IActionResult> IsRunMode()
        {
            return Ok(SystemModes.RunMode);
        }

        [HttpPost("RunMode")]
        public async Task<IActionResult> RunMode(RUN_MODE mode)
        {
            var _previousMode = SystemModes.RunMode;
            SystemModes.RunMode = mode == RUN_MODE.MAINTAIN ? RUN_MODE.SWITCH_TO_MAITAIN_ING : RUN_MODE.SWITCH_TO_RUN_ING;
            //Request VMS Switch Mode first
            LOG.INFO($"[Run Mode Switch] 等待VMS回覆 {mode}模式請求");
            (bool confirm, string message) vms_response = await VMSSerivces.RunModeSwitch(mode);
            LOG.INFO($"[Run Mode Switch] VMS Response={vms_response.ToJson()}");
            if (!vms_response.confirm)
            {
                SystemModes.RunMode = _previousMode;
                return Ok(new { confirm = false, message = vms_response.message });
            }
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
            AGVSConfigulator.LoadConfig();
            var website_config = new
            {
                AGVSConfigulator.SysConfigs.WebUserLogoutExipreTime
            };
            return Ok(website_config);
        }

        [HttpGet("/ws/VMSAliveCheck")]
        public async Task AliveCheck()
        {
            await WebsocketHandler.ClientRequest(HttpContext);
        }


    }
}
