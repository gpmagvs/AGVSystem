using AGVSystem.Service;
using AGVSystemCommonNet6.Alarm;
using AGVSystemCommonNet6.Configuration;
using AGVSystemCommonNet6.Log;
using AGVSystemCommonNet6.Notify;
using EquipmentManagment.ChargeStation;
using EquipmentManagment.Device;
using EquipmentManagment.WIP;

namespace AGVSystem.Models.EQDevices
{
    public partial class EQDeviceEventsHandler
    {
        private static bool _disableEntryPointWhenEQPartsReplacing = AGVSConfigulator.SysConfigs.EQManagementConfigs.DisableEntryPointWhenEQPartsReplacing;

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

            clsPortOfRack.OnRackPortSensorFlash += HandlePortOfRackSensorFlash;
            clsPortOfRack.OnRackPortSensorStatusChanged += HandlePortOfRackSensorStatusChanged;
        }

        private static void HandleEQFinishPartsReplace(object? sender, EndPointDeviceAbstract device)
        {
            var tagOfEQInPartsReplacing = device.EndPointOptions.TagID;
            if (_disableEntryPointWhenEQPartsReplacing)
            {
                NotifyServiceHelper.SUCCESS($"設備-{device.EQName} 結束零件更換作業，開啟道路!");
                ChangeEnableStateOfEntryPointOfEQOfMapAndRequestVMSReload(device.EndPointOptions.TagID, true);
            }
            AGVSystemCommonNet6.Microservices.VMS.VMSSerivces.RemovePartsReplaceworkstationTag(tagOfEQInPartsReplacing);
        }

        private static void HandleEQStartPartsReplace(object? sender, EndPointDeviceAbstract device)
        {
            var tagOfEQInPartsReplacing = device.EndPointOptions.TagID;
            if (_disableEntryPointWhenEQPartsReplacing)
            {
                NotifyServiceHelper.SUCCESS($"設備-{device.EQName} 開始零件更換作業，封閉道路!");
                ChangeEnableStateOfEntryPointOfEQOfMapAndRequestVMSReload(device.EndPointOptions.TagID, false);
            }
            AGVSystemCommonNet6.Microservices.VMS.VMSSerivces.AddPartsReplaceworkstationTag(tagOfEQInPartsReplacing);
        }

        private static void HandleEQInputDataSizeNotEnough(object? sender, EndPointDeviceAbstract device)
        {
            _ = Task.Factory.StartNew(async () =>
            {
                await AlarmManagerCenter.ResetAlarmAsync(new clsAlarmDto() { AlarmCode = (int)ALARMS.EQ_Input_Data_Not_Enough }, false);
                await AlarmManagerCenter.AddAlarmAsync(ALARMS.EQ_Input_Data_Not_Enough, source: ALARM_SOURCE.EQP, Equipment_Name: device.EQName);
            });
        }

        internal static async void HandleDeviceDisconnected(object? sender, EndPointDeviceAbstract device)
        {
            _Log($"EQ-{device.EQName} 連線中斷({device.EndPointOptions.ConnOptions.IP}:{device.EndPointOptions.ConnOptions.Port}-{device.EndPointOptions.ConnOptions.ConnMethod})", device.EQName);

            _ = Task.Run(async () =>
            {
                await AlarmManagerCenter.AddAlarmAsync(ALARMS.EQ_Disconnect, source: ALARM_SOURCE.EQP, Equipment_Name: device.EQName);
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
