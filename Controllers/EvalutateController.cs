using AGVSystem.Evaluation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AGVSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EvalutateController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> StartTest()
        {
            EvaluateFactory.StartTest();
            return Ok();
        }
    }
}
