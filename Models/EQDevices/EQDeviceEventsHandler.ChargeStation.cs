using EquipmentManagment.ChargeStation;
using EquipmentManagment.Device;
using AGVSystemCommonNet6.DATABASE.BackgroundServices;
using AGVSystemCommonNet6.DATABASE;
using AGVSystem.TaskManagers;
using AGVSystemCommonNet6.Notify;
using AGVSystemCommonNet6.Alarm;
using AGVSystem.Models.Map;
using AGVSystemCommonNet6.MAP;
namespace AGVSystem.Models.EQDevices
{
    public partial class EQDeviceEventsHandler
    {
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

                NotifyServiceHelper.WARNING($"{agvName} 偵測到電池未連接!\r\n即將重新下發充電任務...");
                AlarmManagerCenter.AddAlarmAsync(ALARMS.Battery_Not_Connect, ALARM_SOURCE.AGVS, ALARM_LEVEL.WARNING, chargeStation.EQName, mapPt.Graph.Display);
                (bool confirm, ALARMS alarm_code, string message) AddTaskResult = await TaskManager.AddTask(new AGVSystemCommonNet6.AGVDispatch.clsTaskDto
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
    }
}
