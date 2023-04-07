using AGVSystem.Models.TaskAllocation;
using AGVSystem.TaskManagers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using static AGVSystem.UserManagers.UserEntity;

namespace AGVSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
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

        [HttpPost("move")]
        [Authorize]
        public async Task<IActionResult> MoveTask(clsMoveTaskDto taskData)
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
        public async Task<IActionResult> LoadTask(clsLoadTaskDto taskData)
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
        public async Task<IActionResult> UnloadTask(clsUnloadTaskDto taskData)
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
        public async Task<IActionResult> CarryTask(clsCarryTaskDto taskData)
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
        public async Task<IActionResult> ChargeyTask(clsChargeTaskDto taskData)
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
