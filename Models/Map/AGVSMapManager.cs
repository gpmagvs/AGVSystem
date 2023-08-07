using AGVSystemCommonNet6;
using AGVSystemCommonNet6.Configuration;
using AGVSystemCommonNet6.MAP;
using EquipmentManagment;
using Newtonsoft.Json;

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
        public static void Initialize()
        {
            CurrentMap = MapManager.LoadMapFromFile();
            _LoadMapRegionConfig();
        }

        public static bool TryCreateNewMapRegion(string name, out clsMapRegion newRegion, out string errorMsg)
        {
            newRegion = null;
            errorMsg = string.Empty;

            if (MapRegions.Any(_region => _region.RegionName == name))
            {
                errorMsg = $"已經存在名稱為 {name} 的區域";
                return false;
            }

            newRegion = new clsMapRegion
            {
                RegionDescription = name,
            };
            MapRegions.Add(newRegion);
            return true;
        }

        private static void _SaveMapRegionConfig()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(AGVSConfigulator.SysConfigs.MapConfigs.MapRegionConfigFile));
            File.WriteAllText(AGVSConfigulator.SysConfigs.MapConfigs.MapRegionConfigFile, JsonConvert.SerializeObject(MapRegions, Formatting.Indented));
        }
        private static void _LoadMapRegionConfig()
        {
            if (File.Exists(AGVSConfigulator.SysConfigs.MapConfigs.MapRegionConfigFile))
            {
                MapRegions = JsonConvert.DeserializeObject<List<clsMapRegion>>(File.ReadAllText(AGVSConfigulator.SysConfigs.MapConfigs.MapRegionConfigFile));
            }
            else
            {
                _SaveMapRegionConfig();
            }
        }

        internal static void SyncEQRegionSetting(List<MapPoint> mapStations)
        {

            foreach (MapPoint point in mapStations)
            {
                try
                {
                    int tag = point.TagNumber;
                    var eq = StaEQPManagager.EQPDevices.First(_eq => _eq.EndPointOptions.TagID == tag);
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
            return point.Name;
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
    }
}
