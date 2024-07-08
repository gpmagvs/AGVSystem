using AGVSystem.Models.Map;
using AGVSystem.Service;
using AGVSystemCommonNet6.Configuration;
using AGVSystemCommonNet6.Log;
using AGVSystemCommonNet6.MAP;
using AGVSystemCommonNet6.Microservices;
using AGVSystemCommonNet6.Microservices.VMS;
using AGVSystemCommonNet6.Notify;
using EquipmentManagment.Device;
using System.Diagnostics;

namespace AGVSystem.Models.EQDevices
{
    public partial class EQDeviceEventsHandler
    {
        private static bool _disableEntryPointWhenEQMaintaining = AGVSConfigulator.SysConfigs.EQManagementConfigs.DisableEntryPointWhenEQMaintaining;

        private static void HandleDeviceMaintainFinish(object? sender, EndPointDeviceAbstract device)
        {
            LOG.TRACE($"{device.EQName} Maintain Signal OFF");
            NotifyServiceHelper.SUCCESS($"設備-{device.EQName} 完成維修");
            ChangeEnableStateOfEntryPointOfEQOfMapAndRequestVMSReload(device.EndPointOptions.TagID, true);
        }

        private static void HandleDeviceMaintainStart(object? sender, EndPointDeviceAbstract device)
        {
            LOG.TRACE($"{device.EQName} Maintain Signal ON");
            NotifyServiceHelper.WARNING($"設備-{device.EQName} 維修中!");
            if (_disableEntryPointWhenEQMaintaining)
                ChangeEnableStateOfEntryPointOfEQOfMapAndRequestVMSReload(device.EndPointOptions.TagID, false);
        }


        private static async Task ChangeEnableStateOfEntryPointOfEQOfMapAndRequestVMSReload(int deviceTag, bool enabled)
        {

            MapPoint deviceMapPoint = AGVSMapManager.CurrentMap.Points.Values.FirstOrDefault(pt => pt.TagNumber == deviceTag);
            if (deviceMapPoint == null)
            {
                LOG.WARN($"[EQDeviceEventsHandler] Cannot find Map Point that TagNumber equal {deviceTag}");
                return;
            }
            IEnumerable<MapPoint> entryPoints = deviceMapPoint.Target.Keys.Select(_index => AGVSMapManager.CurrentMap.Points[_index]);
            if (!entryPoints.Any())
                return;

            foreach (MapPoint point in entryPoints)
            {
                point.Enable = enabled;
            }
            clsMapConfigs mapConfigs = AGVSConfigulator.SysConfigs.MapConfigs;
            MapManager.SaveMapToFile(AGVSMapManager.CurrentMap, mapConfigs.MapFileFullName);
            await NotifyServiceHelper.INFO($"Map-Point-Enabled-Property-Changed", false);
            MapSync.SendReloadRequest(mapConfigs.CurrentMapFileName);
            string modifiedTagCollectionsStr = string.Join(",", entryPoints.Select(pt => pt.TagNumber));
            LOG.TRACE($"Modify Tags={modifiedTagCollectionsStr} 'Enable' property to {enabled}");

        }


    }
}
