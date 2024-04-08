using AGVSystem.Models.Sys;
using AGVSystem.Models.WebsocketMiddleware;
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
        [HttpGet("/ws")]
        public async Task WebsocketClient(string user_id)
        {
            await AGVSWebsocketServerMiddleware.Middleware.HandleWebsocketClientConnectIn(HttpContext, user_id);

        }

        [HttpGet("OperationStates")]
        public async Task<IActionResult> OperationStates()
        {
            return Ok(new
            {
                system_run_mode = SystemModes.RunMode,
                host_online_mode = SystemModes.HostConnMode,
                host_remote_mode = SystemModes.HostOperMode,
                transfer_mode = SystemModes.TransferTaskMode
            });
        }

        [HttpGet("RunMode")]
        public async Task<IActionResult> IsRunMode()
        {
            return Ok(SystemModes.RunMode);
        }

        [HttpPost("RunMode")]
        public async Task<IActionResult> RunMode(RUN_MODE mode, bool forecing_change = false)
        {
            var _previousMode = SystemModes.RunMode;
            SystemModes.RunMode = mode == RUN_MODE.MAINTAIN ? RUN_MODE.SWITCH_TO_MAITAIN_ING : RUN_MODE.SWITCH_TO_RUN_ING;
            //AGVS先確認
            bool agvs_confirm = SystemModes.RunModeSwitch(mode, out string message, forecing_change);
            if (!agvs_confirm)
            {
                return Ok(new { confirm = false, message = message });
            }
            LOG.INFO($"[Run Mode Switch] 等待VMS回覆 {mode}模式請求");
            (bool confirm, string message) vms_response = await VMSSerivces.RunModeSwitch(mode, forecing_change);
            if (!vms_response.confirm)
            {
                SystemModes.RunMode = _previousMode;

                return Ok(new { confirm = false, message = vms_response.message });
            }
            else
            {

                return Ok(new { confirm = true, message = "" });
            }
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


        [HttpPost("TransferMode")]
        public async Task<IActionResult> TransferMode(TRANSFER_MODE mode)
        {
            SystemModes.TransferTaskMode = mode;
            return Ok(new { confirm = true, message = "" });
        }
        [HttpGet("Website")]
        public async Task<IActionResult> Get()
        {
            AGVSConfigulator.LoadConfig();
            var website_config = new
            {
                AGVSConfigulator.SysConfigs.WebUserLogoutExipreTime,
                AGVSConfigulator.SysConfigs.FieldName,
            };
            return Ok(website_config);
        }

    }
}
