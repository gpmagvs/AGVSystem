using AGVSystem.Service;
using AGVSystemCommonNet6.Alarm;
using AGVSystemCommonNet6.Log;
using AGVSystemCommonNet6.Material;
using EquipmentManagment.ChargeStation;
using EquipmentManagment.Device;
using EquipmentManagment.WIP;

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

            clsPortOfRack.OnRackPortSensorFlash += HandlePortOfRackSensorFlash;
            clsPortOfRack.OnRackPortSensorStatusChanged += HandlePortOfRackSensorStatusChanged;

            MaterialManagerEventHandler.OnMaterialTransferStatusChange += HandleMaterialTransferStatusChanged;
            MaterialManagerEventHandler.OnMaterialAdd += HandleMaterialAdd;
            MaterialManagerEventHandler.OnMaterialDelete += HandleMaterialDelete;
        }

        private static void HandleMaterialDelete(object? sender, clsMaterialInfo e)
        {
            MaterialManager.DeleteMaterialInfo(e.MaterialID, e.SourceStation, e.InstallStatus, e.IDStatus, e.Type);
        }

        private static void HandleMaterialAdd(object? sender, clsMaterialInfo e)
        {
            MaterialManager.AddMaterialInfo(e.MaterialID, e.TargetStation, e.InstallStatus, e.IDStatus, e.Type, e.Condition);
        }

        private static void HandleMaterialTransferStatusChanged(object? sender, clsMaterialInfo e)
        {
            MaterialManager.CreateMaterialInfo(e.MaterialID, e.Type, e.ActualID, e.SourceStation, e.TargetStation, e.TaskSourceStation, e.TaskTargetStation, e.InstallStatus, e.IDStatus, e.Condition);
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
