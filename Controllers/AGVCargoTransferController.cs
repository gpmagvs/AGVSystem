using AGVSystem.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AGVSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AGVCargoTransferController : ControllerBase
    {
        private readonly RackCargoStatusContorlService _rackCargoStatusContorlService;
        public AGVCargoTransferController(RackCargoStatusContorlService rackCargoStatusContorlService)
        {
            _rackCargoStatusContorlService = rackCargoStatusContorlService;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tagNumber"></param>
        /// <param name="slot"></param>
        /// <returns></returns>
        [HttpPost("UnloadCargoFromPort")]
        public async Task<IActionResult> UnloadCargoFromPort(int tagNumber, int slot)
        {
            await _rackCargoStatusContorlService.RemoveRackCargoID(tagNumber, slot, this.GetType().Name);
            return Ok();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tagNumber"></param>
        /// <param name="slot"></param>
        /// <returns></returns>
        [HttpPost("LoadCargoToPort")]
        public async Task<IActionResult> LoadCargoToPort(int tagNumber, int slot, string cargoID = "")
        {
            await _rackCargoStatusContorlService.AddRackCargoID(tagNumber, slot, cargoID, this.GetType().Name);
            return Ok();
        }
    }
}
