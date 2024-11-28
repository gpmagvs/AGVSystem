﻿using AGVSystem.Models.Sys;
using AGVSystem.Service;
using AGVSystemCommonNet6;
using AGVSystemCommonNet6.AGVDispatch;
using AGVSystemCommonNet6.Alarm;
using AGVSystemCommonNet6.Microservices.MCS;
using AGVSystemCommonNet6.Microservices.ResponseModel;
using AGVSystemCommonNet6.Vehicle_Control.VCS_ALARM;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using static AGVSystem.Service.MCSService;

namespace AGVSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public partial class MCSCIMController : Controller
    {
        public class clsResult
        {
            public bool Confirmed { get; set; } = false;
            public int ResultCode { get; set; } = 1;
            public string Message { get; set; } = "";
        }

        readonly MCSService mcsService;
        public MCSCIMController(MCSService mcsService)
        {
            this.mcsService = mcsService;
        }

        [HttpPost("TaskReporter")]
        public async Task<IActionResult> TaskReporter(object data)
        {
            clsAGVSTaskReportResponse response = new clsAGVSTaskReportResponse() { confirm = false, AlarmCode = AGVSystemCommonNet6.Alarm.ALARMS.SYSTEM_ERROR, message = "System Error" };
            if (false) // 測試時不卡條件就true
            {
                (clsTaskDto task, int stat) obj_data = JsonConvert.DeserializeObject<(clsTaskDto, int)>(data.ToString());
                (bool confirm, string message) v = await MCSCIMService.TaskReporter(obj_data);
                response.confirm = v.confirm;
                response.message = v.message;
            }
            else
            {
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
            }
            return Ok(response);
        }
        [HttpPost("AlarmReporterSwitch")]
        public async Task<IActionResult> AlarmReporterSwitch(bool truetoenable)
        {
            clsAGVSTaskReportResponse response = new clsAGVSTaskReportResponse() { confirm = true, AlarmCode = AGVSystemCommonNet6.Alarm.ALARMS.NONE, message = "OK" };
            AlarmManagerCenter.IsReportAlarmToHostON = truetoenable;
            return Ok(response);
        }

        [HttpPost("TransportCommand")]
        public async Task<clsResult> TransportCommand([FromBody] clsTransportCommandDto transportCommand)
        {
            mcsService.HandleTransportCommand(transportCommand);
            return new() { Message = transportCommand.ToJson() };
        }

    }
}
