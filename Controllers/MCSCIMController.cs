using AGVSystem.Hubs;
using AGVSystem.Models.EQDevices;
using AGVSystem.Models.Sys;
using AGVSystem.Service;
using AGVSystem.Service.MCS;
using AGVSystemCommonNet6;
using AGVSystemCommonNet6.AGVDispatch;
using AGVSystemCommonNet6.Alarm;
using AGVSystemCommonNet6.Alarm.SECS_Alarm_Code.Enums;
using AGVSystemCommonNet6.Configuration;
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
        readonly SECSConfigsService secsConfigService;
        public MCSCIMController(MCSService mcsService, IHubContext<FrontEndDataHub> fronendMsgHub, AGVSDbContext dbContext, SECSConfigsService secsConfigService)
        {
            this.dbContext = dbContext;
            this.mcsService = mcsService;
            this.fronendMsgHub = fronendMsgHub;
            this.secsConfigService = secsConfigService;
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
                await mcsService.HandleTransportCommand(transportCommand);
                result.Confirmed = true;
                result.ResultCode = 0;
                await SendMCSMessage($"已接收 MCS命令-{transportCommand.commandID},Carrier ID= {transportCommand.carrierID},From {transportCommand.source} To {transportCommand.dest}");
            }
            catch (HasIDbutNoCargoException ex)
            {
                result.Confirmed = false;
                result.ResultCode = 6;
                result.Message = ex.GetType().Name;
                SendMCSMessage($"[MCS命令-{transportCommand.commandID} 已被系統拒絕] Result Code = 6 ({ex.Message})", true);

            }
            catch (SourceOrDestineNotFoundException ex)
            {
                result.Confirmed = false;
                result.ResultCode = 2;
                result.Message = ex.Message;
                clsTaskDto order = ex.order;
                SendMCSMessage($"[MCS命令-{transportCommand.commandID} 已被系統拒絕] Result Code = 2 ,({ex.Message})", true);
                AddFailOrderToDatabase(ex, order);

            }
            catch (AddOrderFailException ex)
            {
                result.Confirmed = false;
                result.ResultCode = ex.alarmCodeMap.Code;
                result.Message = ex.Message;
                clsTaskDto order = ex.order;
                SendMCSMessage($"[MCS命令-{transportCommand.commandID} 已被系統拒絕]:{ex.Message} , AGVS Result Code : {ex.alarmCode} | HCACK Return Code :{ex.alarmCodeMap.Code} ({ex.alarmCodeMap.Description})", true);
                AddFailOrderToDatabase(ex, order);

            }
            catch (ZoneIsFullException ex)
            {
                result.Confirmed = false;
                result.ResultCode = secsConfigService.alarmConfiguration.Version == AGVSystemCommonNet6.Microservices.MCSCIM.SECSAlarmConfiguration.ALARM_TABLE_VERSION.GPM ?
                    (byte)HCACK_RETURN_CODE_GPM.Cannot_Find_Seat_For_The_Carrier_In_Rack : (byte)HCACK_RETURN_CODE_YELLOW.Cannot_Find_Seat_For_The_Carrier_In_Rack;
                result.Message = ex.Message;
                SendMCSMessage($"[MCS命令-{transportCommand.commandID} 已被系統拒絕] Result Code = {result.ResultCode} ,({ex.Message})", true);
            }
            catch (PortDisabledException ex)
            {
                result.Confirmed = false;
                result.ResultCode = ex.isSource ? (byte)HCACK_RETURN_CODE_YELLOW.Rack_Source_Port_Position_Is_Disable : (byte)HCACK_RETURN_CODE_YELLOW.Rack_Destination_Port_Position_Is_Disable;
                result.Message = ex.Message;
                SendMCSMessage($"[MCS命令-{transportCommand.commandID} 已被系統拒絕] Result Code = {result.ResultCode} ,({ex.Message})", true);
            }
            catch (Exception ex)
            {
                result.Confirmed = false;
                result.ResultCode = 2;
                result.Message = ex.Message;
                SendMCSMessage($"[MCS命令-{transportCommand.commandID} 已被系統拒絕] Result Code = 2 ,({ex.Message})", true);
            }
            return result;
        }
        private async Task AddFailOrderToDatabase(SourceOrDestineNotFoundException ex, clsTaskDto order)
        {
            try
            {
                clsTaskDto orderExist = dbContext.Tasks.FirstOrDefault(t => t.TaskName == order.TaskName);
                if (orderExist == null)
                {
                    string failDesc = $"{ex.Message}";
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
            await fronendMsgHub.Clients.All.SendAsync("MCSMessage", new { time = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), message = message, type = isException ? "error" : "success" });
        }
    }
}
