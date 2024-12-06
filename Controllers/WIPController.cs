using AGVSystem.Models.Map;
using AGVSystem.Service;
using AGVSystemCommonNet6.MAP;
using EquipmentManagment.Device.Options;
using EquipmentManagment.Manager;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AGVSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WIPController : ControllerBase
    {
        private readonly RackCargoStatusContorlService _rackControlService;
        public WIPController(RackCargoStatusContorlService rackControlService)
        {
            _rackControlService = rackControlService;
        }

        [HttpPost("ModifyCargoID")]
        public async Task<IActionResult> ModifyCargoID(string WIPID, string PortID, string NewCargoID)
        {
            //StaEQPManagager.WIPController.ModifyCargoID(WIPID, PortID, NewCargoID);
            _rackControlService.AddRackCargoID(WIPID, PortID, NewCargoID, this.GetType().Name, false);
            return Ok(new { confirm = true, message = "" });
        }

        [HttpPost("RemoveCargoID")]
        public async Task<IActionResult> RemoveCargoID(string WIPID, string PortID)
        {
            //StaEQPManagager.WIPController.RemoveCargoID(WIPID, PortID);
            _rackControlService.RemoveRackCargoID(WIPID, PortID, this.GetType().Name, false);
            return Ok(new { confirm = true, message = "" });
        }

        [HttpGet("GetAllSlotsOptions")]
        public async Task<IActionResult> GetAllSlotsOptions()
        {
            IEnumerable<int> tags = StaEQPManagager.RacksOptions.Values.SelectMany(opt => opt.ColumnTagMap.Values.SelectMany(tag => tag));
            Dictionary<int, string[]> slotPortsNoDisplayMap = tags.ToDictionary(tag => tag, tag => GetPortNosOfRackColumn(tag));

            string[] GetPortNosOfRackColumn(int tag)
            {
                clsRackOptions RackOption = StaEQPManagager.RacksOptions.Values.FirstOrDefault(opt => opt.ColumnTagMap.Values.Any(tags => tags.Contains(tag)));
                if (RackOption != null)
                {
                    int column = RackOption.ColumnTagMap.ToList().FindIndex(pt => pt.Value.Contains(tag));
                    IEnumerable<clsRackPortProperty> ports = RackOption.PortsOptions.Where(portOpt => portOpt.Column == column);
                    string[] portNos = ports.OrderBy(port => port.Row).Select(col => col.PortNo).ToArray();
                    return portNos;
                }
                return new string[3] { "0", "1", "2" };
            }
            return Ok(slotPortsNoDisplayMap);
        }
    }
}
