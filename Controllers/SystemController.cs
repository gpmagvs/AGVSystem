using AGVSystem.Models.Sys;
using AGVSystem.Service;
using AGVSystemCommonNet6;
using AGVSystemCommonNet6.AGVDispatch.RunMode;
using AGVSystemCommonNet6.Configuration;
using AGVSystemCommonNet6.Log;
using AGVSystemCommonNet6.Microservices.MCS;
using AGVSystemCommonNet6.Microservices.VMS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Reflection;
using System.Text;

namespace AGVSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SystemController : ControllerBase
    {
        SystemStatusDbStoreService _SystemStatusDbStoreService;
        public SystemController(SystemStatusDbStoreService _SystemStatusDbStoreService)
        {
            this._SystemStatusDbStoreService = _SystemStatusDbStoreService;
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
                SystemModes.RunMode = _previousMode;
                return Ok(new { confirm = false, message = message });
            }
            LOG.INFO($"[Run Mode Switch] 等待VMS回覆 {mode}模式請求");
            (bool confirm, string message) vms_response = await VMSSerivces.RunModeSwitch(mode, forecing_change);
            OkObjectResult oko = Ok(new { confirm = false, message = "" });
            if (vms_response.confirm == false)
            {
                SystemModes.RunMode = _previousMode;
                oko = Ok(new { confirm = vms_response.confirm, message = vms_response.message });
            }
            else
            {
                oko = Ok(new { confirm = vms_response.confirm, message = vms_response.message });
                _SystemStatusDbStoreService.ModifyRunModeStored(mode);
            }
            return oko;
        }

        [HttpPost("HostConn")]
        public async Task<IActionResult> HostConnMode(HOST_CONN_MODE mode)
        {
            (bool confirm, string message) response = new(false, "[HostConnMode] Fail");

            if (mode == HOST_CONN_MODE.ONLINE)
                response = await MCSCIMService.Online();
            else
                response = await MCSCIMService.Offline();
            if (response.confirm == true)
            {
                SystemModes.HostConnMode = mode;
                if (SystemModes.HostConnMode == HOST_CONN_MODE.OFFLINE)
                    SystemModes.HostOperMode = HOST_OPER_MODE.LOCAL;
                _SystemStatusDbStoreService.ModifyHostConnMode(mode);
            }
            return Ok(new { confirm = response.confirm, message = response.message });
        }


        [HttpPost("HostOperation")]
        public async Task<IActionResult> HostOperationMode(HOST_OPER_MODE mode)
        {
            if (SystemModes.HostConnMode != HOST_CONN_MODE.ONLINE)
                return Ok(new { confirm = false, message = $"HostConnMode is not ONLINE" });
            (bool confirm, string message) response = new(false, "[HostOperationMode] Fail");
            if (mode == HOST_OPER_MODE.LOCAL)
                response = await MCSCIMService.OnlineRemote2OnlineLocal();
            else
                response = await MCSCIMService.OnlineLocalToOnlineRemote();
            if (response.confirm == true)
            {

                SystemModes.HostOperMode = mode;
                _SystemStatusDbStoreService.ModifyHostOperMode(mode);
            }
            return Ok(new { confirm = response.confirm, message = response.message });
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
            try
            {
                AGVSConfigulator.LoadConfig();
            }
            catch (Exception ex)
            {
                LOG.WARN(ex.Message);
            }
            var website_config = new
            {
                AGVSConfigulator.SysConfigs.WebUserLogoutExipreTime,
                FieldName = AGVSConfigulator.SysConfigs.BaseOnKGSWebAGVSystem ? $"{AGVSConfigulator.SysConfigs.FieldName}(Middleware)" : AGVSConfigulator.SysConfigs.FieldName,
                APPVersion = GetAppVersion()
            };
            return Ok(website_config);
        }

        [HttpGet("SystemConfigs")]
        public async Task<IActionResult> SystemConfigs()
        {
            return Ok(AGVSConfigulator.SysConfigs);
        }


        [HttpGet("AliveCheck")]
        public async Task<IActionResult> AliveCheck()
        {
            return Ok();
        }



        private string GetAppVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}
