using AGVSystem.Models.Sys;
using AGVSystem.Service;
using AGVSystem.Service.Aggregates;
using AGVSystemCommonNet6;
using AGVSystemCommonNet6.AGVDispatch.RunMode;
using AGVSystemCommonNet6.Configuration;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.Microservices.MCS;
using AGVSystemCommonNet6.Microservices.VMS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NLog;
using System.Reflection;
using System.Text;
using static SQLite.SQLite3;

namespace AGVSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SystemController : ControllerBase
    {
        SystemStatusDbStoreService _SystemStatusDbStoreService;
        DatabaseMigrateService dbMigrateService;
        Logger logger = LogManager.GetCurrentClassLogger();
        SystemModesAggregateService systemModesAggregateService;
        public SystemController(SystemStatusDbStoreService _SystemStatusDbStoreService, DatabaseMigrateService dbMigrateService, SystemModesAggregateService systemModesAggregateService)
        {
            this.systemModesAggregateService = systemModesAggregateService;
            this._SystemStatusDbStoreService = _SystemStatusDbStoreService;
            this.dbMigrateService = dbMigrateService;
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
            (bool success, string message) = await systemModesAggregateService.MaintainRunSwitch(mode, forecing_change);
            return Ok(new { confirm = success, message = message });
        }

        [HttpPost("HostConn")]
        public async Task<IActionResult> HostConnMode(HOST_CONN_MODE mode)
        {
            (bool confirm, string message) response = await systemModesAggregateService.HostOnlineOfflineModeSwitch(mode);
            return Ok(new { confirm = response.confirm, message = response.message });
        }


        [HttpPost("HostOperation")]
        public async Task<IActionResult> HostOperationMode(HOST_OPER_MODE mode)
        {
            (bool confirm, string message) response = await systemModesAggregateService.HostOnlineRemoteLocalModeSwitch(mode);
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
                logger.Error(ex.Message);
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

        [HttpPost("CloneCurrentDatabaseToNewOne")]
        public async Task<IActionResult> CloneCurrentDatabaseToNewOne([FromBody] DatabaseMigrateService.NewDatabaseConfiguration configuration)
        {
            try
            {
                var result = await dbMigrateService.CreateNewDatabase(configuration);
                return Ok(new { result = result.Item1, message = result.Item2 });
            }
            catch (Exception ex)
            {
                return Ok(new { result = false, message = ex.Message });
            }
        }

        private string GetAppVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}
