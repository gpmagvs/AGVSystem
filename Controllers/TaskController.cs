using AGVSystem.Models.TaskAllocation;
using AGVSystem.Models.TaskAllocation.HotRun;
using AGVSystem.TaskManagers;
using AGVSystemCommonNet6.AGVDispatch;
using AGVSystemCommonNet6.Alarm;
using AGVSystemCommonNet6.Configuration;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.Log;
using AGVSystemCommonNet6.MAP;

using AGVSystemCommonNet6.User;
using EquipmentManagment.MainEquipment;
using EquipmentManagment.Manager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NuGet.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static SQLite.SQLite3;

namespace AGVSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public partial class TaskController : ControllerBase
    {
        private AGVSDbContext _TaskDBContent;
        public TaskController(AGVSDbContext content)
        {
            this._TaskDBContent = content;
        }

        [HttpGet("Allocation")]
        [Authorize]
        public async Task<IActionResult> Test()
        {
            if (!UserValidation())
            {
                return Unauthorized();
            }

            return Ok();
        }

        [HttpGet("/ws/TaskData")]
        public async Task GetNotFinishTaskListData()
        {
            await WebsocketHandler.ClientRequest(HttpContext);
        }

        [HttpGet("/ws/HotRun")]
        public async Task GetHotRunStates()
        {
            await WebsocketHandler.ClientRequest(HttpContext);
        }

        [HttpGet("Cancel")]
        [Authorize]
        public async Task<IActionResult> Cancel(string task_name)
        {
            if (!UserValidation())
            {
                return Unauthorized();
            }

            bool canceled = TaskManager.Cancel(task_name, $"User manual canceled");
            return Ok(canceled);
        }

        [HttpPost("move")]
        [Authorize]
        public async Task<IActionResult> MoveTask([FromBody] clsTaskDto taskData, string user = "")
        {
            if (!UserValidation())
            {
                return Unauthorized();
            }
            return Ok(await AddTask(taskData, user));
        }
        [HttpPost("measure")]
        [Authorize]
        public async Task<IActionResult> MeasureTask([FromBody] clsTaskDto taskData, string user = "")
        {
            if (!UserValidation())
            {
                return Unauthorized();
            }
            Map map = MapManager.LoadMapFromFile();
            if (map.Bays.TryGetValue(taskData.To_Station, out Bay bay))
            {
                taskData.To_Slot = string.Join(",", bay.Points);

                return Ok(await AddTask(taskData, user));
            }
            else
                return Ok(new { confirm = false, message = $"Bay - {taskData.To_Station} not found" });
        }
        [HttpPost("load")]
        [Authorize]
        public async Task<IActionResult> LoadTask([FromBody] clsTaskDto taskData, string user = "")
        {
            if (!UserValidation())
            {
                return Unauthorized();
            }
            return Ok(await AddTask(taskData, user));
        }
        [HttpPost("unload")]
        [Authorize]
        public async Task<IActionResult> UnloadTask([FromBody] clsTaskDto taskData, string user = "")
        {
            if (!UserValidation())
            {
                return Unauthorized();
            }
            return Ok(await AddTask(taskData, user));
        }
        [HttpPost("carry")]
        [Authorize]
        public async Task<IActionResult> CarryTask([FromBody] clsTaskDto taskData, string user = "")
        {
            if (!UserValidation())
            {
                return Unauthorized();
            }
            return Ok(await AddTask(taskData, user));
        }
        [HttpPost("charge")]
        [Authorize]
        public async Task<IActionResult> ChargeTask([FromBody] clsTaskDto taskData, string user = "")
        {
            if (!UserValidation())
            {
                return Unauthorized();
            }
            return Ok(await AddTask(taskData, user));
        }
        [HttpPost("ExangeBattery")]
        [Authorize]
        public async Task<IActionResult> ExangeBattery([FromBody] clsTaskDto taskData, string user = "")
        {
            if (!UserValidation())
            {
                return Unauthorized();
            }
            return Ok(await AddTask(taskData, user));
        }
        [HttpPost("park")]
        [Authorize]
        public async Task<IActionResult> ParkTask([FromBody] clsTaskDto taskData, string user = "")
        {
            if (!UserValidation())
            {
                return Unauthorized();
            }
            return Ok(await AddTask(taskData, user));
        }

        /// <summary>
        /// Load/Unload完成回報
        /// </summary>
        /// <param name="agv_name"></param>
        /// <param name="LDULD">0:load , 1:unlod</param>
        /// <returns></returns>
        [HttpPost("LDULDFinishFeedback")]
        public async Task<IActionResult> LDULDFinishFeedback(string agv_name, int EQTag, int LDULD)
        {
            LOG.INFO($"AGVC LDULD REPORT : {agv_name} Finish {(LDULD == 0 ? "Load" : "Unload")} (EQ TAG={EQTag})");
            clsEQ eq = StaEQPManagager.GetEQByTag(EQTag);

            if (eq == null)
                return Ok(new { confirm = false });
            else
            {
                eq.CancelReserve();
            }
            if (AGVSConfigulator.SysConfigs.EQManagementConfigs.UseEQEmu)
            {
                _ = Task.Run(() =>
                   {
                       var eqEmu = StaEQPEmulatorsManagager.GetEQEmuByName(eq.EQName);
                       if (LDULD == 0)
                       {
                           eqEmu.SetStatusBUSY();
                           //Task.Factory.StartNew(async () =>
                           //{
                           //    await Task.Delay(3000); //等待3秒後 Unload Request ON ,模擬設備完成
                           //    eqEmu.SetStatusUnloadable();
                           //});
                       }
                       else if (LDULD == 1)
                           eqEmu.SetStatusLoadable();
                   });
            }

            return Ok(new { confirm = true });
        }

        [HttpPost("HotRun")]
        public async Task<IActionResult> SaveHotRun([FromBody] HotRunScript[] settings)
        {
            return Ok(new
            {
                result = HotRunScriptManager.Save(settings),
                message = ""
            });
        }
        [HttpGet("HotRun")]
        public async Task<IActionResult> GetHotRunScripts()
        {
            return Ok(HotRunScriptManager.HotRunScripts);
        }
        [HttpGet("HotRun/Start")]
        public async Task<IActionResult> StartHotRun(int no)
        {
            (bool confirm, string message) response = HotRunScriptManager.Run(no);
            return Ok(new { confirm = response.confirm, message = response.message });
        }
        [HttpGet("HotRun/Stop")]
        public async Task<IActionResult> StopHotRun(int no)
        {
            HotRunScriptManager.Stop(no);
            return Ok();
        }
        private async Task<object> AddTask(clsTaskDto taskData, string user = "")
        {
            taskData.DispatcherName = user;
            var result = await TaskManager.AddTask(taskData, TaskManager.TASK_RECIEVE_SOURCE.MANUAL);
            return new { confirm = result.confirm, alarm_code = result.alarm_code, message = result.message };
        }
        private bool UserValidation()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            if (identity != null)
            {
                IEnumerable<Claim> claims = identity.Claims;
                var userId = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                var userRole = claims.FirstOrDefault(c => c.Type == "Role")?.Value;

                if (userRole == ERole.VISITOR.ToString())
                    return false;

                return true;
            }
            else
                return false;
        }
    }
}
