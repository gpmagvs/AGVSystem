using AGVSystemCommonNet6.Alarm;
using AGVSystemCommonNet6.Alarm.VMS_ALARM;
using EquipmentManagment.Device;

namespace AGVSystem.Models.EQDevices
{
    public class EQDeviceEventsHandler
    {
        internal static void HandleDeviceDisconnected(object? sender, EndPointDeviceAbstract device)
        {
            AlarmManagerCenter.AddAlarm(ALARMS.EQ_Disconnect, source: ALARM_SOURCE.EQP, Equipment_Name: device.EQName);
        }

        internal static async void HandleDeviceReconnected(object? sender, EndPointDeviceAbstract device)
        {
            await AlarmManagerCenter.SetAlarmCheckedAsync(device.EQName, ALARMS.EQ_Disconnect, "SystemAuto");
        }
    }

}
