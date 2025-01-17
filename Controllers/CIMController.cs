using AGVSystemCommonNet6.Microservices.MCS;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AGVSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CIMController : ControllerBase
    {
        [HttpPost("ChangePortType")]
        public async Task<IActionResult> ChangePortType(int eqTag, int portType)
        {
            try
            {

                GPMCIMService.clsCIMResponse response = await GPMCIMService.ChangePortTypeOfEq(eqTag, portType);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message + ex.StackTrace);
            }
        }
    }
}
