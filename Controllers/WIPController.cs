using EquipmentManagment.Manager;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AGVSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WIPController : ControllerBase
    {
        [HttpPost("ModifyCargoID")]
        public async Task<IActionResult> ModifyCargoID(string WIPID, string PortID, string NewCargoID)
        {
            NewCargoID = NewCargoID.Replace(" ", "");
            StaEQPManagager.WIPController.ModifyCargoID(WIPID, PortID, NewCargoID);
            return Ok(new { confirm = true, message = "" });
        }
        [HttpPost("RemoveCargoID")]
        public async Task<IActionResult> RemoveCargoID(string WIPID, string PortID)
        {
            StaEQPManagager.WIPController.ModifyCargoID(WIPID, PortID, "");

            return Ok(new { confirm = true, message = "" });
        }
    }
}
