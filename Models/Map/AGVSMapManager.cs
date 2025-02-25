﻿using AGVSystemCommonNet6;
using AGVSystemCommonNet6.Configuration;
using AGVSystemCommonNet6.MAP;
using EquipmentManagment.Device.Options;
using EquipmentManagment.Manager;
using Newtonsoft.Json;
using WebSocketSharp;
using static AGVSystemCommonNet6.MAP.MapPoint;

namespace AGVSystem.Models.Map
{
    public class AGVSMapManager
    {
        public class clsAGVCoordinatinoUploadInfo
        {
            public int TagNumber { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
            public double Theta { get; set; }
        }
        public static Dictionary<int, clsAGVCoordinatinoUploadInfo> AGVUploadCoordinationStore = new Dictionary<int, clsAGVCoordinatinoUploadInfo>();
        public static List<clsMapRegion> MapRegions = new List<clsMapRegion>()
        {

        };

        public static AGVSystemCommonNet6.MAP.Map CurrentMap;

        public static bool AGVUploadLocMode { get; private set; }

        public static void Initialize()
        {
            CurrentMap = MapManager.LoadMapFromFile();
        }

        internal static async Task SyncEQRegionSetting(List<MapPoint> mapStations)
        {
            await Task.Delay(10);
            foreach (MapPoint point in mapStations)
            {
                try
                {
                    int tag = point.TagNumber;
                    var eq = StaEQPManagager.EQPDevices.FirstOrDefault(_eq => _eq.EndPointOptions.TagID == tag);
                    if (eq != null)
                    {
                        eq.EndPointOptions.Region = point.Region;
                    }
                }
                catch (Exception ex)
                {

                }

            }

            StaEQPManagager.SaveEqConfigs();
            Console.WriteLine("SyncEQRegionSetting  Done.");
        }

        internal static void SyncMapPointRegionSetting(Dictionary<string, clsEndPointOptions> eQOptions)
        {
            foreach (var item in eQOptions.Values)
            {
                var tag = item.TagID;

                KeyValuePair<int, MapPoint> pt = CurrentMap.Points.FirstOrDefault(pt => pt.Value.TagNumber == tag);
                if (pt.Value != null)
                    pt.Value.Region = item.Region;

            }
            MapManager.SaveMapToFile(CurrentMap, AGVSConfigulator.SysConfigs.MapConfigs.CurrentMapFileName);
        }

        internal static List<double> CalulateDistanseMap(int SourceTag, List<int> DestineTagList)
        {
            MapPoint sourcePoint = CurrentMap.Points.Values.First(pt => pt.TagNumber == SourceTag);
            List<double> distanceList = new List<double>();
            foreach (var tag in DestineTagList)
            {
                MapPoint point = CurrentMap.Points.Values.First(pt => pt.TagNumber == tag);

                var distance = sourcePoint.CalculateDistance(point);
                distanceList.Add(distance);
            }
            return distanceList;
        }

        internal static string GetNameByTagStr(string currentLocation)
        {
            MapPoint? point = CurrentMap.Points.Values.FirstOrDefault(pt => pt.TagNumber.ToString() == currentLocation);
            if (point == null)
                return currentLocation;
            return point.Graph.Display;
        }

        internal static MapPoint GetMapPointByTag(int unloadStationTag)
        {
            MapPoint? point = CurrentMap.Points.Values.FirstOrDefault(pt => pt.TagNumber == unloadStationTag);
            return point;
        }

        internal static void StoreAGVLocationUpload(string AGVName, int tagNumber, double x, double y, double theta)
        {
            clsAGVCoordinatinoUploadInfo info = new clsAGVCoordinatinoUploadInfo
            {
                TagNumber = tagNumber,
                Theta = theta,
                X = x,
                Y = y
            };
            if (AGVUploadCoordinationStore.Keys.Contains(tagNumber))
            {
                AGVUploadCoordinationStore[tagNumber] = info;
            }
            else
            {
                AGVUploadCoordinationStore.Add(tagNumber, info);
            }

        }

        internal static void SwitchAGVUploadLocFun(bool enabled)
        {
            AGVUploadLocMode = enabled;
            if (!enabled)
                AGVUploadCoordinationStore.Clear();
        }

        internal static List<int> GetEntryPointsOfTag(List<int> maintainingEQTags)
        {
            return maintainingEQTags.SelectMany(tag => GetEntryPointsOfTag(tag)).ToList();
        }
        internal static List<int> GetEntryPointsOfTag(int eqTag)
        {
            MapPoint? eqPoint = CurrentMap.Points.Values.FirstOrDefault(pt => pt.TagNumber == eqTag);
            if (eqPoint == null || !eqPoint.IsEquipment)
                return new List<int>();

            return eqPoint.Target.Keys.SelectMany(index => CurrentMap.Points.Where(pt => pt.Key == index).Select(pt => pt.Value.TagNumber)).ToList();
        }

        internal static List<MapPoint> GetBufferStations()
        {
            List<MapPoint> bufferStations = CurrentMap.Points.Values.Where(pt => _isBuffer(pt.StationType)).ToList();
            bool _isBuffer(STATION_TYPE type)
            {
                return type == STATION_TYPE.Buffer || type == STATION_TYPE.Buffer_EQ || type == STATION_TYPE.Charge_Buffer;
            }

            return bufferStations;
        }
    }
}
