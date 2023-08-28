using AGVSystem.Models.Map;
using AGVSystemCommonNet6.DATABASE;
using EquipmentManagment.ChargeStation;
using EquipmentManagment.Connection;
using EquipmentManagment.Device;
using EquipmentManagment.MainEquipment;
using EquipmentManagment.Manager;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace AGVSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public partial class EquipmentController : ControllerBase
    {
        [HttpGet("/ws/EQStatus")]
        public async Task EQStatus()
        {
            await WebsocketHandler.ClientRequest(HttpContext);

        }

        [HttpPost("WriteOutputs")]
        public async Task<IActionResult> WriteOutPuts(string EqName, ushort start, bool[] value)
        {
            var eq = StaEQPManagager.GetEQByName(EqName);
            eq.WriteOutputs(start, value);
            return Ok();
        }

        [HttpGet("GetEQInfoByTag")]
        public async Task<IActionResult> GetEQInfoByTag(int Tag)
        {
            var EQ = StaEQPManagager.EQOptions.Values.FirstOrDefault(eq => eq.TagID == Tag);
            return Ok(EQ);
        }

        [HttpGet("GetEQOptions")]
        public async Task<IActionResult> GetEQOptions()
        {
            return Ok(StaEQPManagager.EQOptions.Values.ToArray());
        }
        [HttpPost("SaveEQOptions")]
        public async Task<IActionResult> SaveEQOptions(List<clsEndPointOptions> datas)
        {
            //TODO 檢查是否有重複的設定 , 包含 Tag重複、IP:Port 重複、ComPort 重複
            var eqNames = datas.Select(data => data.Name).ToList().Distinct();
            if (eqNames.Count() != datas.Count)
            {
                return Ok(new { confirm = false, message = "重複的設備名稱，請再次確認設定" });
            }

            StaEQPManagager.EQOptions = datas.ToDictionary(eq => eq.Name, eq => eq);

            AGVSMapManager.SyncMapPointRegionSetting(StaEQPManagager.EQOptions);

            StaEQPManagager.DisposeEQs();
            StaEQPManagager.SaveEqConfigs();
            StaEQPManagager.InitializeAsync();

            return Ok(new { confirm = true, message = "" });
        }
        [HttpPost("ConnectTest")]
        public async Task<IActionResult> ConnectTest(ConnectOptions options)
        {

            clsEQ eq = new clsEQ(new clsEndPointOptions { ConnOptions = options });
            bool connected = await eq.Connect(use_for_conn_test: true);
            eq.Dispose();
            return Ok(new { Connected = connected });
        }

        [HttpGet("ChargeStation/Settings")]
        public async Task<IActionResult> ChargeStationCurveSetting(string EqName, string Item, double Value)
        {
            try
            {
                var charge_station = StaEQPManagager.EQPDevices.FirstOrDefault(eq => eq.EQName == EqName);
                if (charge_station == null)
                    return Ok(new { confirm = false, message = $"{EqName} is not exist" });
                clsChargeStation chargeStation = charge_station as clsChargeStation;

                var _item = Item.ToUpper();
                bool success = false;
                string message = "";
                if (_item == "CC")
                {
                    success = chargeStation.SetCC(Value, out message);
                }
                if (_item == "CV")
                {
                    success = chargeStation.SetCV(Value, out message);
                }
                if (_item == "FV")
                {
                    success = chargeStation.SetFV(Value, out message);
                }
                if (_item == "TC")
                {
                    success = chargeStation.SetTC(Value, out message);
                }
                return Ok(new { confirm = success, message = message });
            }
            catch (Exception ex)
            {
                return Ok(new { confirm = false, message = ex.Message });

            }
        }

    }

}
