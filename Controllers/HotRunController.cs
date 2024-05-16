using AGVSystem.Models.TaskAllocation.HotRun;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AGVSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HotRunController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> SaveHotRun([FromBody] List<HotRunScript> settings)
        {
            return Ok(new
            {
                result = HotRunScriptManager.Save(settings),
                message = ""
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetHotRunScripts()
        {
            return Ok(HotRunScriptManager.HotRunScripts);
        }
        [HttpGet("Start")]
        public async Task<IActionResult> StartHotRun(string scriptID)
        {
            (bool confirm, string message) response = HotRunScriptManager.Run(scriptID);
            return Ok(new { confirm = response.confirm, message = response.message });
        }
        [HttpGet("Stop")]
        public async Task<IActionResult> StopHotRun(string scriptID)
        {
            HotRunScriptManager.Stop(scriptID);
            return Ok();
        }
    }
}
