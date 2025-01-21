using AGVSystem.Models.EQDevices;
using AGVSystem.Models.Map;
using AGVSystem.Service;
using AGVSystemCommonNet6.MAP;
using EquipmentManagment.Device.Options;
using EquipmentManagment.Manager;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static AGVSystem.Models.EQDevices.ZoneCapacityStatusMonitor;

namespace AGVSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WIPController : ControllerBase
    {
        private readonly RackService _rackControlService;
        public WIPController(RackService rackControlService)
        {
            _rackControlService = rackControlService;
        }

        [HttpPost("ModifyCargoID")]
        public async Task<IActionResult> ModifyCargoID(string WIPID, string PortID, string NewCargoID)
        {
            //StaEQPManagager.WIPController.ModifyCargoID(WIPID, PortID, NewCargoID);
            (bool confirm, string message) = await _rackControlService.AddRackCargoIDManual(WIPID, PortID, NewCargoID, this.GetType().Name, false);
            return Ok(new { confirm = confirm, message = message });
        }

        [HttpPost("RemoveCargoID")]
        public async Task<IActionResult> RemoveCargoID(string WIPID, string PortID)
        {
            //StaEQPManagager.WIPController.RemoveCargoID(WIPID, PortID);
            (bool confirm, string removedCarrierID, string message) = await _rackControlService.RemoveRackCargoIDManual(WIPID, PortID, this.GetType().Name, false);
            return Ok(new { confirm = confirm, message = message });
        }

        [HttpPost("PortUsableSwitch")]
        public async Task<IActionResult> PortUsableSwitch(string WIPID, string PortID, bool Usable)
        {
            //StaEQPManagager.WIPController.RemoveCargoID(WIPID, PortID);
            (bool confirm, string message) = await _rackControlService.PortUsableSwitch(WIPID, PortID, Usable);
            Task.Delay(100).ContinueWith((t) => { EQDeviceEventsHandler.BrocastRackData(); });
            return Ok(new { confirm = confirm, message = message });
        }


        [HttpPost("PortNoRename")]
        public async Task<IActionResult> PortNoRename(string WIPID, string PortID, string NewPortNo)
        {
            //StaEQPManagager.WIPController.RemoveCargoID(WIPID, PortID);
            (bool confirm, string message) = await _rackControlService.PortNoRename(WIPID, PortID, NewPortNo);
            Task.Delay(100).ContinueWith((t) => { EQDeviceEventsHandler.BrocastRackData(); });
            return Ok(new { confirm = confirm, message = message });
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

        [HttpGet("GetRackStatusData")]
        public async Task<List<ViewModel.WIPDataViewModel>> GetRackStatusData()
        {
            return _rackControlService.GetWIPDataViewModels();
        }

        [HttpGet("LowLevelNotifyOptions")]
        public async Task<object> GetLowLevelNotifyOptions()
        {
            Dictionary<string, clsZoneUsableCarrierOptions> options = ZoneCapacityStatusMonitor.LoadThresholdSettingFromJsonFile();
            return options.Select(pair => new
            {
                ZoneID = pair.Key,
                pair.Value.DisplayZoneName,
                pair.Value.ThresHoldValue,
                pair.Value.NotifyMessageTemplate,
            });
        }

        [HttpPost("LowLevelNotifyOptions")]
        public async Task SaveLowLevelNotifyOptions([FromBody] Dictionary<string, clsZoneUsableCarrierOptions> options)
        {
            ZoneCapacityStatusMonitor.lowLevelMonitorOptionsOfZones = options;
            ZoneCapacityStatusMonitor.SaveThresholdSettingToJsonFile();
        }
    }
}
