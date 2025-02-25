﻿using AGVSystem.Models.Map;
using AGVSystemCommonNet6.Configuration;
using AGVSystemCommonNet6.MAP;
using AGVSystemCommonNet6.Microservices;
using AGVSystemCommonNet6.Notify;
using EquipmentManagment.Device;

namespace AGVSystem.Models.EQDevices
{
    public partial class EQDeviceEventsHandler
    {
        private static bool _disableEntryPointWhenEQMaintaining => AGVSConfigulator.SysConfigs.EQManagementConfigs.DisableEntryPointWhenEQMaintaining;
        private static SemaphoreSlim _semaphoreSlim = new(1, 1);
        private static void HandleDeviceMaintainFinish(object? sender, EndPointDeviceAbstract device)
        {
            _logger.Trace($"{device.EQName} Maintain Signal OFF");
            NotifyServiceHelper.SUCCESS($"設備-{device.EQName} 完成維修");
            if (_disableEntryPointWhenEQMaintaining)
                ChangeEnableStateOfEntryPointOfEQOfMapAndRequestVMSReload(device.EndPointOptions.TagID, true);
        }

        private static void HandleDeviceMaintainStart(object? sender, EndPointDeviceAbstract device)
        {
            _logger.Trace($"{device.EQName} Maintain Signal ON");
            NotifyServiceHelper.WARNING($"設備-{device.EQName} 維修中!");
            if (_disableEntryPointWhenEQMaintaining)
                ChangeEnableStateOfEntryPointOfEQOfMapAndRequestVMSReload(device.EndPointOptions.TagID, false);
        }


        private static async Task ChangeEnableStateOfEntryPointOfEQOfMapAndRequestVMSReload(int deviceTag, bool enabled)
        {
            try
            {
                await _semaphoreSlim.WaitAsync();
                MapPoint deviceMapPoint = AGVSMapManager.CurrentMap.Points.Values.FirstOrDefault(pt => pt.TagNumber == deviceTag);
                if (deviceMapPoint == null)
                {
                    _logger.Warn($"[EQDeviceEventsHandler] Cannot find Map Point that TagNumber equal {deviceTag}");
                    return;
                }
                IEnumerable<MapPoint> entryPoints = deviceMapPoint.Target.Keys.Select(_index => AGVSMapManager.CurrentMap.Points[_index]);
                if (!entryPoints.Any())
                    return;

                foreach (MapPoint point in entryPoints)
                {
                    point.Enable = enabled;
                }
                string _mapFilePath = AGVSConfigulator.SysConfigs.PATHES_STORE[SystemConfigs.PATH_ENUMS.CURRENT_MAP_FILE_PATH];
                MapManager.SaveMapToFile(AGVSMapManager.CurrentMap, _mapFilePath);
                await NotifyServiceHelper.INFO($"Map-Point-Enabled-Property-Changed", false);
                MapSync.SendReloadRequest(_mapFilePath);
                string modifiedTagCollectionsStr = string.Join(",", entryPoints.Select(pt => pt.TagNumber));
                _logger.Trace($"Modify Tags={modifiedTagCollectionsStr} 'Enable' property to {enabled}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            finally
            {
                _semaphoreSlim.Release();
            }

        }


    }
}
