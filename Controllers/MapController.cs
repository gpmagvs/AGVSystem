
using AGVSystem.Models.Map;
using AGVSystemCommonNet6.AGVDispatch.Messages;
using AGVSystemCommonNet6.AGVDispatch.Model;
using AGVSystemCommonNet6.Configuration;
using AGVSystemCommonNet6.GPMRosMessageNet.Messages;
using AGVSystemCommonNet6.Log;
using AGVSystemCommonNet6.MAP;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using static AGVSystemCommonNet6.MAP.MapPoint;

namespace AGVSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public partial class MapController : ControllerBase
    {
        internal string local_map_file_path => AGVSConfigulator.SysConfigs.MapConfigs.MapFileFullName;
        private string tempMapFilePath = "";
        private IConfiguration configuration;
        private string mapFileFolder => configuration.GetValue<string>("StaticFileOptions:MapFile:FolderPath");
        private string agvImageFileFolder => configuration.GetValue<string>("StaticFileOptions:AGVImageStoreFile:FolderPath");
        public MapController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
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

        [HttpPost("ResetGraphXYAsCoordinations")]
        public async Task<IActionResult> ResetGraphXYAsCoordinations()
        {
            try
            {
                foreach (var point in AGVSMapManager.CurrentMap.Points.Values)
                {
                    point.Graph.X = point.X;
                    point.Graph.Y = point.Y;
                }
                MapManager.SaveMapToFile(AGVSMapManager.CurrentMap, local_map_file_path);
                return Ok(new { confirm = true });
            }
            catch (Exception ex)
            {
                return Ok(new { confirm = false, message = ex.Message });
            }
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
            { }

            var list_exist_point = map_modified.Points.Keys.ToList();
            foreach (var note in map_modified.Points)
            {
                foreach (var point in note.Value.Target)
                {
                    if (!list_exist_point.Contains(point.Key))
                    {
                        note.Value.Target.Remove(point.Key);
                    }
                }
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


        [HttpGet("PreviewNavigationPath")]
        public async Task<IActionResult> PreviewNavigationPath(int fromTag, int toTag, ACTION_TYPE action = ACTION_TYPE.None, string AGVName = "")
        {
            PathFinder finder = new PathFinder();
            var map = MapManager.LoadMapFromFile();
            var pathInfo = finder.FindShortestPathByTagNumber(map, fromTag, toTag);
            var coordinates = pathInfo.stations.Select(pt => new clsCoordination(pt.X, pt.Y, 0));
            return Ok(coordinates);
        }


        [HttpGet("MapPointTemplate")]
        public async Task<IActionResult> MapPointTemplate()
        {
            return Ok(new MapPoint()
            {
                Enable = true,
                StationType = STATION_TYPE.Normal,
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

        [HttpPost("AGVIconUpload")]
        public async Task<IActionResult> AGVIconUpload(string AGVName)
        {
            var file = Request.Form.Files[0];
            if (file.Length > 100 * 1024 * 1024) // 100MB
            {
                return BadRequest("檔案大小超過 100MB。");
            }

            if (file.Length > 0)
            {
                var fileName = $@"\{AGVName}-Icon.png";
                var imageFolder = agvImageFileFolder;
                var filePath = imageFolder + fileName;

                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                LOG.TRACE($"User Upload ICON Image For {AGVName}({filePath})");
                return Ok(new { filename = fileName });
            }

            return BadRequest("未接收到任何檔案。");
        }

        [HttpPost("IconUpload")]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> IconUpload()
        {
            var file = Request.Form.Files[0];
            if (file.Length > 100 * 1024 * 1024) // 100MB
            {
                return BadRequest("檔案大小超過 100MB。");
            }

            if (file.Length > 0)
            {
                var fileName = $"/images/{file.FileName}";
                var imageFolder = Path.Combine(Environment.CurrentDirectory, "wwwroot/images");
                var filePath = Path.Combine(imageFolder, file.FileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                if (Debugger.IsAttached)
                {
                    try
                    {
                        var _imageFolder = @"D:\GPM_Work\AGV\Codes\AGVSystem\AGVS_UI\public\images";
                        var _filePath = Path.Combine(_imageFolder, file.FileName);
                        using (var stream = new FileStream(_filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
                var currentMap = MapManager.LoadMapFromFile(false, false);
                currentMap.Options.EQIcons.Add(fileName);
                currentMap.Options.EQIcons = currentMap.Options.EQIcons.Distinct().ToList();
                MapManager.SaveMapToFile(currentMap, AGVSConfigulator.SysConfigs.MapConfigs.MapFileFullName);
                return Ok(new { filename = fileName });
            }

            return BadRequest("未接收到任何檔案。");
        }

        [HttpDelete("DeleteIcon")]
        public async Task<IActionResult> DeleteIcon(string filePath)
        {
            var fileFullPath = Path.Combine(Environment.CurrentDirectory, "wwwroot" + filePath);
            if (System.IO.File.Exists(fileFullPath))
            {
                System.IO.File.Delete(fileFullPath);

            }
            var _map = MapManager.LoadMapFromFile(false, false);
            _map.Options.EQIcons.Remove(filePath);
            MapManager.SaveMapToFile(_map, AGVSConfigulator.SysConfigs.MapConfigs.MapFileFullName);
            return Ok("ok");
        }



        [HttpPost("MapImageUpload")]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> MapImageUpload()
        {
            var file = Request.Form.Files[0];
            if (file.Length > 0)
            {
                var fileName = Path.Combine(mapFileFolder, file.FileName);
                //var imageFolder = Path.Combine(Environment.CurrentDirectory, "wwwroot");
                //var filePath = imageFolder + fileName;
                //Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                using (var stream = new FileStream(fileName, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                //LOG.TRACE($"User Upload ICON Image For {AGVName}({filePath})");
                return Ok(fileName);
            }

            return BadRequest("未接收到任何檔案。");
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
