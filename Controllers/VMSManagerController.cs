
using AGVSystem.Models.Map;
using AGVSystem.Models.WebsocketMiddleware;
using AGVSystemCommonNet6;
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


        [HttpGet("/ws/VMSStatus")]
        public async Task GetVMSStatus(string? user_id ="")
        {
            WebsocketMiddleware.UserJoin(user_id);
            await WebsocketMiddleware.ClientRequest(HttpContext, user_id);
        }


        [HttpGet("/ws/AGVLocationUpload")]
        public async Task AGVLocationUpload()
        {
            await WebsocketMiddleware.ClientRequest(HttpContext);
        }
    }

    public class clsAGVStateViewModel : clsAGVStateDto
    {
        public string StationName { get; set; } = "";
        public string IP { get; set; } = "";
        public int Port { get; set; } = 0;
    }
}
