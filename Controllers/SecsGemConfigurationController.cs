using AGVSystemCommonNet6.Configuration;
using AGVSystemCommonNet6.Microservices.MCSCIM;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static AGVSystemCommonNet6.Microservices.MCSCIM.TransferReportConfiguration;

namespace AGVSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SecsGemConfigurationController : ControllerBase
    {

        private readonly SECSConfigsService service;
        public SecsGemConfigurationController(SECSConfigsService service)
        {
            this.service = service;
        }

        [HttpGet]
        public async Task<SECSConfigsService> Get()
        {
            return service;
        }
        [HttpPost("saveReturnCodeSetting")]
        public async Task<object> SaveReturnCodeSetting([FromBody] clsReturnCodes returnCodeSettings)
        {
            try
            {
                service.UpdateReturnCodes(returnCodeSettings);
                return new { confirm = true, message = "" };
            }
            catch (Exception ex)
            {
                return new { confirm = false, message = ex.Message };
            }
        }

        [HttpPost("SaveResultCodeSetting")]
        public async Task<object> SaveResultCodeSetting([FromBody] clsSaveReturnCodeRequest resultCodeSettings)
        {
            try
            {
                service.UpdateResultCodes(resultCodeSettings.transferCompletedResultCodes);
                return new { confirm = true, message = "" };
            }
            catch (Exception ex)
            {
                return new { confirm = false, message = ex.Message };
            }
        }
        [HttpPost("saveSECSGemSetting")]
        public async Task<object> SaveSECSGemSetting(SECSConfiguration returnSECSGemSettings)
        {
            try
            {
                AGVSConfigulator.SysConfigs.SECSGem = returnSECSGemSettings;
                AGVSConfigulator.Save(AGVSConfigulator.SysConfigs);
                //service.UpdateSECSGemConfigs(returnSECSGemSettings);
                return new { confirm = true, message = "" };
            }
            catch (Exception ex)
            {
                return new { confirm = false, message = ex.Message };
            }
        }

        public class clsSaveReturnCodeRequest
        {
            public clsResultCodes transferCompletedResultCodes { get; set; } = new clsResultCodes();
            public clsReturnCodes taskreplyReturnCodes { get; set; } = new clsReturnCodes();
        }

    }
}
