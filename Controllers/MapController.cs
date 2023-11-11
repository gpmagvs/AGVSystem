
using AGVSystem.Models.Map;
using AGVSystemCommonNet6.Configuration;
using AGVSystemCommonNet6.MAP;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
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
    public partial class MapController : ControllerBase
    {
        internal string local_map_file_path => AGVSConfigulator.SysConfigs.MapConfigs.MapFileFullName;
        private string tempMapFilePath = "";


        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(MapManager.LoadMapFromFile());

        }



        [HttpGet("GeoMapJson")]
        public async Task<IActionResult> GetGeoMapJson()
        {
            var map = MapManager.LoadMapFromFile();
            return Ok(map.GetGeoMapData());
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
            //map_modified.Note = $"v{DateTime.Now.ToString("yyMMdd")}0821.1";
            string[] note_splited = map_modified.Note.Split('.');
            int lastModifyVersion = 0;
            string todayDateTime_Str = DateTime.Now.ToString($"yyMMdd");
            if (note_splited.Length == 2)
            {
                lastModifyVersion = Convert.ToInt16(note_splited[1]);
                if (todayDateTime_Str == note_splited[0].Replace("v", ""))
                {
                    map_modified.Note = $"{note_splited[0]}.{lastModifyVersion + 1}";
                }
                else
                {
                    map_modified.Note = $"v{todayDateTime_Str}.1";
                }

            }
            else
            {

            }

            MapManager.SaveMapToFile(map_modified, local_map_file_path);

            AGVSMapManager.CurrentMap = map_modified;
            AGVSMapManager.SyncEQRegionSetting(map_modified.Points.Values.ToList());

            AGVSystemCommonNet6.Microservices.MapSync.SendReloadRequest(AGVSConfigulator.SysConfigs.MapConfigs.CurrentMapFileName);


            return Ok(map_modified.Note);
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
            var map = MapManager.LoadMapFromFile();
            var pathInfo = finder.FindShortestPathByTagNumber(map, fromTag, toTag);
            return Ok(pathInfo);
        }


        [HttpGet("MapPointTemplate")]
        public async Task<IActionResult> MapPointTemplate()
        {
            return Ok(new MapPoint()
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
        [HttpGet("UploadCoordination")]
        public async Task<IActionResult> UploadCoordintaion(string AGVName, int tagNumber, double x, double y, double theta)
        {
            if (!AGVSMapManager.AGVUploadLocMode)
                return Ok("AGV上報位置功能尚未開啟");
            AGVSMapManager.StoreAGVLocationUpload(AGVName, tagNumber, x, y, theta);
            return Ok(true);
        }
        [HttpGet("SwitchAGVUploadLocFun")]
        public async Task<IActionResult> SwitchAGVUploadLocFun(bool enabled)
        {
            AGVSMapManager.SwitchAGVUploadLocFun(enabled);
            return Ok();
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
