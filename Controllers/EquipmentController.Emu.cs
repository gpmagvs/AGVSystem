﻿using EquipmentManagment.Emu;
using EquipmentManagment.Manager;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace AGVSystem.Controllers
{
    public partial class EquipmentController
    {

        [HttpGet("Emu/EQDownStatusSimulation")]
        public async Task<IActionResult> EQDownStatusSimulation(int TagNumber, bool isDown)
        {
            bool confirm = StaEQPEmulatorsManagager.EQDownStatusSimulation(TagNumber, isDown);
            return Ok(confirm);
        }

        [HttpGet("Emu/MaintainStatusSimulation")]
        public async Task<IActionResult> MaintainStatusSimulation(int TagNumber, bool isMaintain)
        {
            bool confirm = StaEQPEmulatorsManagager.MaintainStatusSimulation(TagNumber, isMaintain);
            return Ok(confirm);
        }



        [HttpGet("Emu/PartsReplcingSimulation")]
        public async Task<IActionResult> PartsReplcingSimulation(int TagNumber, bool isPartsReplcing)
        {
            bool confirm = StaEQPEmulatorsManagager.PartsReplacingSimulation(TagNumber, isPartsReplcing);
            return Ok(confirm);
        }
        [HttpGet("Emu/InputChange")]
        public async Task<IActionResult> InputChange(string EqName, int index, bool value)
        {
            bool confirm = StaEQPEmulatorsManagager.InputChange(EqName, index, value);
            return Ok(confirm);
        }

        [HttpPost("Emu/DisposeModbusTcpServer")]
        public async Task<IActionResult> DisposeModbusTcpServer(string EqName)
        {
            StaEQPEmulatorsManagager.DisposeModbusTcpServer(EqName);
            return Ok();
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
                if (StaEQPEmulatorsManagager.TryGetEQEmuByName(EqName, out EQEmulatorBase? EQ))
                {


                    var anotherPortEq = StaEQPEmulatorsManagager.EqEmulators.Values.FirstOrDefault(eq => eq.options.TagID == EQ.options.AnotherPortTagNumber);

                    if (State == "busy")
                    {
                        confirm = EQ.SetStatusBUSY();
                        anotherPortEq?.SetStatusBUSY();
                    }
                    if (State == "load")
                    {
                        confirm = EQ.SetStatusLoadable();
                        anotherPortEq?.SetStatusLoadable();
                    }
                    if (State == "unload")
                    {
                        confirm = EQ.SetStatusUnloadable();
                        anotherPortEq?.SetStatusUnloadable();
                    }
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


        [HttpGet("Emu/HsSignal")]
        public async Task<IActionResult> HsSignal(string EqName, string SignalName, bool State)
        {
            bool confirm = false;
            string message = "";
            try
            {
                if (StaEQPEmulatorsManagager.TryGetEQEmuByName(EqName, out EQEmulatorBase? EQ))
                {
                    if (SignalName == "L_REQ")
                        confirm = EQ.SetHS_L_REQ(State);
                    if (SignalName == "U_REQ")
                        confirm = EQ.SetHS_U_REQ(State);
                    if (SignalName == "READY")
                        confirm = EQ.SetHS_READY(State);
                    if (SignalName == "UP_READY")
                        confirm = EQ.SetHS_UP_READY(State);
                    if (SignalName == "LOW_READY")
                        confirm = EQ.SetHS_LOW_READY(State);
                    if (SignalName == "BUSY")
                        confirm = EQ.SetHS_BUSY(State);
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


        [HttpGet("Emu/SetUpPose")]
        public async Task<IActionResult> SetUpPose(string EqName)
        {
            if (StaEQPEmulatorsManagager.TryGetEQEmuByName(EqName, out EQEmulatorBase? EQ))
            {
                EQ.SetUpPose();
            }
            return Ok();
        }
        [HttpGet("Emu/SetDownPose")]
        public async Task<IActionResult> SetDownPose(string EqName)
        {
            if (StaEQPEmulatorsManagager.TryGetEQEmuByName(EqName, out EQEmulatorBase? EQ))
            {
                EQ.SetDownPose();
            }
            return Ok();
        }
        [HttpGet("Emu/SetUnknownPose")]
        public async Task<IActionResult> SetUnknownPose(string EqName)
        {
            if (StaEQPEmulatorsManagager.TryGetEQEmuByName(EqName, out EQEmulatorBase? EQ))
            {
                EQ.SetUnknownPose();
            }
            return Ok();
        }


        [HttpGet("Emu/PortExist")]
        public async Task<IActionResult> SetPortExist(string EqName, int portExist)
        {
            if (StaEQPEmulatorsManagager.TryGetEQEmuByName(EqName, out EQEmulatorBase? EQ))
            {
                EQ.SetPortExist(portExist);
            }
            return Ok();
        }

        [HttpGet("Emu/SetPortType")]
        public async Task<IActionResult> SetPortType(string EqName, int PortType)
        {
            if (StaEQPEmulatorsManagager.TryGetEQEmuByName(EqName, out EQEmulatorBase? EQ))
            {
                EQ.SetPortType(PortType);
                var anotherPortEq = StaEQPEmulatorsManagager.EqEmulators.Values.FirstOrDefault(eq => eq.options.TagID == EQ.options.AnotherPortTagNumber);
                if (anotherPortEq != null)
                {
                    anotherPortEq.SetPortType(PortType);
                }
            }
            return Ok();
        }
        [HttpPost("Emu/CSTReadID")]
        public async Task<IActionResult> SetCSTReadID(string EqName, string? CarrierID)
        {
            if (StaEQPEmulatorsManagager.TryGetEQEmuByName(EqName, out EQEmulatorBase? EQ))
            {
                EQ.SetCarrierIDRead(CarrierID);
            }
            return Ok();
        }

    }
}
