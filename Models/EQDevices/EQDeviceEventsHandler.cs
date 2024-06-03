using AGVSystem.Service;
using AGVSystemCommonNet6.Alarm;
using AGVSystemCommonNet6.Log;
using EquipmentManagment.ChargeStation;
using EquipmentManagment.Device;

namespace AGVSystem.Models.EQDevices
{
    public partial class EQDeviceEventsHandler
    {
        internal static void Initialize()
        {
            EndPointDeviceAbstract.OnEQDisconnected += HandleDeviceDisconnected;
            EndPointDeviceAbstract.OnEQConnected += HandleDeviceReconnected;
            EndPointDeviceAbstract.OnEQInputDataSizeNotEnough += HandleEQInputDataSizeNotEnough;
            EndPointDeviceAbstract.OnPartsStartReplacing += HandleEQStartPartsReplace;
            EndPointDeviceAbstract.OnPartsEndReplacing += HandleEQFinishPartsReplace;
            EndPointDeviceAbstract.OnDeviceMaintainStart += HandleDeviceMaintainStart;
            EndPointDeviceAbstract.OnDeviceMaintainFinish += HandleDeviceMaintainFinish;

            clsChargeStation.OnBatteryNotConnected += HandleChargeStationBatteryNotConnectEvent;
            clsChargeStation.OnBatteryChargeFull += HandleChargeFullEvent;

        }


        private static void HandleEQFinishPartsReplace(object? sender, EndPointDeviceAbstract e)
        {

            var tagOfEQInPartsReplacing = e.EndPointOptions.TagID;
            AGVSystemCommonNet6.Microservices.VMS.VMSSerivces.RemovePartsReplaceworkstationTag(tagOfEQInPartsReplacing);
        }

        private static void HandleEQStartPartsReplace(object? sender, EndPointDeviceAbstract e)
        {
            var tagOfEQInPartsReplacing = e.EndPointOptions.TagID;
            AGVSystemCommonNet6.Microservices.VMS.VMSSerivces.AddPartsReplaceworkstationTag(tagOfEQInPartsReplacing);
        }

        private static void HandleEQInputDataSizeNotEnough(object? sender, EndPointDeviceAbstract device)
        {
            _ = Task.Factory.StartNew(async () =>
            {
                await AlarmManagerCenter.ResetAlarmAsync(new clsAlarmDto() { AlarmCode = (int)ALARMS.EQ_Input_Data_Not_Enough }, false);
                await AlarmManagerCenter.AddAlarmAsync(1081, source: ALARM_SOURCE.EQP, Equipment_Name: device.EQName);
            });
        }

        internal static async void HandleDeviceDisconnected(object? sender, EndPointDeviceAbstract device)
        {
            _Log($"EQ-{device.EQName} 連線中斷({device.EndPointOptions.ConnOptions.IP}:{device.EndPointOptions.ConnOptions.Port}-{device.EndPointOptions.ConnOptions.ConnMethod})", device.EQName);

            _ = Task.Run(async () =>
            {
                await AlarmManagerCenter.AddAlarmAsync(1068, source: ALARM_SOURCE.EQP, Equipment_Name: device.EQName);
            });
        }

        internal static async void HandleDeviceReconnected(object? sender, EndPointDeviceAbstract device)
        {
            _Log($"EQ-{device.EQName} 已連線({device.EndPointOptions.ConnOptions.IP}-{device.EndPointOptions.ConnOptions.ConnMethod})", device.EQName);
            _ = Task.Run(async () =>
            {
                await AlarmManagerCenter.SetAlarmCheckedAsync(device.EQName, ALARMS.EQ_Disconnect, "SystemAuto");
            });
        }

        internal static async void HandleEQIOStateChanged(object? sender, EndPointDeviceAbstract.IOChangedEventArgs device)
        {
            _Log($"[{device.Device.EQName}] IO-{device.IOName} Changed To {(device.IOState ? "1" : "0")}", device.Device.EQName);
        }

        private static void _Log(string logMessage, string eqName)
        {
            using LogBase _logger = new LogBase(@$"AGVS LOG\EquipmentManager\{eqName}");
            _logger.WriteLogAsync(new LogItem(LogLevel.Information, logMessage, true)
            {
            });
        }
    }

}
