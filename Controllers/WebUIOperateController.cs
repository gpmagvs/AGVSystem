using AGVSystem.ViewModel;
using AGVSystem.ViewModel.WebOperate;
using AGVSystemCommonNet6;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NLog;

namespace AGVSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebUIOperateController : ControllerBase
    {

        Logger logger = LogManager.GetLogger("WebOperationLogs");

        [HttpPost("MapOperate")]
        public async Task MapOperate([FromBody] MapOperateLogViewModel model)
        {
            logger.Trace($"User click Map feature,{model.ToJson(Newtonsoft.Json.Formatting.None)}");
        }

        [HttpPost("ElementClicked")]
        public async Task ElementClicked([FromBody] ElementClickedLogViewModel model)
        {
            logger.Trace($"User clicked element,{model.ToJson(Newtonsoft.Json.Formatting.None)}");
        }
    }
}
