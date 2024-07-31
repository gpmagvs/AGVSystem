using AGVSystemCommonNet6;
using EquipmentManagment.Manager;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AGVSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChargerController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            List<EquipmentManagment.ChargeStation.clsChargerData> chargerDatas = StaEQPManagager.ChargeStations.Select(charger => charger.Datas)
                                                           .ToList();
            return Ok(chargerDatas);
        }

        [HttpGet("Query")]
        public async Task<IActionResult> Query(string chargerName)
        {
            EquipmentManagment.ChargeStation.clsChargeStation? charger = StaEQPManagager.ChargeStations.Where(charger => charger.EndPointOptions.Name == chargerName).FirstOrDefault();
            if (charger == null)
                return BadRequest($"{chargerName} not exist");
            if (charger.EndPointOptions.IsEmulation)
            {
                EquipmentManagment.ChargeStation.clsChargerData fakData = charger.Datas.Clone();
                fakData.Vin = 219.9;
                fakData.Iout = 30.2;
                fakData.Vout = 24.2;
                fakData.Temperature = 25;
                fakData.ErrorCodes = new List<EquipmentManagment.ChargeStation.clsChargeStation.ERROR_CODE> {
                     EquipmentManagment.ChargeStation.clsChargeStation.ERROR_CODE.Temp_OT_Fault
                };
                fakData.UpdateTime = DateTime.Now;
                return Ok(fakData);
            }
            return Ok(charger.Datas);
        }

        [HttpGet("GetChargerNames")]
        public async Task<IActionResult> GetNames()
        {
            List<string> chargerNames = StaEQPManagager.ChargeStations.Select(charger => charger.EndPointOptions.Name)
                                                           .ToList();
            return Ok(chargerNames);

        }
    }
}
