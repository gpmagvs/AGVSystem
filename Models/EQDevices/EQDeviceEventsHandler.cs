using AGVSystemCommonNet6.Alarm;
using AGVSystemCommonNet6.Log;
using EquipmentManagment.Device;

namespace AGVSystem.Models.EQDevices
{
    public class EQDeviceEventsHandler
    {
        internal static void HandleDeviceDisconnected(object? sender, EndPointDeviceAbstract device)
        {
            LOG.ERROR($"EQ-{device.EQName} 連線中斷({device.EndPointOptions.ConnOptions.IP}-{device.EndPointOptions.ConnOptions.ConnMethod})");
            AlarmManagerCenter.AddAlarmAsync(ALARMS.EQ_Disconnect, source: ALARM_SOURCE.EQP, Equipment_Name: device.EQName);
        }

        internal static async void HandleDeviceReconnected(object? sender, EndPointDeviceAbstract device)
        {
            LOG.INFO($"EQ-{device.EQName} 已連線({device.EndPointOptions.ConnOptions.IP}-{device.EndPointOptions.ConnOptions.ConnMethod})", color: ConsoleColor.Green);
            await AlarmManagerCenter.SetAlarmCheckedAsync(device.EQName, ALARMS.EQ_Disconnect, "SystemAuto");
        }

        internal static void HandleEQIOStateChanged(object? sender, EndPointDeviceAbstract.IOChangedEventArgs e)
        {
            LOG.TRACE($"EQ-{e.Device.EQName}|IO-{e.IOName} Changed To {(e.IOState ? "1" : "0")}");
        }
    }

}
