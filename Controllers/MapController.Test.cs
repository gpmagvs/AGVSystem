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
    }
}
