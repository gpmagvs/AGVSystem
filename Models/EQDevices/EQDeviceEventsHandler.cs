using AGVSystemCommonNet6.Alarm;
using AGVSystemCommonNet6.Log;
using EquipmentManagment.Device;

namespace AGVSystem.Models.EQDevices
{
    public class EQDeviceEventsHandler
    {
        internal static void HandleDeviceDisconnected(object? sender, EndPointDeviceAbstract device)
        {
            AlarmManagerCenter.AddAlarmAsync(ALARMS.EQ_Disconnect, source: ALARM_SOURCE.EQP, Equipment_Name: device.EQName);
        }

        internal static async void HandleDeviceReconnected(object? sender, EndPointDeviceAbstract device)
        {
            await AlarmManagerCenter.SetAlarmCheckedAsync(device.EQName, ALARMS.EQ_Disconnect, "SystemAuto");
        }

        internal static void HandleEQIOStateChanged(object? sender, EndPointDeviceAbstract.IOChangedEventArgs e)
        {
            LOG.INFO($"EQ-{e.Device.EQName}|IO-{e.IOName} Changed To {e.IOState}");
        }
    }

}
