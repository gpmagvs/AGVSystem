using EquipmentManagment.Device.Options;
using EquipmentManagment.Manager;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AGVSystem.Controllers.EmuControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WIPEmuController : ControllerBase
    {
        [HttpGet("GetEmulatorsInfo")]
        public async Task<IActionResult> GetEmulatorsInfo()
        {
            var options = StaEQPEmulatorsManagager.WIPEmulators.Values.Select(emu => (clsRackOptions)emu.options).ToList();
            return Ok(options);
        }

        [HttpGet("Test1")]
        public async Task<IActionResult> Test1()
        {
            var io = StaEQPEmulatorsManagager.WIPEmulators.Values.First();
            io.ModifyInput(2, !io.GetInput(2)) ;
            return Ok();
        }


        [HttpGet("WIP_IO_Toggle")]
        public async Task<IActionResult> WIP_IO_Modify(int port,int index)
        {
            var IOModuleEmu = StaEQPEmulatorsManagager.WIPEmulators.Values.First(wip=>wip.options.ConnOptions.Port==port);
            IOModuleEmu.ModifyInput(index, !IOModuleEmu.GetInput(index));
            return Ok();
        }



    }
}
