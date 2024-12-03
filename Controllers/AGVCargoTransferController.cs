using AGVSystem.Service;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.Microservices.MCS;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AGVSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AGVCargoTransferController : ControllerBase
    {
        private readonly RackCargoStatusContorlService _rackCargoStatusContorlService;
        private readonly AGVSDbContext dbContext;
        public AGVCargoTransferController(RackCargoStatusContorlService rackCargoStatusContorlService, AGVSDbContext dbContext)
        {
            _rackCargoStatusContorlService = rackCargoStatusContorlService;
            this.dbContext = dbContext;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tagNumber"></param>
        /// <param name="slot"></param>
        /// <returns></returns>
        [HttpPost("UnloadCargoFromPort")]
        public async Task<IActionResult> UnloadCargoFromPort(string agvName, int tagNumber, int slot)
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
        public async Task<IActionResult> LoadCargoToPort(string agvName, int tagNumber, int slot, string cargoID = "")
        {
            var agvState = dbContext.AgvStates.FirstOrDefault(agv => agv.AGV_Name == agvName);
            string agvID = agvState.AGV_ID;
            await MCSCIMService.CarrierRemoveCompletedReport(cargoID, agvID, "", 1);
            await _rackCargoStatusContorlService.AddRackCargoID(tagNumber, slot, cargoID, this.GetType().Name);
            return Ok();
        }
    }
}
