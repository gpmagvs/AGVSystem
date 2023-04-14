using AGVSystem.VMS;
using AGVSytemCommonNet6.MAP;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Drawing;
using System.Runtime.InteropServices;

namespace AGVSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MapController : ControllerBase
    {
        internal string local_map_file_path = @"D:\param\Map_UMTC_3F_Yellow.json";
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
        public async Task<IActionResult> SaveMap(Map map_modified)
        {
            return Ok(MapManager.SaveMapToFile(map_modified, local_map_file_path));
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
            var map = MapManager.LoadMapFromFile(@"D:\param\Map_UMTC_3F_Yellow.json");
            var pathInfo = finder.FindShortestPathByTagNumber(map.Points, fromTag, toTag);
            return Ok(pathInfo);
        }

        [HttpGet("AGVList")]
        public async Task<IActionResult> AGVList()
        {
            var infos = VMSManager.VMSList.Select(agv => new clsAGVInfoVM()
            {
                name = agv.Value.BaseProps.AGV_Name,
                current_tag = agv.Value.Running_Status.Last_Visited_Node,
                previous_tag = agv.Value.Running_Status.Last_Visited_Node,
                color = "blue"
            }) ;

            return Ok(infos);
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
