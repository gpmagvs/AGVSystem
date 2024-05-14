using AGVSystem.Models.Map;
using AGVSystemCommonNet6.Configuration;
using AGVSystemCommonNet6.Log;
using AGVSystemCommonNet6.MAP;
using AGVSystemCommonNet6.Microservices.VMS;
using EquipmentManagment.Device;
using System.Diagnostics;

namespace AGVSystem.Models.EQDevices
{
    public partial class EQDeviceEventsHandler
    {
        private static void HandleDeviceMaintainFinish(object? sender, EndPointDeviceAbstract device)
        {
            LOG.TRACE($"{device.EQName} Maintain Signal ON");
            ChangeEnableStateOfEntryPointOfEQOfMapAndRequestVMSReload(device.EndPointOptions.TagID, true);
        }

        private static void HandleDeviceMaintainStart(object? sender, EndPointDeviceAbstract device)
        {
            LOG.TRACE($"{device.EQName} Maintain Signal OFF");
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
            Stopwatch _stopwatch = Stopwatch.StartNew();
            await AGVSystemCommonNet6.Microservices.MapSync.SendReloadRequest(mapConfigs.CurrentMapFileName);
            _stopwatch.Stop();
            Console.WriteLine($"SendReloadRequest time spend={_stopwatch.Elapsed.Seconds} s");
            string modifiedTagCollectionsStr = string.Join(",", entryPoints.Select(pt => pt.TagNumber));
            LOG.TRACE($"Modify Tags={modifiedTagCollectionsStr} 'Enable' property to {enabled}");

        }


    }
}
