using AGVSystemCommonNet6.MAP;
using AGVSystemCommonNet6.MAP.Converter;
using AGVSystemCommonNet6.MAP.YunTech;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AGVSystem.Controllers
{
    public partial class MapController
    {
        [HttpGet("test/YunTechMapConvertTest")]
        public async Task<IActionResult> ConvertTest()
        {
            string JsonFIlePath = "C:\\AGVS\\Map\\yuntech_map_data.json";
            MapConverter mapConverter = new MapConverter();
            Dictionary<string, Dictionary<string, Map>> mapConverted = mapConverter.YuntechMapToGPMMapFromFile(JsonFIlePath, out var yunTecMap);

            foreach (Dictionary<string, Map> item in mapConverted.Values)
            {
                foreach (KeyValuePair<string, Map> _subMap in item)
                {
                    System.IO.File.WriteAllText($@"C:\AGVS\Map\Yuntech-{_subMap.Key}-{_subMap.Value.Name}.json", JsonConvert.SerializeObject(new { Map = _subMap.Value }, Formatting.Indented));
                }
            }
            return Ok(new { yunTecMap, mapConverted });
        }


        [HttpGet("test/ResetGraphXYUseCoordination")]
        public async Task<IActionResult> ResetGraphXYUseCoordination(string mapFilePath)
        {
            Map map = MapManager.LoadMapFromFile(mapFilePath,out string msg);
            foreach (KeyValuePair<int, MapPoint> pt in map.Points)
            {
                pt.Value.Graph.X = Convert.ToInt32(Math.Round(pt.Value.X, 0) + "");
                pt.Value.Graph.Y = Convert.ToInt32(Math.Round(pt.Value.Y, 0) + "");
            }
            return Ok(MapManager.SaveMapToFile(map, mapFilePath));
        }


        [HttpGet("test/ResetCoordinations")]
        public async Task<IActionResult> ResetCoordinations(string mapFilePath,double ratio)
        {
            Map map = MapManager.LoadMapFromFile(mapFilePath, out string msg);
            foreach (KeyValuePair<int, MapPoint> pt in map.Points)
            {
                pt.Value.X = pt.Value.X / ratio;
                pt.Value.Y = pt.Value.Y / ratio;
                pt.Value.Graph.X = Convert.ToInt32(Math.Round(pt.Value.X, 0) + "");
                pt.Value.Graph.Y = Convert.ToInt32(Math.Round(pt.Value.Y, 0) + "");
            }
            return Ok(MapManager.SaveMapToFile(map, mapFilePath));
        }
    }
}
