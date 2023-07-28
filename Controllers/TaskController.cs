using AGVSystem.Models.TaskAllocation;
using AGVSystem.TaskManagers;
using AGVSystemCommonNet6.Alarm;
using AGVSystemCommonNet6.Configuration;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.Log;
using AGVSystemCommonNet6.TASK;
using AGVSystemCommonNet6.User;
using EquipmentManagment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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

        [HttpGet("Cancel")]
        [Authorize]
        public async Task<IActionResult> Cancel(string task_name)
        {
            if (!UserValidation())
            {
                return Unauthorized();
            }

            bool canceled = TaskManager.Cancel(task_name);
            return Ok(canceled);
        }

        [HttpPost("move")]
        [Authorize]
        public async Task<IActionResult> MoveTask(clsTaskDto taskData)
        {
            if (!UserValidation())
            {
                return Unauthorized();
            }
            return Ok(await AddTask(taskData));
        }
        [HttpPost("load")]
        [Authorize]
        public async Task<IActionResult> LoadTask(clsTaskDto taskData)
        {
            if (!UserValidation())
            {
                return Unauthorized();
            }
            return Ok(await AddTask(taskData));
        }
        [HttpPost("unload")]
        [Authorize]
        public async Task<IActionResult> UnloadTask(clsTaskDto taskData)
        {
            if (!UserValidation())
            {
                return Unauthorized();
            }
            return Ok(await AddTask(taskData));
        }
        [HttpPost("carry")]
        [Authorize]
        public async Task<IActionResult> CarryTask(clsTaskDto taskData)
        {
            if (!UserValidation())
            {
                return Unauthorized();
            }
            return Ok(await AddTask(taskData));
        }
        [HttpPost("charge")]
        [Authorize]
        public async Task<IActionResult> ChargeTask(clsTaskDto taskData)
        {
            if (!UserValidation())
            {
                return Unauthorized();
            }
            return Ok(await AddTask(taskData));
        }
        [HttpPost("park")]
        [Authorize]
        public async Task<IActionResult> ParkTask(clsTaskDto taskData)
        {
            if (!UserValidation())
            {
                return Unauthorized();
            }
            return Ok(await AddTask(taskData));
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

            if (AGVSConfigulator.SysConfigs.EQManagementConfigs.UseEQEmu)
            {
                _ = Task.Run(() =>
                   {
                       var eq = StaEQPManagager.GetEQByTag(EQTag);
                       if (eq == null)
                           return;
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


        private async Task<object> AddTask(clsTaskDto taskData)
        {
            taskData.DispatcherName = "Web-USER";
            var result = await TaskManager.AddTask(taskData, TaskManager.TASK_RECIEVE_SOURCE.MANUAL);
            return new { confirm = result.Item1, message = result.Item2.ToString() };
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
