using AGVSystem.Service;
using AGVSystemCommonNet6.AGVDispatch;
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
        private readonly RackService _rackCargoStatusContorlService;
        private readonly AGVSDbContext dbContext;
        public AGVCargoTransferController(RackService rackCargoStatusContorlService, AGVSDbContext dbContext)
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
        public async Task<IActionResult> UnloadCargoFromPort(string taskName, string agvName, int tagNumber, int slot)
        {
            string removedCarrierID = string.Empty;
            (bool confirm, removedCarrierID, string message) = await _rackCargoStatusContorlService.RemoveRackCargoID(tagNumber, slot, this.GetType().Name, true);
            return Ok();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tagNumber"></param>
        /// <param name="slot"></param>
        /// <returns></returns>
        [HttpPost("LoadCargoToPort")]
        public async Task<IActionResult> LoadCargoToPort(string taskName, string agvName, int tagNumber, int slot, string cargoID = "")
        {
            var agvState = dbContext.AgvStates.FirstOrDefault(agv => agv.AGV_Name == agvName);
            string agvID = agvState.AGV_ID;
            clsTaskDto? order = dbContext.Tasks.AsNoTracking().FirstOrDefault(task => task.TaskName == taskName);
            string cargoIDToLoad = cargoID;
            if (order != null && order.soucePortID == agvID && (order.Carrier_ID.ToUpper().Contains("UN") || order.Carrier_ID.ToUpper().Contains("MI")))
                cargoIDToLoad = order.Carrier_ID;
            //await MCSCIMService.CarrierRemoveCompletedReport(cargoID, agvID, "", 1);
            await _rackCargoStatusContorlService.AddRackCargoID(tagNumber, slot, cargoIDToLoad, this.GetType().Name, true);
            return Ok();
        }
    }
}
