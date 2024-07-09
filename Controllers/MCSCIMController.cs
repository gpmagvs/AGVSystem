using AGVSystem.Models.Sys;
using AGVSystemCommonNet6.AGVDispatch;
using AGVSystemCommonNet6.Microservices.MCS;
using AGVSystemCommonNet6.Microservices.ResponseModel;
using AGVSystemCommonNet6.Vehicle_Control.VCS_ALARM;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AGVSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public partial class MCSCIMController : Controller
    {
        [HttpPost("TaskReporter")]
        public async Task<IActionResult> TaskReporter(object data)
        {
            clsAGVSTaskReportResponse response = new clsAGVSTaskReportResponse() { confirm = false, AlarmCode = AGVSystemCommonNet6.Alarm.ALARMS.SYSTEM_ERROR, message = "System Error" };
            if (SystemModes.HostOperMode == AGVSystemCommonNet6.AGVDispatch.RunMode.HOST_OPER_MODE.REMOTE)
            {
                (clsTaskDto task, int stat) obj_data = JsonConvert.DeserializeObject<(clsTaskDto, int)>(data.ToString());
                (bool confirm, string message) v = await MCSCIMService.TaskReporter(obj_data);
                response.confirm = v.confirm;
                response.message = v.message;
            }
            else
            {
                response.confirm = true;
                response.AlarmCode = AGVSystemCommonNet6.Alarm.ALARMS.NONE;
                response.message = $"SystemModes.HostOperMode={SystemModes.HostOperMode}";
            }
            return Ok(response);
        }
    }
}
