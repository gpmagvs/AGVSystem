using AGVSystemCommonNet6;
using AGVSystemCommonNet6.MAP;
using EquipmentManagment;
using Newtonsoft.Json;

namespace AGVSystem.Models.Map
{
    public class AGVSMapManager
    {
        public static List<clsMapRegion> MapRegions = new List<clsMapRegion>()
        {

        };

        public static AGVSystemCommonNet6.MAP.Map CurrentMap;
        public static void Initialize()
        {
            CurrentMap = MapManager.LoadMapFromFile(AppSettings.MapFile);
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
            File.WriteAllText(AppSettings.MapRegionConfigFile, JsonConvert.SerializeObject(MapRegions, Formatting.Indented));
        }
        private static void _LoadMapRegionConfig()
        {
            if (File.Exists(AppSettings.MapRegionConfigFile))
            {
                MapRegions = JsonConvert.DeserializeObject<List<clsMapRegion>>(File.ReadAllText(AppSettings.MapRegionConfigFile));
            }
            else
            {
                _SaveMapRegionConfig();
            }
        }

        internal static void SyncEQRegionSetting(List<MapStation> mapStations)
        {

            foreach (MapStation point in mapStations)
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

                KeyValuePair<int, MapStation> pt = CurrentMap.Points.First(pt => pt.Value.TagNumber == tag);
                pt.Value.Region = item.Region;

            }
            MapManager.SaveMapToFile(CurrentMap, AppSettings.MapFile);
        }

        internal static List<double> CalulateDistanseMap(int SourceTag, List<int> DestineTagList)
        {
            MapStation sourcePoint = CurrentMap.Points.Values.First(pt => pt.TagNumber == SourceTag);
            List<double> distanceList = new List<double>();
            foreach (var tag in DestineTagList)
            {
                MapStation point = CurrentMap.Points.Values.First(pt => pt.TagNumber == tag);

                var distance = sourcePoint.CalculateDistance(point);
                distanceList.Add(distance);
            }
            return distanceList;
        }

        internal static string GetNameByTagStr(string currentLocation)
        {
            MapStation? point = CurrentMap.Points.Values.FirstOrDefault(pt => pt.TagNumber.ToString() == currentLocation);
            if (point == null)
                return currentLocation;
            return point.Name;
        }
    }
}
