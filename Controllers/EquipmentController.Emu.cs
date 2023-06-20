using EquipmentManagment;
using EquipmentManagment.Emu;
using Microsoft.AspNetCore.Mvc;

namespace AGVSystem.Controllers
{
    public partial class EquipmentController
    {
        [HttpGet("Emu/InputChange")]
        public async Task<IActionResult> InputChange(string EqName, int index, bool value)
        {
            bool confirm = StaEQPEmulatorsManagager.InputChange(EqName, index, value);
            return Ok(confirm);
        }

        [HttpPost("Emu/AsUnloadState")]
        public async Task<IActionResult> AsUnloadState(string EqName)
        {
            bool confirm = StaEQPEmulatorsManagager.InputsChange(EqName, 0, new bool[8] {
                false,true,true,false,true,true,true,false
            });
            return Ok(confirm);
        }
        [HttpPost("Emu/AsLoadState")]
        public async Task<IActionResult> AsLoadState(string EqName)
        {
            bool confirm = StaEQPEmulatorsManagager.InputsChange(EqName, 0, new bool[8] {
                true,false,false,false,true,true,true,false
            });
            return Ok(confirm);
        }

        [HttpGet("Emu/State")]
        public async Task<IActionResult> AsBusyState(string EqName, string State)
        {
            bool confirm = false;
            string message = "";
            try
            {
                if (StaEQPEmulatorsManagager.TryGetEQEmuByName(EqName, out clsDIOModuleEmu? EQ))
                {
                    if (State == "busy")
                        confirm = EQ.SetStatusBUSY();
                    if (State == "load")
                        confirm = EQ.SetStatusLoadable();
                    if (State == "unload")
                        confirm = EQ.SetStatusUnloadable();
                }
                else
                {
                    message = $"{EqName} not exist.";
                    confirm = false;
                }
            }
            catch (Exception ex)
            {
                message = $"Exception:{ex.Message}";
                confirm = false;
            }

            return Ok(new { confirm, message });
        }

        [HttpGet("Emu/AllBusy")]
        public async Task<IActionResult> AllBusy()
        {
            StaEQPEmulatorsManagager.ALLBusy();
            return Ok();
        }

        [HttpGet("Emu/AllLoad")]
        public async Task<IActionResult> AllLoad()
        {
            StaEQPEmulatorsManagager.ALLLoad();
            return Ok();
        }
    }
}
