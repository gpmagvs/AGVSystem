
using AGVSystem.Models.Map;
using AGVSystemCommonNet6;
using AGVSystemCommonNet6.DATABASE;
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


        [HttpGet("/ws/VMSStatus")]
        public async Task GetVMSStatus()
        {
            await WebsocketHandler.ClientRequest(HttpContext);
        }


        [HttpGet("/ws/AGVLocationUpload")]
        public async Task AGVLocationUpload()
        {
            await WebsocketHandler.ClientRequest(HttpContext);
        }
    }

    public class clsAGVStateViewModel : clsAGVStateDto
    {
        public string StationName { get; set; } = "";
    }
}
