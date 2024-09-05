using AGVSystem.Models.Automation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AGVSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AutomationController : ControllerBase
    {

        [HttpPost("InvokeTaskReportAutomationAction")]
        public async Task<IActionResult> InvokeTaskReportAutomationAction(bool autoRetry = false)
        {
            try
            {
                AutomationBase? automationProvider = AutomationManager.AutomationTasks.FirstOrDefault(automation => automation.options.TaskName == "Task History Report Save Automaiton");
                if (automationProvider == null)
                {
                    return NotFound("Task History Report Save Automaiton not found");
                }
                (bool success, string message) = automationProvider.ExecuteTaskAndInvokeReulst(autoRetry).GetAwaiter().GetResult();
                return Ok(new
                {
                    success = success,
                    message = message
                });
            }
            catch (Exception ex)
            {
                return BadRequest();
            }

        }

    }
}
