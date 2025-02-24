using AGVSystem.Models.Map;
using AGVSystem.Models.Sys;
using AGVSystem.Models.TaskAllocation.HotRun;
using AGVSystem.Service;
using AGVSystem.Service.MCS;
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
using AGVSystemCommonNet6.Notify;
using EquipmentManagment.Device;
using EquipmentManagment.MainEquipment;
using EquipmentManagment.Manager;
using EquipmentManagment.WIP;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NLog;
using static AGVSystem.TaskManagers.EQTransferTaskManager;
using static EquipmentManagment.MainEquipment.clsEQ;

namespace AGVSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public partial class TaskController : ControllerBase
    {
        private AGVSDbContext _TaskDBContent;

        private UserValidationService UserValidation { get; }

        private Logger logger = LogManager.GetCurrentClassLogger();

        private readonly MCSService mcsService;

        public TaskController(AGVSDbContext content, UserValidationService userValidation, MCSService mcsService)
        {
            this._TaskDBContent = content;
            this.UserValidation = userValidation;
            this.mcsService = mcsService;
        }

        [HttpGet("Allocation")]
        [Authorize]
        public async Task<IActionResult> Test()
        {
            return Ok();
        }

        [HttpPost("ReAssignTask")]
        [Authorize]
        public async Task<IActionResult> ReAssignTask([FromBody] clsTaskDto taskData, string user = "", bool autoSelectVehicle = false)
        {
            if (taskData.isFromMCS)
            {
                return Ok(new { confirm = false, message = "禁止手動重新指派MCS任務", message_en = "Can not re-assign order from MCS" });
            }
            if (SystemModes.TransferTaskMode != AGVSystemCommonNet6.AGVDispatch.RunMode.TRANSFER_MODE.MANUAL)
            {
                return Ok(new { confirm = false, message = "自動派工模式下禁止手動重新指派任務", message_en = "Can not re-assign order in AUTO DISPATCH mode" });
            }

            int existTaskNnm = _TaskDBContent.Tasks.Count(tk => tk.TaskName.Contains(taskData.TaskName));
            taskData.TaskName = taskData.TaskName + $"-{existTaskNnm}";
            taskData.RecieveTime = DateTime.Now;
            taskData.State = TASK_RUN_STATUS.WAIT;
            taskData.FinishTime = DateTime.MinValue;
            taskData.FailureReason = "";
            taskData.need_change_agv = false;
            taskData.DesignatedAGVName = autoSelectVehicle ? "" : taskData.DesignatedAGVName;
            taskData.bypass_eq_status_check = false;
            return Ok(await AddTask(taskData, user));
        }


        [HttpGet("Cancel")]
        public async Task<IActionResult> Cancel(string task_name, string? reason, string? raiserName)
        {
            logger.Info($"User try cancle Task-{task_name}");

            if (AGVSConfigulator.SysConfigs.BaseOnKGSWebAGVSystem)
            {
                await KGSWebAGVSystemAPI.TaskOrder.OrderAPI.CancelTask(task_name);
                return Ok(true);
            }
            bool canceled = await TaskManager.Cancel(task_name, $"用戶取消任務(User manual canceled)", hostAction: reason);
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

        [HttpPost("DeepCharge")]
        [Authorize]
        public async Task<IActionResult> DeepChargeTask([FromBody] clsTaskDto taskData, string user = "")
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
        public async Task<clsAGVSTaskReportResponse> LoadUnloadTaskStart(string? taskID, int tag, int slot, ACTION_TYPE action)
        {
            try
            {
                return await CheckStatus(taskID, tag, slot, action);
            }
            catch (Exception ex)
            {
                return new clsAGVSTaskReportResponse
                {
                    confirm = false,
                    message = ex.Message,
                    AlarmCode = ALARMS.VMSOrderActionStatusReportToAGVSButAGVSGetException
                };
            }
        }

        private async Task<clsAGVSTaskReportResponse> CheckStatus(string? taskID, int tag, int slot, ACTION_TYPE action)
        {
            if (action != ACTION_TYPE.Load && action != ACTION_TYPE.Unload)
                return (new clsAGVSTaskReportResponse() { confirm = false, message = "Action should equal Load or Unlaod" });


            AGVSystemCommonNet6.MAP.MapPoint MapPoint = AGVSMapManager.GetMapPointByTag(tag);
            if (MapPoint == null)
                return (new clsAGVSTaskReportResponse() { confirm = false, AlarmCode = ALARMS.EQ_TAG_NOT_EXIST_IN_CURRENT_MAP, message = $"站點TAG-{tag} 不存在於當前地圖" });

            if (!MapPoint.Enable)
                return (new clsAGVSTaskReportResponse() { confirm = false, AlarmCode = ALARMS.Station_Disabled, message = $"站點TAG-{tag} 未啟用，無法指派任務" });

            if (action == ACTION_TYPE.Load && (MapPoint.StationType == MapPoint.STATION_TYPE.Buffer_EQ || MapPoint.StationType == MapPoint.STATION_TYPE.Buffer) && slot == -2)
            {
                clsPortOfRack port = EQTransferTaskManager.get_empyt_port_of_rack(tag);
                return (new clsAGVSTaskReportResponse() { confirm = true, message = $"Get empty port OK", ReturnObj = port.Layer });
            }

            (bool confirm, ALARMS alarm_code, string message, string message_en, object obj, Type objtype) result = EQTransferTaskManager.CheckLoadUnloadStation(tag, slot, action, out DeviceIDInfo deviceIDInfo, bypasseqandrackckeck: false);
            if (result.confirm == false)
            {
                return (new clsAGVSTaskReportResponse() { confirm = false, message = $"{result.message}", message_en = result.message_en, AlarmCode = result.alarm_code });
            }
            else
            {
                if (result.objtype == typeof(clsEQ))
                {
                    clsEQ mainEQ = (clsEQ)result.obj;
                    try
                    {
                        string carrierIDAssigned = TryGetCarrierIDAssign(taskID);

                        mainEQ.Reserve(carrierIDAssigned);
                        mainEQ.ToEQ();
                        if (action == ACTION_TYPE.Unload)
                        {
                            clsTaskDto? taskExist = DatabaseCaches.TaskCaches.RunningTasks.FirstOrDefault(task => task.From_Station_Tag == mainEQ.EndPointOptions.TagID && task.Action == ACTION_TYPE.Carry);

                            if (taskExist != null && StaEQPManagager.TryGetEQByTag(taskExist.To_Station_Tag, out clsEQ destineDevice))
                            {
                                RACK_CONTENT_STATE rackContentStateOfSourceEQ = StaEQPManagager.CargoContentTypeCheckWhenStartTransferToDestineHandler(mainEQ, destineDevice);
                            }
                        }


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
                        return (new clsAGVSTaskReportResponse() { confirm = false, message = $"{mainEQ.EQName} ToEQUp DO ON的過程中發生錯誤:{ex.Message}" });
                    }
                    logger.Info($"Get AGV LD.ULD Task Start At Tag {tag}-Action={action}. TO Eq Up DO ON");
                    return (new clsAGVSTaskReportResponse() { confirm = true, message = $"{mainEQ.EQName} ToEQUp DO ON" });
                }
                else if (result.objtype == typeof(clsPortOfRack))
                {
                    return (new clsAGVSTaskReportResponse() { confirm = true, message = result.message });
                }
                else
                {
                    return (new clsAGVSTaskReportResponse() { confirm = false, message = "NOT EQ or RACK" });
                }
            }


            string TryGetCarrierIDAssign(string taskID)
            {
                clsTaskDto executingOrder = DatabaseCaches.TaskCaches.InCompletedTasks.FirstOrDefault(order => order.TaskName == taskID);
                return executingOrder != null ? executingOrder.Carrier_ID : "";
            }
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
        public async Task<clsAGVSTaskReportResponse> LoadUnloadTaskFinish(string taskID, int tag, ACTION_TYPE action, bool normalDone)
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
                        bool IsEQZoneAndHasReader = mainEQ.EndPointOptions.IsRoleAsZone && mainEQ.EndPointOptions.IsCSTIDReportable;
                        mainEQ.CancelToEQUpAndLow();
                        if (IsEQZoneAndHasReader)
                        {
                            NotifyServiceHelper.WARNING($"設備 [{mainEQ.EQName}] 因屬於Zone且具有CST Reader功能，等待Reader讀取/清除完畢後取消Reserve訊號");
                            await WaitEqCstReadDone(mainEQ, action);
                        }
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

                if (isCarryTask && action == ACTION_TYPE.Unload)
                {
                    int removedNum = EQTransferTaskManager.TryRemoveWaitUnloadEQ(order.From_Station_Tag, order.GetFromSlotInt());
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
                //if (order != null && normalDone && order.State == TASK_RUN_STATUS.NAVIGATING || order.State == TASK_RUN_STATUS.ACTION_FINISH)
                //{
                //    string? carrierID = action == ACTION_TYPE.Unload ? "" : order?.Actual_Carrier_ID;
                //    if (action == ACTION_TYPE.Load && executingAGVState!=null)
                //        await MCSCIMService.CarrierRemoveCompletedReport(carrierID, executingAGVState.AGV_ID, "", 1);
                //    endPoint?.UpdateCarrierInfo(tag, carrierID, slot);
                //}
                //endPoint.UpdateCarrierInfo(tag,)
            }
        }

        private async Task WaitEqCstReadDone(clsEQ mainEQ, ACTION_TYPE action)
        {
            CancellationTokenSource cancelWaitCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            while (!IsReaderActionDone())
            {
                await Task.Delay(1000);
                if (cancelWaitCts.IsCancellationRequested)
                {
                    NotifyServiceHelper.ERROR($"等待 {mainEQ.EQName}  CST Reader {(action == ACTION_TYPE.Load ? "讀取" : "清空")} 超時!");
                    return;
                }
            }

            bool IsReaderActionDone()
            {
                if (action == ACTION_TYPE.Load)
                    return !string.IsNullOrEmpty(mainEQ.CSTIDReadValue);
                else if (action == ACTION_TYPE.Unload)
                    return string.IsNullOrEmpty(mainEQ.CSTIDReadValue);
                else
                    return true;
            }
        }

        [HttpGet("LDULDOrderStart")]
        public async Task<clsAGVSTaskReportResponse> LDULDOrderStart(string? taskID, int from, int FromSlot, int to, int ToSlot, ACTION_TYPE action, bool isSourceAGV)
        {
            try
            {
                if (action == ACTION_TYPE.Unload || action == ACTION_TYPE.LoadAndPark || action == ACTION_TYPE.Load)
                {
                    clsAGVSTaskReportResponse result = await CheckStatus(taskID, to, ToSlot, action);
                    return result;
                }
                else if (action == ACTION_TYPE.Carry)
                {
                    if (!isSourceAGV)
                    {
                        clsAGVSTaskReportResponse result_from = await CheckStatus(taskID, from, FromSlot, ACTION_TYPE.Unload);
                        if (!result_from.confirm)
                            return result_from;
                    }

                    clsAGVSTaskReportResponse result_to = await CheckStatus(taskID, to, ToSlot, ACTION_TYPE.Load);

                    await Task.Delay(1).ContinueWith(async t =>
                    {
                        try
                        {
                            await GPMCIMService.ChangePortTypeOfEq(to, 1);
                        }
                        catch (Exception ex)
                        {

                        }
                    });

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
                return new clsAGVSTaskReportResponse
                {
                    confirm = false,
                    message = ex.Message,
                    AlarmCode = ALARMS.VMSOrderActionStatusReportToAGVSButAGVSGetException
                };
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
        private (bool existDevice, clsEQ mainEQ, clsRack rack) TryGetEndDevice(int tag, int slot)
        {
            MapPoint _mapPt = AGVSMapManager.GetMapPointByTag(tag);
            bool isPureWIP = _mapPt.StationType == MapPoint.STATION_TYPE.Buffer || _mapPt.StationType == MapPoint.STATION_TYPE.Charge_Buffer;
            clsEQ? Eq = isPureWIP ? null : StaEQPManagager.MainEQList.FirstOrDefault(eq => eq.EndPointOptions.TagID == tag && eq.EndPointOptions.Height == slot);
            if (slot > 0)
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
                (bool confirm, ALARMS alarm_code, string message, string message_en) result = await TaskManager.AddTask(taskData, TaskManager.TASK_RECIEVE_SOURCE.Local_MANUAL);

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
