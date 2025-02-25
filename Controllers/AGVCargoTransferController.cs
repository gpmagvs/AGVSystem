using AGVSystem.Service;
using AGVSystem.TaskManagers;
using AGVSystemCommonNet6.AGVDispatch;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.Microservices.MCS;
using EquipmentManagment.Emu;
using EquipmentManagment.Manager;
using EquipmentManagment.WIP;
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
            if (IsSimulationRackPort(tagNumber, slot, out clsPortOfRack rackPort, out clsWIPEmu wipEmu))
            {
                wipEmu.OffExistSensor(rackPort);
            }
            string removedCarrierID = string.Empty;
            bool isEmuEqStation = IsSimulationEq(tagNumber, slot, out EQEmulatorBase emulator);
            if (isEmuEqStation)
            {
                emulator.SetPortExist(0);
                emulator.SetCarrierIDRead("");
                await Task.Delay(300);
            }
            (bool confirm, removedCarrierID, string message) = await _rackCargoStatusContorlService.RemoveRackCargoID(tagNumber, slot, this.GetType().Name, isByAgvUnloadend: true);
            int removedNum = EQTransferTaskManager.TryRemoveWaitUnloadEQ(tagNumber, slot);

            if (isEmuEqStation)
            {
                _ = Task.Delay(100).ContinueWith((t) =>
                {
                    emulator.SetStatusLoadable();
                });
            }

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
            if (IsSimulationRackPort(tagNumber, slot, out clsPortOfRack rackPort, out clsWIPEmu emulator))
            {
                emulator.OnExistSensor(rackPort);
                await Task.Delay(500);
            }
            var agvState = dbContext.AgvStates.FirstOrDefault(agv => agv.AGV_Name == agvName);
            string agvID = agvState.AGV_ID;
            clsTaskDto? order = dbContext.Tasks.AsNoTracking().FirstOrDefault(task => task.TaskName == taskName);
            string cargoIDToLoad = cargoID;
            if (order != null && order.soucePortID == agvID && (order.Carrier_ID.ToUpper().Contains("UN") || order.Carrier_ID.ToUpper().Contains("MI")))
                cargoIDToLoad = order.Carrier_ID;
            await _rackCargoStatusContorlService.AddRackCargoID(tagNumber, slot, cargoIDToLoad, this.GetType().Name, isByAgvLoadend: true, order: order);

            int removedNum = EQTransferTaskManager.TryRemoveWaitLoadEQ(tagNumber, slot);

            //await MCSCIMService.CarrierRemoveCompletedReport(cargoID, agvID, "", 1);

            //if (isEmuEqStation)
            //    _ = Task.Delay(100).ContinueWith((t) =>
            //    {
            //        emulator.SetStatusUnloadable();
            //    });
            return Ok();
        }

        bool IsSimulationRackPort(int tag, int slot, out clsPortOfRack rackPort, out clsWIPEmu emulator)
        {
            emulator = null;
            rackPort = null;
            rackPort = StaEQPManagager.RackPortsList.FirstOrDefault(port => port.TagNumbers.Contains(tag) && port.Properties.Row == slot);
            if (rackPort == null)
                return false;

            StaEQPEmulatorsManagager.WIPEmulators.TryGetValue(rackPort.GetParentRack().EQName, out emulator);

            return emulator != null;
        }
        bool IsSimulationEq(int _tag, int _slot, out EQEmulatorBase emulator)
        {
            emulator = null;
            var eq = StaEQPManagager.GetEQByTag(_tag, _slot);
            if (eq != null && eq.EndPointOptions.IsEmulation)
                emulator = StaEQPEmulatorsManagager.GetEQEmuByName(eq.EQName);
            return emulator != null;
        }
    }
}
