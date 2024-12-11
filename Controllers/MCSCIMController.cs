using AGVSystem.Hubs;
using AGVSystem.Models.EQDevices;
using AGVSystem.Models.Sys;
using AGVSystem.Service;
using AGVSystem.Service.MCS;
using AGVSystemCommonNet6;
using AGVSystemCommonNet6.AGVDispatch;
using AGVSystemCommonNet6.Alarm;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.Microservices.MCS;
using AGVSystemCommonNet6.Microservices.ResponseModel;
using AGVSystemCommonNet6.Notify;
using AGVSystemCommonNet6.Vehicle_Control.VCS_ALARM;
using EquipmentManagment.Manager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using NLog;
using static AGVSystem.Service.MCS.MCSService;
using static AGVSystemCommonNet6.Microservices.MCS.MCSCIMService;
using static SQLite.SQLite3;

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

        Logger logger => MCSService.logger;
        readonly MCSService mcsService;
        readonly AGVSDbContext dbContext;
        IHubContext<FrontEndDataHub> fronendMsgHub;
        public MCSCIMController(MCSService mcsService, IHubContext<FrontEndDataHub> fronendMsgHub, AGVSDbContext dbContext)
        {
            this.dbContext = dbContext;
            this.mcsService = mcsService;
            this.fronendMsgHub = fronendMsgHub;
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
            return Ok(response);
        }

        [HttpPost("TransportCommand")]
        public async Task<clsResult> TransportCommand([FromBody] clsTransportCommandDto transportCommand)
        {
            clsResult result = new clsResult();

            try
            {
                if (transportCommand.simulation)
                    transportCommand.commandID = $"MSIM{DateTime.Now.ToString("yyyyMMddHHmmssff")}";
                await SendMCSMessage($"[MCS命令-{transportCommand.commandID}] {transportCommand.source} to {transportCommand.dest}");
                await mcsService.HandleTransportCommand(transportCommand);
                result.Confirmed = true;
                result.ResultCode = 0;
            }
            catch (HasIDbutNoCargoException ex)
            {
                result.Confirmed = false;
                result.ResultCode = 6;
                result.Message = ex.GetType().Name;
                SendMCSMessage($"[MCS命令-{transportCommand.commandID} 已被系統拒絕] Result Code = 6 ({ex.Message})");

            }
            catch (AddOrderFailException ex)
            {
                result.Confirmed = false;
                result.ResultCode = (int)ex.alarmCode;
                result.Message = ex.Message;
                clsTaskDto order = ex.order;
                SendMCSMessage($"[MCS命令-{transportCommand.commandID} 已被系統拒絕] Result Code = {(int)ex.alarmCode} ({ex.Message})");
                AddFailOrderToDatabase(ex, order);

            }
            catch (ZoneIsFullException ex)
            {
                result.Confirmed = false;
                result.ResultCode = 2;
                result.Message = ex.Message;
                SendMCSMessage($"[MCS命令-{transportCommand.commandID} 已被系統拒絕] Result Code = 2 ,({ex.Message})");
            }
            catch (Exception ex)
            {
                result.Confirmed = false;
                result.ResultCode = 2;
                result.Message = ex.Message;
                SendMCSMessage($"[MCS命令-{transportCommand.commandID} 已被系統拒絕] Result Code = 2 ,({ex.Message})");
            }
            return result;
        }

        private async Task AddFailOrderToDatabase(AddOrderFailException ex, clsTaskDto order)
        {
            try
            {
                clsTaskDto orderExist = dbContext.Tasks.FirstOrDefault(t => t.TaskName == order.TaskName);
                if (orderExist == null)
                {

                    string failDesc = string.Empty;//[40]AGV衝撞(前後膠條)(Bumper)

                    if (AlarmManagerCenter.AlarmCodes.TryGetValue(ex.alarmCode, out var alarmModel))
                        failDesc = $"[{(int)ex.alarmCode}]{alarmModel.Description}";
                    else
                        failDesc = $"[{(int)ex.alarmCode}]{ex.Message}";

                    order.State = AGVSystemCommonNet6.AGVDispatch.Messages.TASK_RUN_STATUS.FAILURE;
                    order.StartTime = order.FinishTime = DateTime.Now;
                    order.FailureReason = failDesc;
                    dbContext.Tasks.Add(order);
                    await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception innderEx)
            {
                logger.Error(innderEx);
            }
        }

        /// <summary>
        /// Cancel用於MCS想要取消在駐列中的任務
        /// </summary>
        /// <returns></returns>
        [HttpPost("TransportCancel")]
        public async Task<clsResult> TranportCancel(string commandID)
        {
            return await mcsService.HandleTransportCancelAsync(commandID);
        }

        /// <summary>
        /// Abort用於MCS想要中止運行中的任務
        /// </summary>
        /// <returns></returns>
        [HttpPost("TransportAbort")]
        public async Task<clsResult> TransportAbort(string commandID)
        {
            MCSCIMService.TransferAbortInitiatedReport(new TransportCommandDto()
            {

            });
            return new clsResult();
        }

        [HttpGet("EnhancedActiveZones")]
        public async Task<clsResult> EnhancedActiveZones()
        {
            List<ZoneData> zoneDataList = StaEQPManagager.RacksList.Select(rack => EQDeviceEventsHandler.GenerateZoneData(rack)).ToList();
            zoneDataList = zoneDataList.DistinctBy(zone => zone.ZoneName).ToList();
            return new clsResult()
            {
                Confirmed = true,
                Message = zoneDataList.ToJson(Formatting.None)
            };
        }

        private async Task SendMCSMessage(string message, bool isException = false)
        {
            logger.Trace(message);
            if (isException)
                NotifyServiceHelper.WARNING(message);
            else
                NotifyServiceHelper.INFO(message);
            await fronendMsgHub.Clients.All.SendAsync("MCSMessage", message);
        }
    }
}
