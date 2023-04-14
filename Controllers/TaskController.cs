using AGVSystem.Models.TaskAllocation;
using AGVSystem.TaskManagers;
using AGVSystemCommonNet6.DATABASE;
using AGVSytemCommonNet6.TASK;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static AGVSystemCommonNet6.UserManagers.UserEntity;

namespace AGVSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
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

        [HttpGet("/ws/InCompletedTaskList")]
        public async Task GetNotFinishTaskListData()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                var websocket_client = await HttpContext.WebSockets.AcceptWebSocketAsync();
                while (websocket_client.State == System.Net.WebSockets.WebSocketState.Open)
                {
                    Thread.Sleep(1000);

                    byte[] rev_buffer = new byte[4096];

                    websocket_client.ReceiveAsync(new ArraySegment<byte>(rev_buffer), CancellationToken.None);

                    var data = TaskAllocator.TaskList.FindAll(tk => tk.State != TASK_RUN_STATE.FINISH).ToArray();
                    await websocket_client.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data))), System.Net.WebSockets.WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

        [HttpPost("move")]
        [Authorize]
        public async Task<IActionResult> MoveTask(clsTaskDispatchDto taskData)
        {
            if (!UserValidation())
            {
                return Unauthorized();
            }

            TaskAllocator.AddTask(taskData);

            return Ok();
        }
        [HttpPost("load")]
        [Authorize]
        public async Task<IActionResult> LoadTask(clsTaskDispatchDto taskData)
        {
            if (!UserValidation())
            {
                return Unauthorized();
            }

            TaskAllocator.AddTask(taskData);

            return Ok();
        }
        [HttpPost("unload")]
        [Authorize]
        public async Task<IActionResult> UnloadTask(clsTaskDispatchDto taskData)
        {
            if (!UserValidation())
            {
                return Unauthorized();
            }

            TaskAllocator.AddTask(taskData);

            return Ok();
        }
        [HttpPost("carry")]
        [Authorize]
        public async Task<IActionResult> CarryTask(clsTaskDispatchDto taskData)
        {
            if (!UserValidation())
            {
                return Unauthorized();
            }

            TaskAllocator.AddTask(taskData);

            return Ok();
        }
        [HttpPost("charge")]
        [Authorize]
        public async Task<IActionResult> ChargeTask(clsTaskDispatchDto taskData)
        {
            if (!UserValidation())
            {
                return Unauthorized();
            }

            TaskAllocator.AddTask(taskData);

            return Ok();
        }

        private bool UserValidation()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            if (identity != null)
            {
                IEnumerable<Claim> claims = identity.Claims;
                var userId = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                var userRole = claims.FirstOrDefault(c => c.Type == "Role")?.Value;

                if (userRole == USER_ROLE.VISITOR.ToString())
                    return false;

                return true;
            }
            else
                return false;
        }
    }
}
