using AGVSystem.Models.Map;
using AGVSystem.Models.Sys;
using AGVSystem.Models.TaskAllocation.HotRun;
using AGVSystem.Service;
using AGVSystem.Service.EquipmentStatusCheckServices;
using AGVSystem.TaskManagers;
using AGVSystemCommonNet6;
using AGVSystemCommonNet6.AGVDispatch;
using AGVSystemCommonNet6.AGVDispatch.Messages;
using AGVSystemCommonNet6.Alarm;
using AGVSystemCommonNet6.Configuration;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.MAP;
using AGVSystemCommonNet6.Material;
using AGVSystemCommonNet6.Microservices.MCS;
using AGVSystemCommonNet6.Microservices.ResponseModel;
using AGVSystemCommonNet6.Vehicle_Control.VCS_ALARM;
using EquipmentManagment.Device;
using EquipmentManagment.MainEquipment;
using EquipmentManagment.Manager;
using EquipmentManagment.WIP;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace AGVSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public partial class TaskController : ControllerBase
    {
        private AGVSDbContext _TaskDBContent;

        private UserValidationService UserValidation { get; }
        /// <summary>
        /// 站點檢查服務
        /// </summary>
        private WorkStationStatusCheckService _workStationStatusCheckService { get; }

        private Logger logger = LogManager.GetCurrentClassLogger();

        public TaskController(AGVSDbContext content, UserValidationService userValidation, WorkStationStatusCheckService workStationStatusCheckService)
        {
            this._TaskDBContent = content;
            this.UserValidation = userValidation;
            _workStationStatusCheckService = workStationStatusCheckService;
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
            logger.Info($"User try cancle Task-{task_name}");

            if (AGVSConfigulator.SysConfigs.BaseOnKGSWebAGVSystem)
            {
                await KGSWebAGVSystemAPI.TaskOrder.OrderAPI.CancelTask(task_name);
                return Ok(true);
            }

            bool canceled = await TaskManager.Cancel(task_name, $"用戶取消任務(User manual canceled)");
            logger.Info($"User try cancle Task-{task_name}---{canceled}");
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


        [HttpGet("StartTransferCargoReport")]
        public async Task<clsAGVSTaskReportResponse> StartTransferCargoReport(string AGVName, int SourceTag, int DestineTag, string SourceSlot, string DestineSlot, bool IsSourceAGV = false)
        {
            var _response = new clsAGVSTaskReportResponse();
            (bool existDevice, clsEQ mainEQ, clsRack rack) destineDevice = TryGetEndDevice(DestineTag, int.Parse(DestineSlot));
            if (destineDevice.existDevice == false)
                return new clsAGVSTaskReportResponse() { confirm = false, message = $"[StartTransferCargoReport] 找不到Tag為{DestineTag}的終點設備" };
            else
            {
                if (IsSourceAGV)
                    return new clsAGVSTaskReportResponse { confirm = true };
                (bool existDevice, clsEQ mainEQ, clsRack rack) sourceDevice = TryGetEndDevice(SourceTag, int.Parse(SourceSlot));
                if (sourceDevice.existDevice == false)
                    return new clsAGVSTaskReportResponse() { confirm = false, message = $"[StartTransferCargoReport] 找不到Tag為{SourceTag}的起點設備" };
                else if (sourceDevice.rack != null)
                    return new clsAGVSTaskReportResponse { confirm = true };
                else if (destineDevice.rack != null)
                    return new clsAGVSTaskReportResponse { confirm = true };
                else
                {
                    return new clsAGVSTaskReportResponse { confirm = true };
                }
            }
        }

        [HttpGet("LoadUnloadTaskFinish")]
        public async Task<clsAGVSTaskReportResponse> LoadUnloadTaskFinish(string taskID, int tag, ACTION_TYPE action)
        {
            clsTaskDto? order = _TaskDBContent.Tasks.AsNoTracking().FirstOrDefault(od => od.TaskName == taskID);

            clsAGVStateDto? executingAGVState = _TaskDBContent.AgvStates.FirstOrDefault(agv => agv.TaskName == taskID);

            if (order == null)
                return new clsAGVSTaskReportResponse() { confirm = false, message = $"Task ID={taskID} not at task table of database" };
            if (action != ACTION_TYPE.Load && action != ACTION_TYPE.Unload)
                return new clsAGVSTaskReportResponse() { confirm = false, message = "Action should equal Load or Unlaod" };

            bool isUnloadFromAGV = (tag == -1 || order.IsFromAGV) && action == ACTION_TYPE.Unload;

            if (isUnloadFromAGV)
                return new clsAGVSTaskReportResponse() { confirm = true };

            bool isCarryTask = order.Action == ACTION_TYPE.Carry;
            string slotStr = "";

            if (isCarryTask)
                slotStr = action == ACTION_TYPE.Unload ? order.From_Slot : order.To_Slot;
            else
                slotStr = order.To_Slot;


            int slot = int.Parse(slotStr);

            (bool existDevice, clsEQ mainEQ, clsRack rack) = TryGetEndDevice(tag, slot);

            if (!existDevice)
                return new clsAGVSTaskReportResponse() { confirm = false, message = $"[LoadUnloadTaskFinish] 找不到Tag為{tag}的設備" };

            EndPointDeviceAbstract endPoint = null;
            bool DelayReserveCancel = SystemModes.RunMode == AGVSystemCommonNet6.AGVDispatch.RunMode.RUN_MODE.RUN &&
                                                             SystemModes.TransferTaskMode == AGVSystemCommonNet6.AGVDispatch.RunMode.TRANSFER_MODE.LOCAL_AUTO &&
                                                             isCarryTask;
            bool isDestinePortAboveEq = slot > 0 && mainEQ != null && rack != null;
            try
            {
                if (mainEQ != null)
                {
                    _ = Task.Run(async () =>
                    {
                        mainEQ.CancelToEQUpAndLow();
                        await Task.Delay(DelayReserveCancel ? 1000 : 1);
                        mainEQ.CancelReserve();
                    });
                    logger.Info($"Get AGV LD.ULD Task Finish At Tag {tag}-Action={action}. TO Eq DO ALL OFF");

                    if (!isDestinePortAboveEq)
                    {
                        endPoint = mainEQ;
                        return new clsAGVSTaskReportResponse() { confirm = true, message = $"{mainEQ.EQName} ToEQUp DO OFF" };
                    }
                }
                if (rack != null)
                {
                    endPoint = rack;
                    return new clsAGVSTaskReportResponse() { confirm = true, message = $"{action} from {rack} Finish" };
                }
                return new clsAGVSTaskReportResponse() { confirm = true, message = $"{action} from {order.From_Station} Finish" };
            }
            finally
            {

                if (isCarryTask && action == ACTION_TYPE.Load)
                {
                    EQTransferTaskManager.HandleTransferOrderFinish(order);
                }

                if (SystemModes.TransferTaskMode == AGVSystemCommonNet6.AGVDispatch.RunMode.TRANSFER_MODE.LOCAL_AUTO && endPoint.EndPointOptions.IsEmulation)
                {
                    if (action == ACTION_TYPE.Load)
                    {

                    }
                    else if (action == ACTION_TYPE.Unload && HotRunScriptManager.IsRegularUnloadRequstHotRunRunning)
                    {

                        bool isAGVLocateIsSecondaryPtOfUnloadEQ = executingAGVState?.TransferProcess == VehicleMovementStage.WorkingAtSource;
                        if (isAGVLocateIsSecondaryPtOfUnloadEQ)
                            HotRunScriptManager.RegularUnloadHotRunner.SetUnloadEqAsBusy(mainEQ.EQName);
                    }
                    //var eqEmu = StaEQPEmulatorsManagager.GetEQEmuByName(endPoint.EQName);
                    //eqEmu.SetStatusBUSY();
                    //_ = Task.Run(async () =>
                    //{
                    //    await Task.Delay(1000);
                    //    //eqEmu.SetStatusUnloadable();
                    //});
                }
                if (order != null && order.State == TASK_RUN_STATUS.NAVIGATING || order.State == TASK_RUN_STATUS.ACTION_FINISH)
                {
                    string? carrierID = action == ACTION_TYPE.Unload ? "" : order?.Actual_Carrier_ID;
                    endPoint.UpdateCarrierInfo(tag, carrierID, slot);
                }
                //endPoint.UpdateCarrierInfo(tag,)
            }
        }

        /// <summary>
        /// VMS回報取放貨任務訂單開始執行
        /// </summary>
        /// <param name="order"></param>
        /// <param name="action"></param>
        /// <param name="isSourceAGV"></param>
        /// <returns></returns>
        [HttpPost("LDULDOrderStart")]
        public async Task<clsAGVSTaskReportResponse> LDULDOrderStart([FromBody] clsTaskDto order, ACTION_TYPE action, bool isSourceAGV)
        {
            try
            {
                (WorkStationCheckResult? fromStationCheckResult, WorkStationCheckResult? toStationCheckResult) = _workStationStatusCheckService.CheckWorkStationStatus(order, action);

                clsAGVSTaskReportResponse response = new clsAGVSTaskReportResponse();

                if (order.Action == ACTION_TYPE.Carry && action == ACTION_TYPE.Unload && fromStationCheckResult.alarmCode != ALARMS.NONE)
                {
                    response.AlarmCode = fromStationCheckResult.alarmCode;
                    response.message = fromStationCheckResult.message;
                    response.message_en = fromStationCheckResult.message_en;
                    return response;
                }

                if (toStationCheckResult.alarmCode != ALARMS.NONE)
                {
                    response.AlarmCode = toStationCheckResult.alarmCode;
                    response.message = toStationCheckResult.message;
                    response.message_en = toStationCheckResult.message_en;
                    return response;
                }

                try
                {
                    fromStationCheckResult?.TryReserveEq(out string errMsg);
                    fromStationCheckResult?.TryToEq(out errMsg);
                    toStationCheckResult?.TryReserveEq(out errMsg);
                    toStationCheckResult?.TryToEq(out errMsg);
                }
                catch (Exception ex)
                {

                }
                //if (action == ACTION_TYPE.Unload || action == ACTION_TYPE.LoadAndPark || action == ACTION_TYPE.Load)
                //{
                //    clsAGVSTaskReportResponse result = ((OkObjectResult)await LoadUnloadTaskStart(to, ToSlot, action)).Value as clsAGVSTaskReportResponse;
                //    return result;
                //}
                //else if (action == ACTION_TYPE.Carry)
                //{
                //    if (!isSourceAGV)
                //    {
                //        clsAGVSTaskReportResponse result_from = ((OkObjectResult)await LoadUnloadTaskStart(from, FromSlot, ACTION_TYPE.Unload)).Value as clsAGVSTaskReportResponse;
                //        if (result_from.confirm == false)
                //            return result_from;
                //    }

                //    clsAGVSTaskReportResponse result_to = ((OkObjectResult)await LoadUnloadTaskStart(to, ToSlot, ACTION_TYPE.Load)).Value as clsAGVSTaskReportResponse;
                //    return result_to;
                //}
                //else
                //{
                //    // LDULDOrder not accept other action type
                //    return new clsAGVSTaskReportResponse() { confirm = false, message = $"LDULDOrder ACTION_TYPE can not be={action}" };
                //}
                return new clsAGVSTaskReportResponse() { confirm = true, message = $"" };
            }
            catch (Exception ex)
            {
                return new clsAGVSTaskReportResponse() { confirm = false, message = $"{ex}" };
            }
        }


        [HttpPost("LoadUnloadTaskStart")]
        public async Task<clsAGVSTaskReportResponse> LoadUnloadTaskStart([FromBody] clsTaskDto order, ACTION_TYPE action)
        {
            (WorkStationCheckResult? fromStationCheckResult, WorkStationCheckResult? toStationCheckResult) = _workStationStatusCheckService.CheckWorkStationStatus(order, action);

            clsAGVSTaskReportResponse response = new clsAGVSTaskReportResponse();

            if (order.Action == ACTION_TYPE.Carry && action == ACTION_TYPE.Unload && fromStationCheckResult.alarmCode != ALARMS.NONE)
            {
                response.AlarmCode = fromStationCheckResult.alarmCode;
                response.message = fromStationCheckResult.message;
                response.message_en = fromStationCheckResult.message_en;
                return response;
            }

            if (toStationCheckResult.alarmCode != ALARMS.NONE)
            {
                response.AlarmCode = toStationCheckResult.alarmCode;
                response.message = toStationCheckResult.message;
                response.message_en = toStationCheckResult.message_en;
                return response;
            }
            //if (action != ACTION_TYPE.Load && action != ACTION_TYPE.Unload)
            //    return Ok(new clsAGVSTaskReportResponse() { confirm = false, message = "Action should equal Load or Unlaod" });


            //AGVSystemCommonNet6.MAP.MapPoint MapPoint = AGVSMapManager.GetMapPointByTag(tag);
            //if (MapPoint == null)
            //    return Ok(new clsAGVSTaskReportResponse() { confirm = false, AlarmCode = ALARMS.EQ_TAG_NOT_EXIST_IN_CURRENT_MAP, message = $"站點TAG-{tag} 不存在於當前地圖" });

            //if (!MapPoint.Enable)
            //    return Ok(new clsAGVSTaskReportResponse() { confirm = false, AlarmCode = ALARMS.Station_Disabled, message = $"站點TAG-{tag} 未啟用，無法指派任務" });

            //if (action == ACTION_TYPE.Load && (MapPoint.StationType == MapPoint.STATION_TYPE.Buffer_EQ || MapPoint.StationType == MapPoint.STATION_TYPE.Buffer) && slot == -2)
            //{
            //    clsPortOfRack port = EQTransferTaskManager.get_empyt_port_of_rack(tag);
            //    return Ok(new clsAGVSTaskReportResponse() { confirm = true, message = $"Get empty port OK", ReturnObj = port.Layer });
            //}

            //(bool confirm, ALARMS alarm_code, string message, string message_en, object obj, Type objtype) result = EQTransferTaskManager.CheckLoadUnloadStation(tag, slot, action, bypasseqandrackckeck: false);
            //if (result.confirm == false)
            //{
            //    return Ok(new clsAGVSTaskReportResponse() { confirm = false, message = $"{result.message}", message_en = result.message_en, AlarmCode = result.alarm_code });
            //}
            //else
            //{
            //    if (result.objtype == typeof(clsEQ))
            //    {
            //        clsEQ mainEQ = (clsEQ)result.obj;
            //        try
            //        {
            //            mainEQ.Reserve();
            //            mainEQ.ToEQ();

            //            if (action == ACTION_TYPE.Unload)
            //            {
            //                clsTaskDto? taskExist = DatabaseCaches.TaskCaches.RunningTasks.FirstOrDefault(task => task.From_Station_Tag == mainEQ.EndPointOptions.TagID && task.Action == ACTION_TYPE.Carry);

            //                if (taskExist != null && StaEQPManagager.TryGetEQByTag(taskExist.To_Station_Tag, out clsEQ destineDevice))
            //                {
            //                    RACK_CONTENT_STATE rackContentStateOfSourceEQ = StaEQPManagager.CargoContentTypeCheckWhenStartTransferToDestineHandler(mainEQ, destineDevice);
            //                }
            //            }


            //            if (mainEQ.EndPointOptions.IsOneOfDualPorts)
            //            {
            //                bool isForkAGVOnlyPort = mainEQ.EndPointOptions.Accept_AGV_Type == EquipmentManagment.Device.Options.VEHICLE_TYPE.FORK;
            //                bool isSubmarineAGVOnlyPort = mainEQ.EndPointOptions.Accept_AGV_Type == EquipmentManagment.Device.Options.VEHICLE_TYPE.SUBMERGED_SHIELD;
            //                if (isForkAGVOnlyPort)
            //                    await GPMCIMService.ChangePortTypeOfEq(mainEQ.EndPointOptions.TagID, 0);
            //                if (isSubmarineAGVOnlyPort)
            //                    await GPMCIMService.ChangePortTypeOfEq(mainEQ.EndPointOptions.TagID, 1);
            //            }
            //        }
            //        catch (Exception ex)
            //        {
            //            return Ok(new clsAGVSTaskReportResponse() { confirm = false, message = $"{mainEQ.EQName} ToEQUp DO ON的過程中發生錯誤:{ex.Message}" });
            //        }
            //        logger.Info($"Get AGV LD.ULD Task Start At Tag {tag}-Action={action}. TO Eq Up DO ON");
            //        return Ok(new clsAGVSTaskReportResponse() { confirm = true, message = $"{mainEQ.EQName} ToEQUp DO ON" });
            //    }
            //    else if (result.objtype == typeof(clsPortOfRack))
            //    {
            //        return Ok(new clsAGVSTaskReportResponse() { confirm = true, message = result.message });
            //    }
            //    else
            //    {
            //        return Ok(new clsAGVSTaskReportResponse() { confirm = false, message = "NOT EQ or RACK" });
            //    }
            //}
            return new clsAGVSTaskReportResponse(true, ALARMS.NONE, "", "");
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
        private (bool existDevice, clsEQ mainEQ, clsRack rack) TryGetEndDevice(int tag, int slot)
        {
            MapPoint _mapPt = AGVSMapManager.GetMapPointByTag(tag);
            bool isPureWIP = _mapPt.StationType == MapPoint.STATION_TYPE.Buffer || _mapPt.StationType == MapPoint.STATION_TYPE.Charge_Buffer;
            clsEQ? Eq = isPureWIP ? null : StaEQPManagager.MainEQList.FirstOrDefault(eq => eq.EndPointOptions.TagID == tag && eq.EndPointOptions.Height == slot);
            if (slot > 0 && _mapPt.StationType == MapPoint.STATION_TYPE.Buffer_EQ)
            {
                //try get eq at first slot.
                Eq = isPureWIP ? null : StaEQPManagager.MainEQList.FirstOrDefault(eq => eq.EndPointOptions.TagID == tag && eq.EndPointOptions.Height == 0);
            }
            clsRack? Rack = StaEQPManagager.RacksList.FirstOrDefault(x => x.PortsStatus.Any(p => p.TagNumbers.Contains(tag)));
            return (Eq != null || Rack != null, Eq, Rack);
        }

        private async Task<object> AddTask(clsTaskDto taskData, string user = "")
        {

            if (AGVSConfigulator.SysConfigs.BaseOnKGSWebAGVSystem)
            {
                KGSWebAGVSystemAPI.TaskOrder.MissionRequestParams kgMissionRequest = taskData.ToKGSMissionRequestParam();
                try
                {
                    await KGSWebAGVSystemAPI.TaskOrder.OrderAPI.AddTask(kgMissionRequest);
                    return new { confirm = true };
                }
                catch (Exception ex)
                {
                    return new { confirm = false, message = ex.Message };
                }
            }

            taskData.DispatcherName = user;
            try
            {

                (WorkStationCheckResult fromStationCheckResult, WorkStationCheckResult toStationCheckResult) = _workStationStatusCheckService.CheckWorkStationStatus(taskData, taskData.Action);

                if (taskData.Action == ACTION_TYPE.Carry && fromStationCheckResult != null && fromStationCheckResult.alarmCode != ALARMS.NONE)
                {
                    string stationName = fromStationCheckResult.workStationName;
                    AlarmManagerCenter.AddAlarmAsync(fromStationCheckResult.alarmCode, level: ALARM_LEVEL.WARNING, Equipment_Name: stationName);
                    return new { confirm = false, alarm_code = fromStationCheckResult.alarmCode, message = fromStationCheckResult.message, message_en = fromStationCheckResult.message_en };
                }

                if (toStationCheckResult != null && toStationCheckResult.alarmCode != ALARMS.NONE)
                {
                    string stationName = toStationCheckResult.workStationName;
                    AlarmManagerCenter.AddAlarmAsync(toStationCheckResult.alarmCode, level: ALARM_LEVEL.WARNING, Equipment_Name: stationName);
                    return new { confirm = false, alarm_code = toStationCheckResult.alarmCode, message = toStationCheckResult.message, message_en = toStationCheckResult.message_en };

                }


                (bool confirm, ALARMS alarm_code, string message, string message_en) result = await TaskManager.AddTask(taskData, TaskManager.TASK_RECIEVE_SOURCE.MANUAL);

                if (!result.confirm && string.IsNullOrEmpty(result.message))
                {

                }

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
                return new { confirm = result.confirm, alarm_code = result.alarm_code, message = result.message, message_en = result.message_en, showEmptyOrFullContentCheck = showEmptyOrFullContentCheck };


            }
            catch (Exception exc)
            {

                throw exc;
            }
        }


    }
}
