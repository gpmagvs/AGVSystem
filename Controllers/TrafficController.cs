using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AGVSystemCommonNet6.Microservices.ResponseModel;
using EquipmentManagment.Manager;
using AGVSystem.Models.Map;
namespace AGVSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrafficController : ControllerBase
    {
        [HttpGet("GetBlockedTagsByEqMaintain")]
        public async Task<IActionResult> GetBlockedTagsByEqMaintain()
        {
            try
            {
                List<int> MaintainingEQTags = StaEQPManagager.GetBlockedTagsByEqIsMainain();
                if (!MaintainingEQTags.Any())
                    return Ok(new clsAGVSBlockedPointsResponse(true, "", new List<int>()));

                List<int> tags = AGVSMapManager.GetEntryPointsOfTag(MaintainingEQTags);
                return Ok(new clsAGVSBlockedPointsResponse(true, "", tags));

            }
            catch (Exception ex)
            {
                return Ok(new clsAGVSBlockedPointsResponse(false, ex.Message, new List<int>()));
            }

        }
        [HttpGet("GetUseableChargeStationTags")]
        public async Task<IActionResult> GetUseableChargeStationTags(string agv_name)
        {
            try
            {
                var tags = StaEQPManagager.GetUsableChargeStationTags(agv_name).ToList();
                return Ok(new clsGetUsableChargeStationTagResponse(true, "", tags));

            }
            catch (Exception ex)
            {
                return Ok(new clsAGVSBlockedPointsResponse(false, ex.Message, new List<int>()));
            }

        }
    }
}
