
using AGVSystem.Models.Map;
using AGVSystemCommonNet6.MAP;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace AGVSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MapController : ControllerBase
    {
        internal string local_map_file_path => AppSettings.MapFile;
        private string tempMapFilePath = "";
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(MapManager.LoadMapFromFile(local_map_file_path));

        }

        [HttpGet("ReloadMap")]
        public async Task<IActionResult> ReloadMap()
        {
            if (System.IO.File.Exists(tempMapFilePath))
            {
                var json = System.IO.File.ReadAllText(local_map_file_path);
                if (json == null)
                    return null;
                var data_obj = JsonConvert.DeserializeObject<Dictionary<string, Map>>(json);
                return Ok(data_obj["Map"]);
            }
            else
                return Ok(null);
        }
        [HttpPost("SaveMap")]
        public async Task<IActionResult> SaveMap([FromBody] Map map_modified)
        {
            MapManager.SaveMapToFile(map_modified, local_map_file_path);

            AGVSMapManager.CurrentMap = map_modified;
            AGVSMapManager.SyncEQRegionSetting(map_modified.Points.Values.ToList());

            AGVSystemCommonNet6.Microservices.MapSync.SendReloadRequest(AppSettings.MapFile);


            return Ok();
        }


        [HttpGet("Tags")]
        public async Task<IActionResult> GetTags()
        {

            return Ok(MapManager.GetTags());
        }


        [HttpGet("PathPlan")]
        public async Task<IActionResult> PathPlan(int fromTag, int toTag)
        {
            PathFinder finder = new PathFinder();
            var map = MapManager.LoadMapFromFile(local_map_file_path);
            var pathInfo = finder.FindShortestPathByTagNumber(map.Points, fromTag, toTag);
            return Ok(pathInfo);
        }


        [HttpGet("MapPointTemplate")]
        public async Task<IActionResult> MapPointTemplate()
        {
            return Ok(new MapStation()
            {
                Enable = true,
                StationType = AGVSystemCommonNet6.AGVDispatch.Messages.STATION_TYPE.Normal,
                TagNumber = 1
            });
        }

        /// <summary>
        /// 給前端的區域選項資料
        /// </summary>
        /// <returns></returns>
        [HttpGet("RegionOptions")]
        public async Task<IActionResult> RegionOptions()
        {
            //     { label: 'Normal',
            //value: 0}
            var options = AGVSMapManager.MapRegions.Select(map => new { label = map.RegionName, value = map.RegionName }).ToList();
            return Ok(options);
        }

        public class clsAGVInfoVM
        {

            public string name { get; set; }
            public int current_tag { get; set; }
            public int previous_tag { get; set; }
            public string color { get; set; }
        }
    }
}
