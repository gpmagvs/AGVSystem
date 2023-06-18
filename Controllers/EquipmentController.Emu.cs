using EquipmentManagment;
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
            var outputs = new bool[8] { false, false, true, false, true, true, true, false };
            if(State=="busy")
                outputs = new bool[8] { false, false, true, false, true, true, true, false };
            if(State == "load")
                outputs = new bool[8] { true, false, false, false, true, true, true, false };
            if(State == "unload")
                outputs = new bool[8] { false, true, true, false, true, true, true, false };
            bool confirm = StaEQPEmulatorsManagager.InputsChange(EqName, 0, outputs);
            return Ok(confirm);
        }
    }
}
