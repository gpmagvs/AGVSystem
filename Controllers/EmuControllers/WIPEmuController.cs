using AGVSystemCommonNet6.Configuration;
using AGVSystemCommonNet6.Log;
using EquipmentManagment.Device.Options;
using EquipmentManagment.Manager;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using NuGet.DependencyResolver;
using SQLite;
using static EquipmentManagment.WIP.clsPortOfRack;

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
            io.ModifyInput(2, !io.GetInput(2));
            return Ok();
        }


        [HttpGet("WIP_IO_Toggle")]
        public async Task<IActionResult> WIP_IO_Modify(int port, int index)
        {
            var IOModuleEmu = StaEQPEmulatorsManagager.WIPEmulators.Values.First(wip => wip.options.ConnOptions.Port == port);
            IOModuleEmu.ModifyInput(index, !IOModuleEmu.GetInput(index));
            return Ok();
        }


        [HttpGet("SetSensorState")]
        public async Task<IActionResult> SetSensorState(string rack_id, string port_id, string cargo_type, int sensor_number, bool state)
        {
            EquipmentManagment.Emu.clsWIPEmu? IOModuleEmu = StaEQPEmulatorsManagager.WIPEmulators.Values.FirstOrDefault(wip => wip.options.Name == rack_id);
            if (IOModuleEmu == null)
                return Ok(new { confirm = false, message = $"Rack-{rack_id} Not Exist" });

            // clsRackIOLocation iolocation = (clsRackIOLocation)IOModuleEmu.options.IOLocation;
            //IOModuleEmu.ModifyInput(index, !IOModuleEmu.GetInput(index));

            var rack = StaEQPManagager.RacksList.FirstOrDefault(rack => rack.EQName == rack_id);
            
            if(!rack.RackOption.IsEmulation)
                return Ok(new { confirm = false, message = $"Not simulation mode, modify sensor io state is invalid." });

            var port = rack?.PortsStatus.FirstOrDefault(port => port.Properties.ID == port_id);
            //if (port.RackPlacementState == CARGO_PLACEMENT_STATUS.NO_CARGO_BUT_CLICK)
            //{
            //    return Ok(new { confirm = false, message = $"{rack_id}-{port_id},Rack Sensor is Flash" });
            //}
            //else if (port.TrayPlacementState == CARGO_PLACEMENT_STATUS.NO_CARGO_BUT_CLICK)
            //{
            //    return Ok(new { confirm = false, message = $"{rack_id}-{port_id},Tray Sensor is Flash" });
            //}
            var ioLocation = port.Properties.IOLocation;

            int _io_location = 0;
            if (cargo_type == "rack")
                _io_location = sensor_number == 0 ? ioLocation.Box_Sensor1 : ioLocation.Box_Sensor2;
            if (cargo_type == "tray")
                _io_location = sensor_number == 0 ? ioLocation.Tray_Sensor1 : ioLocation.Tray_Sensor2;
            var _current_state = IOModuleEmu.GetInput(_io_location);
            IOModuleEmu.ModifyInput(_io_location, !_current_state);
            return Ok(new { confirm = true, message = "" });
        }



    }
}
