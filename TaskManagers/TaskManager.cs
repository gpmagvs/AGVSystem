using AGVSystem.Controllers;
using AGVSystem.Models.BayMeasure;
using AGVSystem.Models.Map;
using AGVSystem.Models.Sys;
using AGVSystemCommonNet6;
using AGVSystemCommonNet6.AGVDispatch;
using AGVSystemCommonNet6.AGVDispatch.Messages;
using AGVSystemCommonNet6.AGVDispatch.RunMode;
using AGVSystemCommonNet6.Alarm;
using AGVSystemCommonNet6.Configuration;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.DATABASE.Helpers;
using AGVSystemCommonNet6.HttpTools;
using AGVSystemCommonNet6.Log;
using AGVSystemCommonNet6.MAP;
using AGVSystemCommonNet6.Material;
using AGVSystemCommonNet6.Microservices.MCS;
using AGVSystemCommonNet6.Microservices.ResponseModel;
using AGVSystemCommonNet6.Microservices.VMS;
using EquipmentManagment.Device.Options;
using EquipmentManagment.Manager;
using EquipmentManagment.WIP;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;
using System.Threading.Tasks;
using WebSocketSharp;
using static AGVSystem.TaskManagers.EQTransferTaskManager;
using static AGVSystemCommonNet6.MAP.MapPoint;
using static AGVSystemCommonNet6.Microservices.MCS.MCSCIMService;
using static SQLite.SQLite3;
using static System.Collections.Specialized.BitVector32;

namespace AGVSystem.TaskManagers
{
    public class TaskManager
    {
        public enum TASK_RECIEVE_SOURCE
        {
            LOCAL_Auto,
            Local_MANUAL,
            REMOTE,
        }

        public static async Task<(bool confirm, ALARMS alarm_code, string message, string message_en)> AddTask(clsTaskDto taskData, TASK_RECIEVE_SOURCE source)
        {
            ACTION_TYPE _order_action = taskData.Action;

            bool isCarryAndSourceIsAGV = _order_action == ACTION_TYPE.Carry && taskData.IsFromAGV;

            bool fromTagParsed = int.TryParse(taskData.From_Station, out int source_station_tag);
            bool toTagParsed = int.TryParse(taskData.To_Station, out int destine_station_tag);


            AGVSystemCommonNet6.MAP.MapPoint sourcePoint = AGVSMapManager.GetMapPointByTag(source_station_tag);
            AGVSystemCommonNet6.MAP.MapPoint destinePoint = AGVSMapManager.GetMapPointByTag(destine_station_tag);

            using AGVSDatabase database = new AGVSDatabase();
            #region   AGV 狀態檢查
            // AGV 有貨不可派取貨or搬運, 無貨不可派放貨, 有貨不能去充電(非潛盾車型)
            if (taskData.DesignatedAGVName != "")
            {
                IEnumerable<clsAGVStateDto> agvstates = database.tables.AgvStates;
                clsAGVStateDto? _agv_assigned = agvstates.FirstOrDefault(agv_dat => agv_dat.AGV_Name == taskData.DesignatedAGVName);
                VEHICLE_TYPE model = _agv_assigned.Model.ConvertToEQAcceptAGVTYPE();

                //一班移動任務 檢查
                if (_order_action == ACTION_TYPE.None && _IsMoveDestineIsForbidden(model, destine_station_tag))
                {
                    return (false, ALARMS.Navigation_Path_Contain_Forbidden_Point, $"目的地({destine_station_tag})已被設為{model}車款禁止停車或通過", $"Destine Tag({destine_station_tag}) is not allow {model} AGV to reach or pass");
                }

                bool _IsMoveDestineIsForbidden(VEHICLE_TYPE model, int destineTag)
                {
                    var forbidenTagMap = model == VEHICLE_TYPE.FORK ? AGVSMapManager.CurrentMap.TagForbiddenForForkAGV : AGVSMapManager.CurrentMap.TagForbiddenForSubMarineAGV;
                    return forbidenTagMap.Contains(destineTag);
                }


                if (!isCarryAndSourceIsAGV && (taskData.Action == ACTION_TYPE.Unload || taskData.Action == ACTION_TYPE.Carry) && _agv_assigned.CargoStatus != 0)
                {
                    AlarmManagerCenter.AddAlarmAsync(ALARMS.CANNOT_DISPATCH_CARRY_TASK_WHEN_AGV_HAS_CARGO);
                    return (false, ALARMS.CANNOT_DISPATCH_CARRY_TASK_WHEN_AGV_HAS_CARGO, $"{_agv_assigned.AGV_Name} 車上有貨物無法指派[{taskData.ActionName}]任務", $"{_agv_assigned.AGV_Name} with cargo can not assigned to {taskData.Action}");
                }
                else if (taskData.Action == ACTION_TYPE.Load && _agv_assigned.CargoStatus == 0)
                {
                    AlarmManagerCenter.AddAlarmAsync(ALARMS.AGV_NO_Carge_Cannot_Transfer_Cargo_From_AGV_To_Desinte);
                    return (false, ALARMS.AGV_NO_Carge_Cannot_Transfer_Cargo_From_AGV_To_Desinte, $"{_agv_assigned.AGV_Name} 車上無貨物無法指派[{taskData.ActionName}]任務", $"{_agv_assigned.AGV_Name} no cargo can not assigned to {taskData.Action}");
                }
                if ((taskData.Action == ACTION_TYPE.Charge || taskData.Action == ACTION_TYPE.DeepCharge) && _agv_assigned.Model != clsEnums.AGV_TYPE.SUBMERGED_SHIELD && (_agv_assigned.CargoStatus != 0 || _agv_assigned.CurrentCarrierID != ""))
                {
                    AlarmManagerCenter.AddAlarmAsync(ALARMS.CannotAssignChargeJobBecauseWrongCargoStatus);
                    return (false, ALARMS.CannotAssignChargeJobBecauseWrongCargoStatus, $"車型非{clsEnums.AGV_TYPE.SUBMERGED_SHIELD}車上有貨不行進行充電任務", $"{_agv_assigned.AGV_Name} Has Cargo Can't Execute Charge Task");
                }
            }
            #endregion

            bool source_station_disabled = sourcePoint == null || source_station_tag == -1 ? false : !sourcePoint.Enable;
            bool destine_station_disabled = destinePoint == null || destine_station_tag == -1 ? false : !destinePoint.Enable;
            bool destine_station_isequipment = destinePoint == null || destine_station_tag == -1 ? false : destinePoint.IsEquipment;
            if (source_station_disabled)
            {
                AlarmManagerCenter.AddAlarmAsync(ALARMS.Station_Disabled);
                return (false, ALARMS.Station_Disabled, $"來源站點 {sourcePoint?.Name} 未啟用，無法指派任務", $"Station {sourcePoint?.Name} isn't enable,can't dispatch any task");
            }
            if (destine_station_disabled)
            {
                AlarmManagerCenter.AddAlarmAsync(ALARMS.Station_Disabled);
                return (false, ALARMS.Station_Disabled, "目標站點未啟用，無法指派任務", $"Station {destinePoint?.Name} isn't enable,can't dispatch any task");
            }
            if (destine_station_isequipment == true)
            {
                if (_order_action == ACTION_TYPE.None)
                    return (false, ALARMS.Station_Disabled, "目標站點為設備，無法指派移動任務", "Can't dispatch MOVE task when destine is Equipment type");
                else if (_order_action == ACTION_TYPE.Park && !destinePoint.IsParking)
                    return (false, ALARMS.Station_Disabled, "目標站點非可停車點，無法指派停車任務", "Can't dispatch PARK task when destine isn't parkable");
            }
            DeviceIDInfo sourceDeviceIDInfo = new DeviceIDInfo();
            DeviceIDInfo destineDeviceIDInfo = new DeviceIDInfo();
            #region 設備狀態檢查
            if (!taskData.bypass_eq_status_check && (_order_action == ACTION_TYPE.Load || _order_action == ACTION_TYPE.LoadAndPark || _order_action == ACTION_TYPE.Unload || _order_action == ACTION_TYPE.Carry))
            {
                (bool confirm, ALARMS alarm_code, string message, string message_en) results = (false, ALARMS.NONE, "", "");
                (bool confirm, ALARMS alarm_code, string message, string message_en, object obj, Type objtype) results2;

                if (taskData.Action == ACTION_TYPE.Unload || taskData.Action == ACTION_TYPE.Load || taskData.Action == ACTION_TYPE.LoadAndPark)
                {
                    results2 = EQTransferTaskManager.CheckLoadUnloadStation(destine_station_tag, Convert.ToInt16(taskData.To_Slot), taskData.Action, out destineDeviceIDInfo, bypasseqandrackckeck: taskData.bypass_eq_status_check);
                    results.confirm = results2.confirm;
                    results.alarm_code = results2.alarm_code;
                    results.message = results2.message;
                    if (!results.confirm)
                        return results;
                }
                else if (taskData.Action == ACTION_TYPE.Carry)
                {
                    if (!isCarryAndSourceIsAGV)
                    {
                        results2 = EQTransferTaskManager.CheckLoadUnloadStation(source_station_tag, Convert.ToInt16(taskData.From_Slot), ACTION_TYPE.Unload, out sourceDeviceIDInfo, bypasseqandrackckeck: taskData.bypass_eq_status_check);
                        results.confirm = results2.confirm;
                        results.alarm_code = results2.alarm_code;
                        results.message = results2.message;
                        results.message_en = results2.message_en;
                        if (!results.confirm)
                            return results;
                    }
                    results2 = EQTransferTaskManager.CheckLoadUnloadStation(destine_station_tag, Convert.ToInt16(taskData.To_Slot), ACTION_TYPE.Load, out destineDeviceIDInfo, bypasseqandrackckeck: taskData.bypass_eq_status_check);
                    results.confirm = results2.confirm;
                    results.alarm_code = results2.alarm_code;
                    results.message = results2.message;
                    results.message_en = results2.message_en;
                    if (!results.confirm)
                        return results;
                }
            }
            #endregion

            #region 檢查設備與車輛 AGV車款與設備允許車款確認
            bool bypass = false;
            if (taskData.DesignatedAGVName != "")
            {
                if (!bypass && (_order_action == ACTION_TYPE.Load || _order_action == ACTION_TYPE.LoadAndPark
                                                   || _order_action == ACTION_TYPE.Unload || _order_action == ACTION_TYPE.Carry))
                {
                    (bool confirm, ALARMS alarm_code, string message, string message_en) results = (false, ALARMS.NONE, "", "");

                    IEnumerable<clsAGVStateDto> agvstates = database.tables.AgvStates;

                    clsAGVStateDto? _agv_assigned = agvstates.FirstOrDefault(agv_dat => agv_dat.AGV_Name == taskData.DesignatedAGVName);
                    VEHICLE_TYPE model = _agv_assigned.Model.ConvertToEQAcceptAGVTYPE();


                    if (taskData.Action == ACTION_TYPE.Unload)
                    {
                        if (destinePoint.StationType == STATION_TYPE.Buffer)
                        {
                            if (model == VEHICLE_TYPE.SUBMERGED_SHIELD)
                            {
                                results = (false, ALARMS.NONE, "AGV為潛盾無法指派站點類型為[Buffer]的任務", $"Station Type = {destinePoint.StationType} can not accept car model = {model}");
                                return results;
                            }
                            else
                            {
                                // Do nothing
                            }
                        }
                        else if (destinePoint.StationType == STATION_TYPE.Buffer_EQ && (Convert.ToInt16(taskData.To_Slot) > 0 && model == VEHICLE_TYPE.SUBMERGED_SHIELD))
                        {
                            results = (false, ALARMS.NONE, $"站點類型 = {destinePoint.StationType} 無法接受車型 = {model} 在槽位 {taskData.To_Slot} 進行 {ACTION_TYPE.Unload}", $"Station Type = {destinePoint.StationType} can not accept car model = {model} to {ACTION_TYPE.Unload} at slot {taskData.To_Slot}");
                            return results;
                        }
                        else
                        {
                            results = EQTransferTaskManager.CheckEQAcceptAGVType(destine_station_tag, taskData.GetToSlotInt(), taskData.DesignatedAGVName, taskData.need_change_agv);
                            if (!results.confirm)
                                return results;
                        }
                    }
                    else if (taskData.Action == ACTION_TYPE.Load)
                    {
                        if (destinePoint.StationType == STATION_TYPE.Buffer)
                        {
                            if (model == VEHICLE_TYPE.SUBMERGED_SHIELD)
                                taskData.need_change_agv = true;
                        }
                        else if (destinePoint.StationType == STATION_TYPE.Buffer_EQ && Convert.ToInt16(taskData.To_Slot) > 0 && model == VEHICLE_TYPE.SUBMERGED_SHIELD)
                        { taskData.need_change_agv = true; }
                        else
                        {
                            results = EQTransferTaskManager.CheckEQAcceptAGVType(destine_station_tag, taskData.GetToSlotInt(), taskData.DesignatedAGVName, taskData.need_change_agv);
                            if (!results.confirm)
                                taskData.need_change_agv = true;

                            // TODO (需再新增EQTransferTaskManager.CheckEQAcceptCargoType) 放貨
                            // 須知道車子目前背KUAN or TRAY 再比對放貨站點可接受貨物類型                            
                        }
                    }
                    else if (taskData.Action == ACTION_TYPE.Carry) // 先檢查From Station,如果允許再比From Station及 To Station如果兩個不同則生成轉運
                    {
                        if (!isCarryAndSourceIsAGV)
                        {

                            if (sourcePoint?.StationType == STATION_TYPE.Buffer)
                            {
                                if (model == VEHICLE_TYPE.SUBMERGED_SHIELD)
                                {
                                    results = (false, ALARMS.NONE, $"AGV為潛盾無法指派站點類型為[Buffer]的任務", $"Station Type = {sourcePoint.StationType} can not accept car model = {model}");
                                    return results;
                                }
                                else
                                {
                                    // Do nothing
                                }
                            }
                            else if (sourcePoint.StationType == STATION_TYPE.Buffer_EQ && (Convert.ToInt16(taskData.To_Slot) > 0 && model == VEHICLE_TYPE.SUBMERGED_SHIELD))
                            {
                                results = (false, ALARMS.NONE, $"無法指派潛盾AGV進行在 Buffer_EQ 第一層以上的取放任務", $"Station Type = {sourcePoint.StationType} can not accept car model = {model} to {ACTION_TYPE.Unload} at slot {taskData.To_Slot}");
                                return results;
                            }
                            else
                            {
                                results = EQTransferTaskManager.CheckEQAcceptAGVType(source_station_tag, taskData.GetFromSlotInt(), taskData.DesignatedAGVName, taskData.need_change_agv);
                                if (!results.confirm)
                                    return results;
                            }
                        }

                        if (destinePoint.StationType == STATION_TYPE.Buffer)
                        {
                            if (model == VEHICLE_TYPE.SUBMERGED_SHIELD)
                                taskData.need_change_agv = true;
                        }
                        else if (destinePoint.StationType == STATION_TYPE.Buffer_EQ && Convert.ToInt16(taskData.To_Slot) > 0 && model == VEHICLE_TYPE.SUBMERGED_SHIELD)
                        {
                            taskData.need_change_agv = true;
                        }
                        else
                        {
                            results = EQTransferTaskManager.CheckEQAcceptAGVType(destine_station_tag, taskData.GetToSlotInt(), taskData.DesignatedAGVName, taskData.need_change_agv);
                            if (results.confirm == false)
                                taskData.need_change_agv = true;
                        }
                    }
                }
            }
            #endregion

            #region 若起點設定是AGV,則起點要設為
            if (isCarryAndSourceIsAGV)
            {
                //起點是AGV 確認AGV是否有貨
                bool agv_has_cargo = database.tables.AgvStates.FirstOrDefault(agv => agv.AGV_Name == taskData.From_Station).CargoStatus != 0;
                if (!taskData.bypass_eq_status_check && !agv_has_cargo)
                {
                    AlarmManagerCenter.AddAlarmAsync(ALARMS.AGV_NO_Carge_Cannot_Transfer_Cargo_From_AGV_To_Desinte, ALARM_SOURCE.AGVS, level: ALARM_LEVEL.WARNING);
                    return (false, ALARMS.AGV_NO_Carge_Cannot_Transfer_Cargo_From_AGV_To_Desinte, "AGV車上無貨，無法指派來源為AGV的搬運任務", "Not any cargo on AGV, cannot assign a transport task with AGV as the source.");
                }
                (bool confirm, ALARMS alarm_code, string message, string message_en) results = EQTransferTaskManager.CheckEQAcceptAGVType(destine_station_tag, taskData.GetToSlotInt(), taskData.DesignatedAGVName, taskData.need_change_agv);
                if (!results.confirm)
                    return results;

                var agv_name = taskData.From_Station;
                taskData.DesignatedAGVName = agv_name;
                var agv = database.tables.AgvStates.FirstOrDefault(d => d.AGV_Name == agv_name);
                taskData.From_Station = agv_name;
                sourceDeviceIDInfo.portID = agv.AGV_ID;
            }
            #endregion

            #region 充電任務確認

            if ((taskData.Action == ACTION_TYPE.Charge || taskData.Action == ACTION_TYPE.DeepCharge) && taskData.DesignatedAGVName != "")
            {
                try
                {
                    if (database.tables.AgvStates.Where(agv => agv.AGV_Name != taskData.DesignatedAGVName).Any(agv => agv.CurrentLocation == taskData.To_Station))
                    {
                        AlarmManagerCenter.AddAlarmAsync(ALARMS.Destine_Charge_Station_Has_AGV, ALARM_SOURCE.AGVS, level: ALARM_LEVEL.WARNING);
                        return (false, ALARMS.Destine_Eq_Station_Has_Task_To_Park, $"目的充電站已有AGV停駐", "The destination charging station already has an AGV parked.");
                    }
                }
                catch (Exception ex)
                {
                    LOG.Critical(ex);
                }
            }

            #endregion
            try
            {
                if (taskData.DesignatedAGVName != "")
                {
                    clsResponseBase checkReuslt = await VMSSerivces.TASK_DISPATCH.CheckOutAGVBatteryAndChargeStatus(taskData.DesignatedAGVName, taskData.Action);
                    if (!checkReuslt.confirm)
                    {
                        AlarmManagerCenter.AddAlarmAsync(ALARMS.CANNOT_DISPATCH_ORDER_BY_AGV_BAT_STATUS_CHECK, ALARM_SOURCE.AGVS, level: ALARM_LEVEL.WARNING);
                        return (false, ALARMS.CANNOT_DISPATCH_ORDER_BY_AGV_BAT_STATUS_CHECK, checkReuslt.message, checkReuslt.message);
                    }
                }

                taskData.RecieveTime = DateTime.Now;
                await Task.Delay(200);

                //起點確認
                (bool confirm, ALARMS alarmCode, string message, string message_en) GoalCheckReuslt = CheckGoalPortOrderAssignedState(taskData);
                if (GoalCheckReuslt.confirm == false)
                    return GoalCheckReuslt;


                using (var db = new AGVSDatabase())
                {
                    if (source == TASK_RECIEVE_SOURCE.LOCAL_Auto || source == TASK_RECIEVE_SOURCE.Local_MANUAL)
                    {
                        if (_order_action == ACTION_TYPE.Unload)
                            taskData.Carrier_ID = destineDeviceIDInfo.carrierIDMounted;
                        else if (_order_action == ACTION_TYPE.Carry)
                            taskData.Carrier_ID = sourceDeviceIDInfo.carrierIDMounted;
                    }

                    //test
                    //taskData.Carrier_ID ="TAE1314123123";
                    SetUpHighestPriorityState(taskData);
                    SetUpDeviceIDState(taskData, sourceDeviceIDInfo, destineDeviceIDInfo);
                    await SetUnknowCarrierID(taskData);
                    taskData.CST_TYPE = taskData.Carrier_ID.StartsWith("T") ? 200 : 201;
                    db.tables.Tasks.Add(taskData);
                    var added = await db.SaveChanges();

                    if (taskData.Action == ACTION_TYPE.DeepCharge)
                    {
                        db.tables.DeepChargeRecords.Add(new AGVSystemCommonNet6.Equipment.AGV.DeepChargeRecord()
                        {
                            AGVID = taskData.DesignatedAGVName,
                            OrderRecievedTime = DateTime.Now,
                            TaskID = taskData.TaskName,
                            OrderStatus = TASK_RUN_STATUS.WAIT,
                            TriggerBy = AGVSystemCommonNet6.Equipment.AGV.DeepChargeRecord.DEEP_CHARGE_TRIGGER_MOMENT.MANUAL,
                        });
                        await db.SaveChanges();
                    }

                }
            }
            catch (Exception ex)
            {
                LOG.ERROR(ex);
                AlarmManagerCenter.AddAlarmAsync(ALARMS.Task_Add_To_Database_Fail, ALARM_SOURCE.AGVS);
                return new(false, ALARMS.Task_Add_To_Database_Fail, ex.Message, ex.Message);
            }
            try
            {
                if (taskData.Action == ACTION_TYPE.Unload || taskData.Action == ACTION_TYPE.Load || taskData.Action == ACTION_TYPE.LoadAndPark || taskData.Action == ACTION_TYPE.Carry)
                    if (taskData.Carrier_ID != string.Empty)
                        MaterialManager.CreateMaterialInfo(taskData.Carrier_ID, materialCondition: MaterialCondition.Wait, TaskSource: taskData.From_Station, TaskTarget: taskData.To_Station);
                return new(true, ALARMS.NONE, "", "");
            }
            catch (Exception ex)
            {
                LOG.ERROR(ex);
                AlarmManagerCenter.AddAlarmAsync(ALARMS.Task_Add_To_Database_Fail, ALARM_SOURCE.AGVS);
                return new(false, ALARMS.Task_Add_To_Database_Fail, ex.Message, ex.Message);
            }
        }

        private static (bool confirm, ALARMS alarmCode, string message, string message_en) CheckGoalPortOrderAssignedState(clsTaskDto taskData)
        {
            ACTION_TYPE _order_action = taskData.Action;
            List<string> currentNotUsableGoalKeyList = new List<string>(DatabaseCaches.TaskCaches.InCompletedTasks.Count);
            foreach (var task in DatabaseCaches.TaskCaches.InCompletedTasks)
            {
                currentNotUsableGoalKeyList.Add($"{task.To_Station}_{task.To_Slot}");
                if (task.currentProgress == VehicleMovementStage.Not_Start_Yet || task.currentProgress == VehicleMovementStage.Traveling_To_Source || task.currentProgress == VehicleMovementStage.WorkingAtSource)
                    currentNotUsableGoalKeyList.Add($"{task.From_Station}_{task.From_Slot}");
            }

            //來源確認

            if (_order_action == ACTION_TYPE.Carry)
            {
                string sourceKey = $"{taskData.From_Station}_{taskData.From_Slot}";
                if (currentNotUsableGoalKeyList.Contains(sourceKey))
                {
                    AlarmManagerCenter.AddAlarmAsync(ALARMS.Destine_Eq_Already_Has_Task_To_Excute, ALARM_SOURCE.AGVS);
                    return (false, ALARMS.Destine_Eq_Already_Has_Task_To_Excute, $"來源設備已有搬運任務", "The source equipment already has a carry task.");
                }
            }
            //終點確認
            string destineKey = $"{taskData.To_Station}_{taskData.To_Slot}";
            if (currentNotUsableGoalKeyList.Contains(destineKey))
            {
                AlarmManagerCenter.AddAlarmAsync(ALARMS.Destine_Eq_Already_Has_Task_To_Excute, ALARM_SOURCE.AGVS);
                return (false, ALARMS.Destine_Eq_Already_Has_Task_To_Excute, $"目的地已被指派任務", "The goal already has assigned any task.");
            }
            return (true, ALARMS.NONE, "", "");
        }

        private static async Task SetUnknowCarrierID(clsTaskDto taskData)
        {

            if (taskData.Action == ACTION_TYPE.None || taskData.Action == ACTION_TYPE.Charge || taskData.Action == ACTION_TYPE.DeepCharge
                || taskData.Action == ACTION_TYPE.Measure || taskData.Action == ACTION_TYPE.Discharge || taskData.Action == ACTION_TYPE.Unpark)
                return;

            if (!string.IsNullOrEmpty(taskData.Carrier_ID))
                return;

            int flowNumber = 0;
            string unknowCargoID = "";
            if (taskData.CST_TYPE == 200 || taskData.CST_TYPE == 0)
                taskData.Carrier_ID = await AGVSConfigulator.GetTrayUnknownFlowID();
            else
                taskData.Carrier_ID = await AGVSConfigulator.GetRackUnknownFlowID();
        }


        private static void SetUpDeviceIDState(clsTaskDto taskData, DeviceIDInfo sourceDeviceIDInfo, DeviceIDInfo destineDeviceIDInfo)
        {
            taskData.soucePortID = sourceDeviceIDInfo.portID;
            taskData.sourceZoneID = sourceDeviceIDInfo.zoneID;
            taskData.destinePortID = destineDeviceIDInfo.portID;
            taskData.destineZoneID = destineDeviceIDInfo.zoneID;
        }

        private static void SetUpHighestPriorityState(clsTaskDto taskData)
        {
            MapPoint? sourcePt = AGVSMapManager.GetMapPointByTag(taskData.From_Station_Tag);
            MapPoint? destinePt = AGVSMapManager.GetMapPointByTag(taskData.To_Station_Tag);

            bool isSourceStationHighestPriority = sourcePt == null ? false : sourcePt.IsHighestPriorityStation;
            bool isDestineStationHighestPriority = destinePt == null ? false : destinePt.IsHighestPriorityStation;
            taskData.IsHighestPriorityTask = isSourceStationHighestPriority || isDestineStationHighestPriority;
            taskData.Priority = taskData.IsHighestPriorityTask ? 9999999 : taskData.Priority;
        }

        public static (bool confirm, int alarm_code, string message) CheckChargeTask(string agv_name, int assign_charge_station_tag)
        {
            try
            {

                IEnumerable<int> useableChargeTags = StaEQPManagager.GetUsableChargeStationTags(agv_name);
                IEnumerable<AGVSystemCommonNet6.MAP.MapPoint> chargeStations = AGVSMapManager.CurrentMap.Points.Values.Where(point => point.IsCharge && useableChargeTags.Contains(point.TagNumber));

                bool isNoChargeStation = chargeStations.Count() == 0;

                bool isUnspecified = assign_charge_station_tag == -1;
                if (isNoChargeStation)
                    return (false, 1008, "當前地圖上沒有充電站可以使用");

                var chargeTasks = DatabaseCaches.TaskCaches.InCompletedTasks.Where(_task => _task.Action == ACTION_TYPE.Charge || _task.Action == ACTION_TYPE.DeepCharge);

                bool isAlreadyHasChargeTask = chargeTasks.Any(_task => _task.DesignatedAGVName == agv_name);
                if (isAlreadyHasChargeTask)
                    return (false, 1082, "AGV已有充電任務");

                if (!isUnspecified) //有指定充電站
                {
                    if (!useableChargeTags.Contains(assign_charge_station_tag))
                    {
                        return (false, 61, $"該充電站不允許{agv_name}使用");
                    }
                    bool isChargeStationHasTask = chargeTasks.Count() == 0 ? false : chargeTasks.Where(_task => _task.DesignatedAGVName != agv_name).Any(tk => tk.To_Station_Tag == assign_charge_station_tag);
                    bool isAnyAGVInTheChargeStation = DatabaseCaches.Vehicle.VehicleStates.Where(agv => agv.AGV_Name != agv_name).Any(agv => agv.CurrentLocation == assign_charge_station_tag + "");

                    if (isChargeStationHasTask)
                        return (false, 1082, "已有任務指派AGV前往此充電站");

                    if (isAnyAGVInTheChargeStation)
                        return (false, 1084, "已有AGV停駐在此充電站");

                    return (true, 0, "");
                }
                else //沒有指定充電站:無充電站可以用的情境:1. 所有充電站都有AGV(除了自己)
                {

                    string agv_currnet_tag = DatabaseCaches.Vehicle.VehicleStates.First(agv => agv.AGV_Name == agv_name).CurrentLocation;
                    string[] other_agv_current_tag = DatabaseCaches.Vehicle.VehicleStates.Where(agv => agv.AGV_Name != agv_name).Select(agv => agv.CurrentLocation).ToArray();

                    List<int> chargeStationTags = chargeStations.Where(station => useableChargeTags.Contains(station.TagNumber)).Select(station => station.TagNumber).ToList();

                    bool isAGVInChargeStation = chargeStationTags.Any(tag => tag + "" == agv_currnet_tag);
                    if (isAGVInChargeStation)
                        return (true, 0, "");

                    IEnumerable<int> usableChargeStationTags = chargeStationTags.Where(tag => !other_agv_current_tag.Contains(tag + ""));

                    bool hasChargeStationUse = usableChargeStationTags.Count() > 0;
                    if (!hasChargeStationUse)
                        return (false, 1008, "沒有空閒的充電站可以使用");

                    bool isAllChargeStationsHasTask = chargeTasks.Where(tk => tk.DesignatedAGVName == agv_name).Count() == usableChargeStationTags.Count();
                    if (isAllChargeStationsHasTask)
                        return (false, 1008, "沒有空閒的充電站可以使用");

                    return (true, 0, "");
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }


        }

        internal static async Task<(bool confirm, string message)> CancelChargeTaskByAGVAsync(string agv_name)
        {
            var db = new AGVSDatabase();
            var charge_task = db.tables.Tasks.FirstOrDefault(_task => (_task.State == TASK_RUN_STATUS.NAVIGATING || _task.State == TASK_RUN_STATUS.WAIT) && _task.DesignatedAGVName == agv_name);
            if (charge_task == null)
                return (false, "Charge Task Not Found");
            bool cancel_success = await Cancel(charge_task.TaskName, "User Cancel");
            return (cancel_success, cancel_success ? "" : "任務取消失敗");
        }
        internal async static Task<bool> Cancel(string task_name, string reason = "", TASK_RUN_STATUS status = TASK_RUN_STATUS.CANCEL, string hostAction = "cancel")
        {
            try
            {

                bool isOrderWaiting = DatabaseCaches.TaskCaches.WaitExecuteTasks.Any(order => order.TaskName == task_name);

                using (var db = new AGVSDatabase())
                {
                    var order = db.tables.Tasks.FirstOrDefault(order => order.TaskName == task_name);
                    if (order != null && (!order.DesignatedAGVName.IsNullOrEmpty() || order.DesignatedAGVName != "-1"))
                    {
                        clsAGVStateDto vehicleState = DatabaseCaches.Vehicle.VehicleStates.FirstOrDefault(agvState => agvState.AGV_Name == order.DesignatedAGVName);
                        if (vehicleState != null && (vehicleState.OnlineStatus == clsEnums.ONLINE_STATE.OFFLINE || vehicleState.MainStatus == clsEnums.MAIN_STATUS.DOWN))
                        {
                            order.State = status;
                            order.FinishTime = DateTime.Now;
                            order.FailureReason = reason;
                            await db.SaveChanges();
                            return true;
                        }
                    }
                }

                DatabaseCaches.TaskCaches.WaitExecuteTasks.Any(order => order.TaskName == task_name);

                if (isOrderWaiting)
                {
                    MCSCIMService.TransportCommandDto transportCommandDto = new TransportCommandDto();
                    await VMSSerivces.TaskCancel(task_name, reason, hostAction);
                    using (var agvsDb = new AGVSDatabase())
                    {
                        var dto = agvsDb.tables.Tasks.FirstOrDefault(od => od.TaskName == task_name);
                        if (dto != null)
                        {
                            transportCommandDto = new MCSCIMService.TransportCommandDto
                            {
                                CommandID = dto.TaskName,
                                CarrierID = dto.Carrier_ID,
                                CarrierLoc = dto.soucePortID,
                                CarrierZoneName = dto.sourceZoneID,
                            };
                            dto.State = TASK_RUN_STATUS.CANCEL;
                            dto.FailureReason = reason;
                            dto.FinishTime = DateTime.Now;
                        }
                        await agvsDb.SaveChanges();
                    }

                    MCSCIMService.TransferAbortCompletedReport(transportCommandDto);
                    return true;
                }

                await VMSSerivces.TaskCancel(task_name, reason, hostAction);
                //using (var db = new AGVSDatabase())
                //{
                //    var task = db.tables.Tasks.Where(tk => tk.TaskName == task_name).FirstOrDefault();
                //    if (task != null)
                //    {

                //        if (task.Action == ACTION_TYPE.Carry)
                //        {
                //            if (EQTransferTaskManager.MonitoringCarrerTasks.Remove(task_name, out clsLocalAutoTransferTaskMonitor monitor))
                //            {
                //                monitor.sourceEQ.CancelReserve();
                //                monitor.destineEQ.CancelReserve();
                //            }
                //        }
                //        task.FinishTime = DateTime.Now;
                //        task.FailureReason = reason;
                //        task.State = status;
                //        await db.SaveChanges();
                //    }
                //}
                return true;
            }
            catch (Exception ex)
            {
                AlarmManagerCenter.AddAlarmAsync(ALARMS.Task_Cancel_Fail);
                return false;
            }

        }


        internal static bool TaskStatusChangeToWait(string task_name, string reason = "")
        {
            LOG.TRACE($"Change Task-{task_name} Status = Wait.[Reason:{reason}]");
            try
            {
                using (var db = new AGVSDatabase())
                {
                    var task = db.tables.Tasks.Where(tk => tk.TaskName == task_name).FirstOrDefault();
                    if (task != null)
                    {
                        task.FailureReason = "";
                        task.State = TASK_RUN_STATUS.WAIT;
                        db.SaveChanges();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                LOG.Critical(ex);
                return false;
            }

        }

    }
}
