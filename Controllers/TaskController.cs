using AGVSystem.Models.Map;
using AGVSystem.Models.TaskAllocation;
using AGVSystem.Models.TaskAllocation.HotRun;
using AGVSystem.Service;
using AGVSystem.TaskManagers;
using AGVSystemCommonNet6;
using AGVSystemCommonNet6.AGVDispatch;
using AGVSystemCommonNet6.AGVDispatch.Messages;
using AGVSystemCommonNet6.Alarm;
using AGVSystemCommonNet6.Configuration;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.Log;
using AGVSystemCommonNet6.MAP;
using AGVSystemCommonNet6.Microservices.MCS;
using AGVSystemCommonNet6.Microservices.ResponseModel;
using AGVSystemCommonNet6.User;
using AGVSystemCommonNet6.Vehicle_Control.VCS_ALARM;
using EquipmentManagment.MainEquipment;
using EquipmentManagment.Manager;
using EquipmentManagment.WIP;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using NuGet.Configuration;
using NuGet.Protocol;
using RosSharp.RosBridgeClient.MessageTypes.ObjectRecognition;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static SQLite.SQLite3;

namespace AGVSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public partial class TaskController : ControllerBase
    {
        private AGVSDbContext _TaskDBContent;

        private UserValidationService UserValidation { get; }

        public TaskController(AGVSDbContext content, UserValidationService userValidation)
        {
            this._TaskDBContent = content;
            this.UserValidation = userValidation;
        }

        [HttpGet("Allocation")]
        [Authorize]
        public async Task<IActionResult> Test()
        {
            return Ok();
        }



        [HttpGet("Cancel")]
        [Authorize]
        public async Task<IActionResult> Cancel(string task_name)
        {
            LOG.TRACE($"User try cancle Task-{task_name}");
            bool canceled = await TaskManager.Cancel(task_name, $"User manual canceled");
            LOG.TRACE($"User try cancle Task-{task_name}---{canceled}");
            return Ok(canceled);
        }

        [HttpPost("move")]
        [Authorize]
        public async Task<IActionResult> MoveTask([FromBody] clsTaskDto taskData, string user = "")
        {
            return Ok(await AddTask(taskData, user));
        }
        [HttpPost("measure")]
        [Authorize]
        public async Task<IActionResult> MeasureTask([FromBody] clsTaskDto taskData, string user = "")
        {
            Map map = MapManager.LoadMapFromFile();
            if (map.Bays.TryGetValue(taskData.To_Station, out Bay bay))
            {
                taskData.To_Slot = string.Join(",", bay.Points);

                return Ok(await AddTask(taskData, user));
            }
            else
                return Ok(new { confirm = false, message = $"Bay - {taskData.To_Station} not found" });
        }
        [HttpPost("load")]
        [Authorize]
        public async Task<IActionResult> LoadTask([FromBody] clsTaskDto taskData, string user = "")
        {
            return Ok(await AddTask(taskData, user));
        }
        [HttpPost("unload")]
        [Authorize]
        public async Task<IActionResult> UnloadTask([FromBody] clsTaskDto taskData, string user = "")
        {
            return Ok(await AddTask(taskData, user));
        }
        [HttpPost("carry")]
        [Authorize]
        public async Task<IActionResult> CarryTask([FromBody] clsTaskDto taskData, string user = "")
        {
            return Ok(await AddTask(taskData, user));
        }
        [HttpPost("charge")]
        [Authorize]
        public async Task<IActionResult> ChargeTask([FromBody] clsTaskDto taskData, string user = "")
        {
            try
            {

                (bool confirm, int alarm_code, string message) check_result = TaskManager.CheckChargeTask(taskData.DesignatedAGVName, taskData.To_Station_Tag);
                if (!check_result.confirm)
                    return Ok(new { confirm = check_result.confirm, alarm_code = check_result.alarm_code, message = check_result.message });
                return Ok(await AddTask(taskData, user));
            }
            catch (Exception ex)
            {
                return Ok(new { confirm = false, alarm_code = ALARMS.SYSTEM_ERROR, message = "[Internal Error] " + ex.Message });
            }
        }

        [HttpGet("CancelChargeTask")]
        [Authorize]
        public async Task<IActionResult> CancelChargeTask(string agv_name)
        {
            var result = await TaskManager.CancelChargeTaskByAGVAsync(agv_name);
            return Ok(new { confirm = result.confirm, message = result.message });
        }


        [HttpPost("ExangeBattery")]
        [Authorize]
        public async Task<IActionResult> ExangeBattery([FromBody] clsTaskDto taskData, string user = "")
        {
            taskData.Priority = 100000;
            return Ok(await AddTask(taskData, user));
        }
        [HttpPost("park")]
        [Authorize]
        public async Task<IActionResult> ParkTask([FromBody] clsTaskDto taskData, string user = "")
        {
            return Ok(await AddTask(taskData, user));
        }


        [HttpGet("LoadUnloadTaskStart")]
        public async Task<IActionResult> LoadUnloadTaskStart(int tag, int slot, ACTION_TYPE action)
        {

            if (action != ACTION_TYPE.Load && action != ACTION_TYPE.Unload)
                return Ok(new clsAGVSTaskReportResponse() { confirm = false, message = "Action should equal Load or Unlaod" });


            AGVSystemCommonNet6.MAP.MapPoint MapPoint = AGVSMapManager.GetMapPointByTag(tag);
            if (MapPoint == null)
                return Ok(new clsAGVSTaskReportResponse() { confirm = false, AlarmCode = ALARMS.EQ_TAG_NOT_EXIST_IN_CURRENT_MAP, message = $"站點TAG-{tag} 不存在於當前地圖" });

            if (!MapPoint.Enable)
                return Ok(new clsAGVSTaskReportResponse() { confirm = false, AlarmCode = ALARMS.Station_Disabled, message = $"站點TAG-{tag} 未啟用，無法指派任務" });

            if (action == ACTION_TYPE.Load && (MapPoint.StationType == MapPoint.STATION_TYPE.Buffer_EQ || MapPoint.StationType == MapPoint.STATION_TYPE.Buffer) && slot == -2)
            {
                clsPortOfRack port = EQTransferTaskManager.get_empyt_port_of_rack(tag);
                return Ok(new clsAGVSTaskReportResponse() { confirm = true, message = $"Get empty port OK", ReturnObj = port.Layer });
            }

            (bool confirm, ALARMS alarm_code, string message, object obj, Type objtype) result = EQTransferTaskManager.CheckLoadUnloadStation(tag, slot, action, bypasseqandrackckeck: false);
            if (result.confirm == false)
            {
                return Ok(new clsAGVSTaskReportResponse() { confirm = false, message = $"{result.message}", AlarmCode = result.alarm_code });
            }
            else
            {
                if (result.objtype == typeof(clsEQ))
                {
                    clsEQ mainEQ = (clsEQ)result.obj;
                    try
                    {
                        mainEQ.ReserveUp();
                        mainEQ.ToEQUp();
                        if (mainEQ.EndPointOptions.IsOneOfDualPorts)
                        {
                            bool isForkAGVOnlyPort = mainEQ.EndPointOptions.Accept_AGV_Type == EquipmentManagment.Device.Options.VEHICLE_TYPE.FORK;
                            bool isSubmarineAGVOnlyPort = mainEQ.EndPointOptions.Accept_AGV_Type == EquipmentManagment.Device.Options.VEHICLE_TYPE.SUBMERGED_SHIELD;
                            if (isForkAGVOnlyPort)
                                await GPMCIMService.ChangePortTypeOfEq(mainEQ.EndPointOptions.TagID, 0);
                            if (isSubmarineAGVOnlyPort)
                                await GPMCIMService.ChangePortTypeOfEq(mainEQ.EndPointOptions.TagID, 1);
                        }
                    }
                    catch (Exception ex)
                    {
                        return Ok(new clsAGVSTaskReportResponse() { confirm = false, message = $"{mainEQ.EQName} ToEQUp DO ON的過程中發生錯誤:{ex.Message}" });
                    }
                    LOG.INFO($"Get AGV LD.ULD Task Start At Tag {tag}-Action={action}. TO Eq Up DO ON", color: ConsoleColor.Green);
                    return Ok(new clsAGVSTaskReportResponse() { confirm = true, message = $"{mainEQ.EQName} ToEQUp DO ON" });
                }
                else if (result.objtype == typeof(clsPortOfRack))
                {
                    return Ok(new clsAGVSTaskReportResponse() { confirm = true, message = result.message });
                }
                else
                {
                    return Ok(new clsAGVSTaskReportResponse() { confirm = false, message = "NOT EQ or RACK" });
                }
            }
        }

        [HttpGet("StartTransferCargoReport")]
        public async Task<clsAGVSTaskReportResponse> StartTransferCargoReport(string AGVName, int SourceTag, int DestineTag, bool IsSourceAGV = false)
        {
            var _response = new clsAGVSTaskReportResponse();

            clsEQ? destineEQ = StaEQPManagager.MainEQList.FirstOrDefault(eq => eq.EndPointOptions.TagID == DestineTag);
            clsEQ sourceEQ = null;
            if (destineEQ == null)
            {

                return new clsAGVSTaskReportResponse() { confirm = false, message = $"[StartTransferCargoReport] 找不到Tag為{DestineTag}的終點設備" };
            }
            else
            {
                if (IsSourceAGV)
                {
                    return new clsAGVSTaskReportResponse { confirm = true };
                }
                sourceEQ = StaEQPManagager.MainEQList.FirstOrDefault(eq => eq.EndPointOptions.TagID == SourceTag);
                if (sourceEQ == null)
                {
                    return new clsAGVSTaskReportResponse() { confirm = false, message = $"[StartTransferCargoReport] 找不到Tag為{SourceTag}的起點設備" };

                }
                else if (sourceEQ.EndPointOptions.CheckRackContentStateIOSignal || destineEQ.EndPointOptions.CheckRackContentStateIOSignal)
                {

                    RACK_CONTENT_STATE rackContentStateOfSourceEQ = StaEQPManagager.CargoStartTransferToDestineHandler(sourceEQ, destineEQ);
                    if (rackContentStateOfSourceEQ == RACK_CONTENT_STATE.UNKNOWN)
                    {
                        return new clsAGVSTaskReportResponse() { confirm = false, AlarmCode = ALARMS.EQ_LOAD_REQ_BUT_RACK_FULL_OR_EMPTY_IS_UNKNOWN, message = $"Task Abort_起點設備RACK空框/實框狀態未知" };
                        //return new clsAGVSTaskReportResponse() { confirm = true, AlarmCode = ALARMS.EQ_LOAD_REQ_BUT_RACK_FULL_OR_EMPTY_IS_UNKNOWN, message = $"Task Abort_起點設備RACK空框/實框狀態未知" };
                    }
                    else
                    {
                        return new clsAGVSTaskReportResponse { confirm = true };
                    }
                }
                else
                {
                    return new clsAGVSTaskReportResponse { confirm = true };
                }
            }
        }

        [HttpGet("LoadUnloadTaskFinish")]
        public async Task<clsAGVSTaskReportResponse> LoadUnloadTaskFinish(int tag, ACTION_TYPE action)
        {
            if (action != ACTION_TYPE.Load && action != ACTION_TYPE.Unload)
                return new clsAGVSTaskReportResponse() { confirm = false, message = "Action should equal Load or Unlaod" };

            (bool existDevice, clsEQ mainEQ, clsRack rack) result = TryGetEndDevice(tag);
            if (!result.existDevice)
                return new clsAGVSTaskReportResponse() { confirm = false, message = $"[LoadUnloadTaskFinish] 找不到Tag為{tag}的設備" };
            if (result.mainEQ != null)
            {
                result.mainEQ.CancelToEQUpAndLow();
                result.mainEQ.CancelReserve();
                LOG.INFO($"Get AGV LD.ULD Task Finish At Tag {tag}-Action={action}. TO Eq DO ALL OFF", color: ConsoleColor.Green);
                return new clsAGVSTaskReportResponse() { confirm = true, message = $"{result.mainEQ.EQName} ToEQUp DO OFF" };
            }
            else
                return new clsAGVSTaskReportResponse() { confirm = true, message = $"{action} from {result.rack} Finish" };
        }
        [HttpGet("LDULDOrderStart")]
        public async Task<clsAGVSTaskReportResponse> LDULDOrderStart(int from, int FromSlot, int to, int ToSlot, ACTION_TYPE action, bool isSourceAGV)
        {
            try
            {

                if (action == ACTION_TYPE.Unload || action == ACTION_TYPE.LoadAndPark || action == ACTION_TYPE.Load)
                {
                    clsAGVSTaskReportResponse result = ((OkObjectResult)await LoadUnloadTaskStart(to, ToSlot, action)).Value as clsAGVSTaskReportResponse;
                    return result;
                }
                else if (action == ACTION_TYPE.Carry)
                {
                    if (!isSourceAGV)
                    {
                        clsAGVSTaskReportResponse result_from = ((OkObjectResult)await LoadUnloadTaskStart(from, FromSlot, ACTION_TYPE.Unload)).Value as clsAGVSTaskReportResponse;
                        if (result_from.confirm == false)
                            return result_from;
                    }

                    clsAGVSTaskReportResponse result_to = ((OkObjectResult)await LoadUnloadTaskStart(to, ToSlot, ACTION_TYPE.Load)).Value as clsAGVSTaskReportResponse;
                    return result_to;
                }
                else
                {
                    // LDULDOrder not accept other action type
                    return new clsAGVSTaskReportResponse() { confirm = false, message = $"LDULDOrder ACTION_TYPE can not be={action}" };
                }
            }
            catch (Exception ex)
            {
                return new clsAGVSTaskReportResponse() { confirm = false, message = $"{ex}" };
            }
        }

        [HttpPost("UpdateMaterialTransferStatus")]
        public async Task<IActionResult> UpdateMaterialTransferStatus(Models.TaskAllocation.clsMaterialInfoDto materialInfoDto, string User = "")
        {
            if (!UserValidation.UserValidation(HttpContext))
            {
                return Unauthorized();
            }
            return Ok();
        }
        private (bool existDevice, clsEQ mainEQ, clsRack rack) TryGetEndDevice(int tag)
        {
            var Eq = StaEQPManagager.MainEQList.FirstOrDefault(eq => eq.EndPointOptions.TagID == tag);
            var Rack = StaEQPManagager.RacksList.FirstOrDefault(eq => eq.EndPointOptions.TagID == tag);
            return (Eq != null || Rack != null, Eq, Rack);
        }

        private async Task<object> AddTask(clsTaskDto taskData, string user = "")
        {
            taskData.DispatcherName = user;
            var result = await TaskManager.AddTask(taskData, TaskManager.TASK_RECIEVE_SOURCE.MANUAL);
            bool showEmptyOrFullContentCheck = false;
            if (result.alarm_code == ALARMS.EQ_UNLOAD_REQ_BUT_RACK_FULL_OR_EMPTY_IS_UNKNOWN)
            {
                int eqTag = taskData.Action == ACTION_TYPE.Unload ? taskData.To_Station_Tag : taskData.From_Station_Tag;
                //MapPoint mapPoint = AGVSMapManager.GetMapPointByTag(eqTag);
                clsEQ eq = StaEQPManagager.GetEQByTag(eqTag);
                if (eq.EndPointOptions.IsFullEmptyUnloadAsVirtualInput)
                {
                    showEmptyOrFullContentCheck = true;
                }
            }
            return new { confirm = result.confirm, alarm_code = result.alarm_code, message = result.message, showEmptyOrFullContentCheck = showEmptyOrFullContentCheck };
        }


    }
}
