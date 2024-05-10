using AGVSystem.Models.Map;
using AGVSystem.Models.WebsocketMiddleware;
using AGVSystem.TaskManagers;
using AGVSystemCommonNet6.DATABASE;
using EquipmentManagment.ChargeStation;
using EquipmentManagment.Connection;
using EquipmentManagment.Device.Options;
using EquipmentManagment.Emu;
using EquipmentManagment.MainEquipment;
using EquipmentManagment.Manager;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Options;
using Newtonsoft.Json;
using System.Text;
using System.Xml.Linq;

namespace AGVSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public partial class EquipmentController : ControllerBase
    {

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
            clsEndPointOptions[] MainEQs = StaEQPManagager.MainEQList.Select(eq => eq.EndPointOptions).ToArray();
            return Ok(MainEQs);
        }
        [HttpGet("GetWIPOptions")]
        public async Task<IActionResult> GetWIPOptions()
        {
            clsEndPointOptions[] WIPs = StaEQPManagager.RacksList.Select(wip => wip.EndPointOptions).ToArray();
            return Ok(WIPs);
        }
        [HttpGet("GetEQOptionByTag")]
        public async Task<IActionResult> GetEQOptionByTag(int eq_tag)
        {
            var eqoptions = StaEQPManagager.MainEQList.Select(eq => eq.EndPointOptions);
            var option = (eqoptions.FirstOrDefault(opt => opt.TagID == eq_tag));
            if (option != null)
            {
                return Ok(new { Tag = option.TagID, EqName = option.Name, AGVModbusGatewayPort = option.ConnOptions.AGVModbusGatewayPort });
            }
            else
            {
                return BadRequest();
            }
        }
        [HttpPost("GetEQOptionsByTags")]
        public async Task<IActionResult> GetEQOptionsByTags([FromBody] int[] eq_tags)
        {
            var eqoptions = StaEQPManagager.MainEQList.Select(eq => eq.EndPointOptions);
            var options = eqoptions.Where(opt => eq_tags.Contains(opt.TagID)).Select(option => new
            {
                Tag = option.TagID,
                EqName = option.Name,
                AGVModbusGatewayPort = option.ConnOptions.AGVModbusGatewayPort,
                Accept_AGV_Type = option.Accept_AGV_Type.ToAGVModel()
            }).ToList();
            return Ok(options);
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

            //StaEQPManagager.DisposeEQs();
            StaEQPManagager.SaveEqConfigs();
            //StaEQPManagager.InitializeAsync();

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
                var charge_station = StaEQPManagager.ChargeStations.FirstOrDefault(eq => eq.EQName == EqName);
                if (charge_station == null)
                    return Ok(new { confirm = false, message = $"{EqName} is not exist" });

                clsChargeStation chargeStation = charge_station as clsChargeStation;
                bool isGy7601 = (chargeStation.EndPointOptions as clsChargeStationOptions).chip_brand == 2;

                var _item = Item.ToUpper();
                bool success = false;
                string message = "";
                if (_item == "CC")
                {
                    if (isGy7601)
                    {
                        var result = (await (chargeStation as clsChargeStationGY7601Base).SetCCAsync(Value));
                        success = result.Item1;
                        message = result.Item2;
                    }
                    else
                    {
                        success = chargeStation.SetCCAsync(Value, out message);
                    }
                }
                if (_item == "CV")
                {
                    if (isGy7601)
                    {
                        var result = (await (chargeStation as clsChargeStationGY7601Base).SetCVAsync(Value));
                        success = result.Item1;
                        message = result.Item2;
                    }
                    else
                        success = chargeStation.SetCVAsync(Value, out message);
                }
                if (_item == "FV")
                {
                    if (isGy7601)
                    {
                        var result = (await (chargeStation as clsChargeStationGY7601Base).SetFV(Value));
                        success = result.Item1;
                        message = result.Item2;
                    }
                    else
                        success = chargeStation.SetFV(Value, out message);
                }
                if (_item == "TC")
                {
                    if (isGy7601)
                    {
                        var result = (await (chargeStation as clsChargeStationGY7601Base).SetTCAsync(Value));
                        success = result.Item1;
                        message = result.Item2;
                    }
                    else
                        success = chargeStation.SetTCAsync(Value, out message);
                }
                return Ok(new { confirm = success, message = message });
            }
            catch (Exception ex)
            {
                return Ok(new { confirm = false, message = ex.Message });

            }
        }


        [HttpPost("ChargeStation/SaveUsableAGVSetting")]
        public async Task<IActionResult> SaveUsableAGVSetting([FromBody] string[] agvNames, string ChargeStationName)
        {
            try
            {

                var charge_station = StaEQPManagager.ChargeStations.FirstOrDefault(eq => eq.EQName == ChargeStationName);
                if (charge_station == null)
                    return Ok(new { confirm = false, message = $"Charge Station:{ChargeStationName} is not exist" });
                charge_station.SetUsableAGVList(agvNames);
                StaEQPManagager.SaveChargeStationConfigs();
                return Ok(new { confirm = true, message = "" });
            }
            catch (Exception ex)
            {
                return Ok(new { confirm = false, message = ex.Message });
            }
        }

        [HttpPost("AgvHsSignal")]
        public async Task<IActionResult> AgvHsSignal(string EqName, string SignalName, bool State)
        {
            bool confirm = false;
            string message = "";
            try
            {
                if (StaEQPManagager.TryGetEQByEqName(EqName, out clsEQ? EQ, out string errmsg))
                {

                    if (SignalName == "To_EQ_Up")
                        EQ.To_EQ_Up = State;
                    if (SignalName == "To_EQ_Low")
                        EQ.To_EQ_Low = State;
                    if (SignalName == "Cmd_Reserve_Up")
                        EQ.CMD_Reserve_Up = State;
                    if (SignalName == "Cmd_Reserve_Low")
                        EQ.CMD_Reserve_Low = State;
                    if (SignalName == "VALID")
                        EQ.HS_AGV_VALID = State;
                    if (SignalName == "TR_REQ")
                        EQ.HS_AGV_TR_REQ = State;
                    if (SignalName == "BUSY")
                        EQ.HS_AGV_BUSY = State;
                    if (SignalName == "READY")
                        EQ.HS_AGV_READY = State;
                    if (SignalName == "COMPT")
                        EQ.HS_AGV_COMPT = State;
                    confirm = true;
                }
                else
                {
                    message = $"{errmsg}";
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

    }

}
