﻿using AGVSystem.Models.Map;
using AGVSystem.Models.TaskAllocation;
using AGVSystem.Models.TaskAllocation.HotRun;
using AGVSystem.Models.WebsocketMiddleware;
using AGVSystem.TaskManagers;
using AGVSystemCommonNet6;
using AGVSystemCommonNet6.AGVDispatch;
using AGVSystemCommonNet6.AGVDispatch.Messages;
using AGVSystemCommonNet6.Alarm;
using AGVSystemCommonNet6.Configuration;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.Log;
using AGVSystemCommonNet6.MAP;
using AGVSystemCommonNet6.Microservices.ResponseModel;
using AGVSystemCommonNet6.User;
using AGVSystemCommonNet6.Vehicle_Control.VCS_ALARM;
using EquipmentManagment.MainEquipment;
using EquipmentManagment.Manager;
using EquipmentManagment.WIP;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        public TaskController(AGVSDbContext content)
        {
            this._TaskDBContent = content;
        }

        [HttpGet("Allocation")]
        public async Task<IActionResult> Test()
        {
            if (!UserValidation())
            {
                return Unauthorized();
            }

            return Ok();
        }



        [HttpGet("Cancel")]
        public async Task<IActionResult> Cancel(string task_name)
        {
            if (!UserValidation())
            {
                return Unauthorized();
            }
            LOG.TRACE($"User try cancle Task-{task_name}");

            bool canceled = await TaskManager.Cancel(task_name, $"User manual canceled");
            LOG.TRACE($"User try cancle Task-{task_name}---{canceled}");
            return Ok(canceled);
        }

        [HttpPost("move")]
        public async Task<IActionResult> MoveTask([FromBody] clsTaskDto taskData, string user = "")
        {
            if (!UserValidation())
            {
                return Unauthorized();
            }
            return Ok(await AddTask(taskData, user));
        }
        [HttpPost("measure")]
        public async Task<IActionResult> MeasureTask([FromBody] clsTaskDto taskData, string user = "")
        {
            if (!UserValidation())
            {
                return Unauthorized();
            }
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
        public async Task<IActionResult> LoadTask([FromBody] clsTaskDto taskData, string user = "")
        {
            if (!UserValidation())
            {
                return Unauthorized();
            }
            return Ok(await AddTask(taskData, user));
        }
        [HttpPost("unload")]
        public async Task<IActionResult> UnloadTask([FromBody] clsTaskDto taskData, string user = "")
        {
            if (!UserValidation())
            {
                return Unauthorized();
            }
            return Ok(await AddTask(taskData, user));
        }
        [HttpPost("carry")]
        public async Task<IActionResult> CarryTask([FromBody] clsTaskDto taskData, string user = "")
        {
            if (!UserValidation())
            {
                return Unauthorized();
            }
            return Ok(await AddTask(taskData, user));
        }
        [HttpPost("charge")]
        public async Task<IActionResult> ChargeTask([FromBody] clsTaskDto taskData, string user = "")
        {
            if (!UserValidation())
            {
                return Unauthorized();
            }
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
        public async Task<IActionResult> CancelChargeTask(string agv_name)
        {
            if (!UserValidation())
            {
                return Unauthorized();
            }
            var result = await TaskManager.CancelChargeTaskByAGVAsync(agv_name);
            return Ok(new { confirm = result.confirm, message = result.message });
        }


        [HttpPost("ExangeBattery")]
        public async Task<IActionResult> ExangeBattery([FromBody] clsTaskDto taskData, string user = "")
        {
            if (!UserValidation())
            {
                return Unauthorized();
            }
            return Ok(await AddTask(taskData, user));
        }
        [HttpPost("park")]
        public async Task<IActionResult> ParkTask([FromBody] clsTaskDto taskData, string user = "")
        {
            if (!UserValidation())
            {
                return Unauthorized();
            }
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
                return Ok(new clsAGVSTaskReportResponse() { confirm = false, AlarmCode = ALARMS.Station_Disabled, message = "站點未啟用，無法指派任務" });

            if (action == ACTION_TYPE.Load && (MapPoint.StationType == MapPoint.STATION_TYPE.Buffer_EQ || MapPoint.StationType == MapPoint.STATION_TYPE.Buffer) && slot == -2)
            {
                clsPortOfRack port = EQTransferTaskManager.get_empyt_port_of_rack(tag);
                return Ok(new clsAGVSTaskReportResponse() { confirm = true, message = $"Get empty port OK", ReturnObj = port.Layer });
            }

            (bool confirm, ALARMS alarm_code, string message, object obj, Type objtype) result = EQTransferTaskManager.CheckLoadUnloadStation(tag, slot, action);
            if (result.confirm == false)
                return Ok(new clsAGVSTaskReportResponse() { confirm = false, message = $"{result.message}" });
            else
            {
                if (result.objtype == typeof(clsEQ))
                {
                    clsEQ mainEQ = (clsEQ)result.obj;
                    try
                    {
                        mainEQ.ReserveUp();
                        mainEQ.ToEQUp();
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

            //(bool existDevice, clsEQ mainEQ, clsRack rack) result = TryGetEndDevice(tag);

            //if (!result.existDevice)
            //    return Ok(new clsAGVSTaskReportResponse() { confirm = false, message = $"[LoadUnloadTaskStart] 找不到Tag為{tag}的設備" });
            //else
            //{
            //    if (result.mainEQ != null)
            //    { // TODO 設備異常
            //        if (action == ACTION_TYPE.Unload)
            //        {
            //            if (result.mainEQ.IsConnected == false)
            //                return Ok(new clsAGVSTaskReportResponse() { confirm = false, alarmcode_int = 1000, message = $"Unload_Request={result.mainEQ.Unload_Request} cannot Unload" });//EQ_Disconnect
            //            if (result.mainEQ.Unload_Request == false)
            //                return Ok(new clsAGVSTaskReportResponse() { confirm = false, alarmcode_int = 1015, message = $"Unload_Request={result.mainEQ.Unload_Request} cannot Unload" });//EQ_UNLOAD_REQUEST_IS_NOT_ON
            //            if (result.mainEQ.Port_Exist == false)
            //                return Ok(new clsAGVSTaskReportResponse() { confirm = false, alarmcode_int = 1076, message = $"Port_Exist={result.mainEQ.Port_Exist} cannot Unload" });//EQ_UNLOAD_REQUEST_ON_BUT_NO_CARGO
            //            if (result.mainEQ.Up_Pose == false)
            //                return Ok(new clsAGVSTaskReportResponse() { confirm = false, alarmcode_int = 1076, message = $"Up_Pose={result.mainEQ.Up_Pose} cannot Unload" });//EQ_UNLOAD_REQUEST_ON_BUT_NO_CARGO
            //        }
            //        else if (action == ACTION_TYPE.Load)
            //        {
            //            if (result.mainEQ.IsConnected == false)
            //                return Ok(new clsAGVSTaskReportResponse() { confirm = false, alarmcode_int = 1068, message = $"IsConnected={result.mainEQ.IsConnected} cannot Unload" });//EQ_Disconnect
            //            if (result.mainEQ.Load_Request == false)
            //                return Ok(new clsAGVSTaskReportResponse() { confirm = false, alarmcode_int = 1014, message = $"Load_Request={result.mainEQ.Load_Request} cannot Unload" });//EQ_LOAD_REQUEST_IS_NOT_ON
            //            if (result.mainEQ.Port_Exist == true)
            //                return Ok(new clsAGVSTaskReportResponse() { confirm = false, alarmcode_int = 1075, message = $"Port_Exist={result.mainEQ.Port_Exist} cannot Unload" });//EQ_LOAD_REQUEST_ON_BUT_HAS_CARGO
            //            if (result.mainEQ.Down_Pose == false)
            //                return Ok(new clsAGVSTaskReportResponse() { confirm = false, alarmcode_int = 1014, message = $"Down_Pose={result.mainEQ.Down_Pose} cannot Unload" });//EQ_LOAD_REQUEST_IS_NOT_ON
            //        }
            //    }
            //    else if (result.rack != null)
            //    {
            //        // 不交握do noting
            //    }
            //    else
            //    {
            //        return Ok(new clsAGVSTaskReportResponse() { confirm = false, message = $"tag={tag}, mainEQ=null, Rack=null, cannot Unload" });
            //    }
            //}

            //if (result.mainEQ != null)
            //{
            //    try
            //    {
            //        result.mainEQ.ReserveUp();
            //        result.mainEQ.ToEQUp();
            //    }
            //    catch (Exception ex)
            //    {
            //        return Ok(new clsAGVSTaskReportResponse() { confirm = false, message = $"{result.mainEQ.EQName} ToEQUp DO ON的過程中發生錯誤:{ex.Message}" });
            //    }
            //    LOG.INFO($"Get AGV LD.ULD Task Start At Tag {tag}-Action={action}. TO Eq Up DO ON", color: ConsoleColor.Green);
            //    return Ok(new clsAGVSTaskReportResponse() { confirm = true, message = $"{result.mainEQ.EQName} ToEQUp DO ON" });
            //}
            //else
            //    return Ok(new clsAGVSTaskReportResponse() { confirm = true, message = $"{action} at {result.rack.EQName} Start" });
        }

        [HttpGet("StartTransferCargoReport")]
        public async Task<clsAGVSTaskReportResponse> StartTransferCargoReport(string AGVName, int SourceTag, int DestineTag)
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
                sourceEQ = StaEQPManagager.MainEQList.FirstOrDefault(eq => eq.EndPointOptions.TagID == SourceTag);
                if (sourceEQ == null)
                {
                    return new clsAGVSTaskReportResponse() { confirm = false, message = $"[StartTransferCargoReport] 找不到Tag為{SourceTag}的起點設備" };

                }
                else
                {
                    RACK_CONTENT_STATE rack_content_state = StaEQPManagager.CargoStartTransferToDestineHandler(sourceEQ, destineEQ);
                    if (rack_content_state == RACK_CONTENT_STATE.UNKNOWN)
                    {
                        return new clsAGVSTaskReportResponse() { confirm = false, message = $"Task Abort_起點設備RACK空框/實框狀態未知" };
                    }
                    else
                    {
                        return new clsAGVSTaskReportResponse { confirm = true };
                    }
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
        public async Task<clsAGVSTaskReportResponse> LDULDOrderStart(int from, int FromSlot, int to, int ToSlot, ACTION_TYPE action)
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
                    clsAGVSTaskReportResponse result_from = ((OkObjectResult)await LoadUnloadTaskStart(from, FromSlot, ACTION_TYPE.Unload)).Value as clsAGVSTaskReportResponse;
                    if (result_from.confirm == false)
                        return result_from;

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
            return new { confirm = result.confirm, alarm_code = result.alarm_code, message = result.message };
        }
        private bool UserValidation()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            if (identity != null)
            {
                IEnumerable<Claim> claims = identity.Claims;
                var userId = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                var userRole = claims.FirstOrDefault(c => c.Type == "Role")?.Value;

                if (userRole == ERole.VISITOR.ToString())
                    return false;

                return true;
            }
            else
                return false;
        }

    }
}
