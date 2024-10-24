using EquipmentManagment.ChargeStation;
using EquipmentManagment.Device;
using AGVSystemCommonNet6.DATABASE.BackgroundServices;
using AGVSystemCommonNet6.DATABASE;
using AGVSystem.TaskManagers;
using AGVSystemCommonNet6.Notify;
using AGVSystemCommonNet6.Alarm;
using AGVSystem.Models.Map;
using AGVSystemCommonNet6.MAP;
using AGVSystemCommonNet6.AGVDispatch;
using AGVSystemCommonNet6.Configuration;
using AGVSystemCommonNet6.Microservices.AudioPlay;
namespace AGVSystem.Models.EQDevices
{
    public partial class EQDeviceEventsHandler
    {

        private static string ChargerSmokeDetectedAudioFilePath => Path.Combine(AGVSConfigulator.ConfigsFilesFolder, "Sounds/charger-smoke-detected-alarm.mp3");
        private static string ChargerAirErrorAudioFilePath => Path.Combine(AGVSConfigulator.ConfigsFilesFolder, "Sounds/charger-air-error-alarm.mp3");
        private static string ChargerEMOAudioFilePath => Path.Combine(AGVSConfigulator.ConfigsFilesFolder, "Sounds/charger-emo-alarm.mp3");

        /// <summary>
        /// 處理充電站偵測到電池未連接的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="chargeStation"></param>
        private static async void HandleChargeStationBatteryNotConnectEvent(object? sender, clsChargeStation chargeStation)
        {
            int tag = chargeStation.EndPointOptions.TagID;
            AGVSystemCommonNet6.clsAGVStateDto? agvState = DatabaseCaches.Vehicle.VehicleStates.FirstOrDefault(vehicle => vehicle.CurrentLocation == tag.ToString());
            if (agvState != null)
            {
                string agvName = agvState.AGV_Name;
                MapPoint mapPt = AGVSMapManager.GetMapPointByTag(tag);

                //判斷如果不是因為充電任務停到充電站 則返回

                clsTaskDto _task = DatabaseCaches.TaskCaches.RunningTasks.FirstOrDefault(task => task.State == AGVSystemCommonNet6.AGVDispatch.Messages.TASK_RUN_STATUS.NAVIGATING && task.Action != AGVSystemCommonNet6.AGVDispatch.Messages.ACTION_TYPE.Charge);

                if (_task != null)
                    return;

                NotifyServiceHelper.WARNING($"{agvName} 偵測到電池未連接!\r\n即將重新下發充電任務...");
                AlarmManagerCenter.AddAlarmAsync(ALARMS.Battery_Not_Connect, ALARM_SOURCE.AGVS, ALARM_LEVEL.WARNING, chargeStation.EQName, mapPt.Graph.Display);
                (bool confirm, ALARMS alarm_code, string message, string message_en) AddTaskResult = await TaskManager.AddTask(new AGVSystemCommonNet6.AGVDispatch.clsTaskDto
                {
                    TaskName = $"Charge-{DateTime.Now.ToString("yyyyMMdd_HHmmssfff")}",//20240529_090159358
                    Action = AGVSystemCommonNet6.AGVDispatch.Messages.ACTION_TYPE.Charge,
                    To_Station = tag.ToString(),
                    To_Slot = "0",
                    DesignatedAGVName = agvName,
                    DispatcherName = "AGVS-ReCharge"
                }, TaskManager.TASK_RECIEVE_SOURCE.LOCAL);

                if (AddTaskResult.confirm)
                {
                    NotifyServiceHelper.SUCCESS($"{agvName} 充電任務已重新下發");
                }
                else
                {
                    NotifyServiceHelper.ERROR($"{agvName} 充電任務重新下發失敗 :\r\n{AddTaskResult.message}");
                }
            }
        }

        private static void HandleChargeFullEvent(object? sender, clsChargeStation e)
        {
            (int ChargeStationTag, MapPoint StationMapPoint, string UsingChargeStationVehicleName) = _GetChargeStationAndVehicle(e);
            NotifyServiceHelper.SUCCESS($"{UsingChargeStationVehicleName} 已充飽電!({StationMapPoint.Graph.Display})");
        }



        private static void HandleChargeStationTemperatureOverThreshoad(object? sender, clsChargeStation e)
        {
            (int ChargeStationTag, MapPoint StationMapPoint, string UsingChargeStationVehicleName) = _GetChargeStationAndVehicle(e);
            string chargerName = e.EQName;
            AlarmManagerCenter.AddAlarmAsync(ALARMS.Charge_Station_Temperature_High, ALARM_SOURCE.EQP, Equipment_Name: chargerName, location: chargerName);
        }
        private static void HandleChargeStationTemperatureRestoreUnderThreshoad(object? sender, clsChargeStation e)
        {
            (int ChargeStationTag, MapPoint StationMapPoint, string UsingChargeStationVehicleName) = _GetChargeStationAndVehicle(e);
            string chargerName = e.EQName;
            AlarmManagerCenter.SetAlarmCheckedAsync(chargerName, ALARMS.Charge_Station_Temperature_High);
        }


        private static (int ChargeStationTag, MapPoint StationMapPoint, string UsingChargeStationVehicleName) _GetChargeStationAndVehicle(clsChargeStation chargeStation)
        {
            int tag = chargeStation.EndPointOptions.TagID;
            MapPoint mapPt = AGVSMapManager.GetMapPointByTag(tag);
            AGVSystemCommonNet6.clsAGVStateDto? agvState = DatabaseCaches.Vehicle.VehicleStates.FirstOrDefault(vehicle => vehicle.CurrentLocation == tag.ToString());
            if (agvState != null)
            {
                string agvName = agvState.AGV_Name;
                return (tag, mapPt, agvName);
            }
            else
            {
                return (tag, mapPt, "");
            }
        }


        private static void ChargerIOSynchronizer_OnSmokeDetected(object? sender, string chargerName)
        {
            AudioPlayService.AddAudioToPlayQueue(ChargerSmokeDetectedAudioFilePath);
            AlarmManagerCenter.AddAlarmAsync(ALARMS.Charge_Station_Smoke_Detected, Equipment_Name: chargerName);
        }

        private static void ChargerIOSynchronizer_OnAirError(object? sender, string chargerName)
        {
            AudioPlayService.AddAudioToPlayQueue(ChargerAirErrorAudioFilePath);
            AlarmManagerCenter.AddAlarmAsync(ALARMS.Charge_Station_Air_Error, Equipment_Name: chargerName);
        }

        private static void ChargerIOSynchronizer_OnEMO(object? sender, string chargerName)
        {
            AudioPlayService.AddAudioToPlayQueue(ChargerEMOAudioFilePath);
            AlarmManagerCenter.AddAlarmAsync(ALARMS.Charge_Station_EMO, Equipment_Name: chargerName);
        }
    }
}
