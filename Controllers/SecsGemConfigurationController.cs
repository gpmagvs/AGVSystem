using AGVSystemCommonNet6.Configuration;
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
        public async Task<object> SaveReturnCodeSetting([FromBody] clsSaveReturnCodeRequest returnCodeSettings)
        {
            try
            {
                service.UpdateReturnCodes(returnCodeSettings.transferCompletedResultCodes);
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
        }

    }
}
