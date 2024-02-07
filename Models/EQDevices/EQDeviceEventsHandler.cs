using AGVSystemCommonNet6.Alarm;
using AGVSystemCommonNet6.Log;
using EquipmentManagment.Device;

namespace AGVSystem.Models.EQDevices
{
    public class EQDeviceEventsHandler
    {

        internal static void Initialize()
        {
            EndPointDeviceAbstract.OnEQDisconnected += HandleDeviceDisconnected;
            EndPointDeviceAbstract.OnEQConnected += HandleDeviceReconnected;
            EndPointDeviceAbstract.OnEQInputDataSizeNotEnough += HandleEQInputDataSizeNotEnough;

        }

        private static void HandleEQInputDataSizeNotEnough(object? sender, EndPointDeviceAbstract device)
        {
            _ = Task.Factory.StartNew(async () =>
            {
                await AlarmManagerCenter.ResetAlarmAsync(new clsAlarmDto() { AlarmCode = (int)ALARMS.EQ_Input_Data_Not_Enough }, false);
                await AlarmManagerCenter.AddAlarmAsync(ALARMS.EQ_Input_Data_Not_Enough, source: ALARM_SOURCE.EQP, Equipment_Name: device.EQName);
            });
        }

        internal static void HandleDeviceDisconnected(object? sender, EndPointDeviceAbstract device)
        {
            _Log($"EQ-{device.EQName} 連線中斷({device.EndPointOptions.ConnOptions.IP}-{device.EndPointOptions.ConnOptions.ConnMethod})", device.EQName);
            _ = Task.Factory.StartNew(async () =>
            {
                await AlarmManagerCenter.AddAlarmAsync(ALARMS.EQ_Disconnect, source: ALARM_SOURCE.EQP, Equipment_Name: device.EQName);
            });
        }

        internal static void HandleDeviceReconnected(object? sender, EndPointDeviceAbstract device)
        {
            _Log($"EQ-{device.EQName} 已連線({device.EndPointOptions.ConnOptions.IP}-{device.EndPointOptions.ConnOptions.ConnMethod})", device.EQName);
            _ = Task.Factory.StartNew(async () =>
            {
                await AlarmManagerCenter.SetAlarmCheckedAsync(device.EQName, ALARMS.EQ_Disconnect, "SystemAuto");
            });
        }

        internal static void HandleEQIOStateChanged(object? sender, EndPointDeviceAbstract.IOChangedEventArgs device)
        {
            _Log($"[{device.Device.EQName}] IO-{device.IOName} Changed To {(device.IOState ? "1" : "0")}", device.Device.EQName);
        }


        private static void _Log(string logMessage, string eqName)
        {
            using LogBase _logger = new LogBase(@$"AGVS LOG\EquipmentManager\{eqName}");
            _logger.WriteLog(new LogItem(LogLevel.Information, logMessage, true)
            {
            });
        }
    }

}
