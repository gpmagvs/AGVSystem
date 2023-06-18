using AGVSystem.Models.TaskAllocation;
using AGVSystem.TaskManagers;
using AGVSystemCommonNet6.Alarm;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.TASK;
using AGVSystemCommonNet6.User;
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

        [HttpGet("/ws/TaskData")]
        public async Task GetNotFinishTaskListData()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                var websocket_client = await HttpContext.WebSockets.AcceptWebSocketAsync();
                byte[] rev_buffer = new byte[4096];
                int incomTaskTotalNum = -1;
                int comTaskTotalNum = -1;
                while (websocket_client.State == System.Net.WebSockets.WebSocketState.Open)
                {
                    Thread.Sleep(1000);

                    websocket_client.ReceiveAsync(new ArraySegment<byte>(rev_buffer), CancellationToken.None);

                    clsTaskDto[] incompleteds = TaskAllocator.InCompletedTaskList.ToArray();
                    clsTaskDto[] completeds = TaskAllocator.CompletedTaskList.FindAll(task => task.RecieveTime.Date == DateTime.Now.Date).ToArray();
                    var _incomtasks = incompleteds.Length;
                    var _comtasks = completeds.Length;

                    if (_incomtasks != incomTaskTotalNum | _comtasks != comTaskTotalNum) //有變化再傳送
                    {
                        incomTaskTotalNum =_incomtasks;
                        comTaskTotalNum = _comtasks;
                        var dto = new { incompleteds, completeds };
                        var _dtoJson = JsonConvert.SerializeObject(dto);
                        await websocket_client.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(_dtoJson)), System.Net.WebSockets.WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

        [HttpGet("Cancel")]
        [Authorize]
        public async Task<IActionResult> Cancel(string task_name)
        {
            if (!UserValidation())
            {
                return Unauthorized();
            }

            bool canceled = TaskAllocator.Cancel(task_name);
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
        private async Task<object> AddTask(clsTaskDto taskData)
        {
            var result = await TaskAllocator.AddTask(taskData);
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
