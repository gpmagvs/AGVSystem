
using AGVSystem.Models.Map;
using AGVSystemCommonNet6;
using AGVSystemCommonNet6.AGVDispatch.Messages;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.Log;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;

namespace AGVSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VMSManagerController : ControllerBase
    {
        private AGVSDbContext _dbContent;

        public VMSManagerController(AGVSDbContext dbContent)
        {
            _dbContent = dbContent;
        }



    }

    public class clsAGVStateViewModel : clsAGVStateDto
    {
        public string StationName { get; set; } = "";
        public string TaskSourceStationName { get; set; } = "";
        public string TaskDestineStationName { get; set; } = "";
        public ACTION_TYPE OrderAction { get; set; } = ACTION_TYPE.None;
        public string IP { get; set; } = "";
        public int Port { get; set; } = 0;
    }
}
