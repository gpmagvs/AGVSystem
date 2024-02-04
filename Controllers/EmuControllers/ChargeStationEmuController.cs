using EquipmentManagment.Manager;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AGVSystem.Controllers.EmuControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChargeStationEmuController : ControllerBase
    {
        [HttpGet("NoAGVCharging")]
        public async Task<IActionResult> NoAGVCharging()
        {
            var charge_station = StaEQPEmulatorsManagager.ChargeStationEmulators.FirstOrDefault().Value;
            charge_station.AGVNoCharging();
            return Ok();
        }
        [HttpGet("AGVCharging")]
        public async Task<IActionResult> AGVCharging()
        {
            var charge_station = StaEQPEmulatorsManagager.ChargeStationEmulators.FirstOrDefault().Value;
            charge_station.AGVCharging();
            return Ok();
        }
    }
}
