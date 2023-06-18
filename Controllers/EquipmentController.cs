using AGVSystem.Models.Map;
using AGVSystemCommonNet6.DATABASE;
using EquipmentManagment;
using EquipmentManagment.Connection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace AGVSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public partial class EquipmentController : ControllerBase
    {
        [HttpGet("/ws/EQStatus")]
        public async Task EQStatus()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                var websocket_client = await HttpContext.WebSockets.AcceptWebSocketAsync();
                try
                {

                    byte[] rev_buffer = new byte[4096];

                    while (websocket_client.State == System.Net.WebSockets.WebSocketState.Open)
                    {
                        Thread.Sleep(200);

                        websocket_client.ReceiveAsync(new ArraySegment<byte>(rev_buffer), CancellationToken.None);
                        var _newData = StaEQPManagager.GetEQStates();
                        var dataJson = JsonConvert.SerializeObject(_newData);
                        await websocket_client.SendAsync(new ArraySegment<byte>(Encoding.ASCII.GetBytes(dataJson)), System.Net.WebSockets.WebSocketMessageType.Text, true, CancellationToken.None);
                       
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

        [HttpPost("WriteOutputs")]
        public async Task<IActionResult> WriteOutPuts(string EqName, ushort start, bool[] value)
        {
            var eq = StaEQPManagager.GetEQByName(EqName);
            eq.WriteOutputs(start, value);
            return Ok();
        }

        [HttpGet("GetEQInfoByTag")]
        public async Task<IActionResult> GetEQInfoByTag(int Tag)
        {
            var EQ = StaEQPManagager.EQOptions.Values.First(eq => eq.TagID == Tag);
            return Ok(EQ);
        }

        [HttpGet("GetEQOptions")]
        public async Task<IActionResult> GetEQOptions()
        {
            return Ok(StaEQPManagager.EQOptions.Values.ToArray());
        }
        [HttpPost("SaveEQOptions")]
        public async Task<IActionResult> SaveEQOptions(List<clsEndPointOptions> datas)
        {
            //TODO 檢查是否有重複的設定 , 包含 Tag重複、IP:Port 重複、ComPort 重複
            var eqNames = datas.Select(data => data.Name).ToList().Distinct();
            if (eqNames.Count() != datas.Count)
            {
                return Ok(new { confirm = false, message = "重複的設備名稱，請再次確認設定" });
            }

            StaEQPManagager.EQOptions = datas.ToDictionary(eq => eq.Name, eq => eq);

            AGVSMapManager.SyncMapPointRegionSetting(StaEQPManagager.EQOptions);

            StaEQPManagager.DisposeEQs();
            StaEQPManagager.SaveEqConfigs();
            StaEQPManagager.InitializeAsync();

            return Ok(new { confirm = true, message = "" });
        }
        [HttpPost("ConnectTest")]
        public async Task<IActionResult> ConnectTest(ConnectOptions options)
        {

            clsEQ eq = new clsEQ(new clsEndPointOptions { ConnOptions = options });
            bool connected = await eq.Connect(use_for_conn_test: true);
            eq.Dispose();
            return Ok(new { Connected = connected });
        }



    }

}
