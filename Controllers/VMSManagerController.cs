
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
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                var websocket_client = await HttpContext.WebSockets.AcceptWebSocketAsync();
                try
                {
                    while (websocket_client.State == System.Net.WebSockets.WebSocketState.Open)
                    {
                        Thread.Sleep(200);

                        byte[] rev_buffer = new byte[4096];
                        websocket_client.ReceiveAsync(new ArraySegment<byte>(rev_buffer), CancellationToken.None);
                        using (AGVStatusDBHelper dBHelper = new AGVStatusDBHelper())
                        {
                            clsAGVStateViewModel GenViewMode(clsAGVStateDto data)
                            {
                                var s = JsonConvert.DeserializeObject<clsAGVStateViewModel>(JsonConvert.SerializeObject(data));
                                s.StationName = AGVSMapManager.GetNameByTagStr(data.CurrentLocation);
                                return s;
                            };
                            var vmdata = dBHelper.GetALL().OrderBy(a => a.AGV_Name).ToList().Select(data => GenViewMode(data));
                            await websocket_client.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(vmdata))), System.Net.WebSockets.WebSocketMessageType.Text, true, CancellationToken.None);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Websocket Client Closed (/ws/VMSStatus):" + ex.Message);
                }

            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

    }

    public class clsAGVStateViewModel : clsAGVStateDto
    {
        public string StationName { get; set; } = "";
    }
}
