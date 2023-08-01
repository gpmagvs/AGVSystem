using AGVSystem.Models.Map;
using AGVSystem.TaskManagers;
using AGVSystemCommonNet6;
using AGVSystemCommonNet6.Alarm;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.TASK;
using EquipmentManagment;
using Newtonsoft.Json;
using System.Net.WebSockets;
using System.Text;

namespace AGVSystem.Controllers
{
    public class WebsocketHandler
    {
        private static Dictionary<string, List<WebSocket>> clients = new Dictionary<string, List<WebSocket>>();
        public static async Task ClientRequest(HttpContext _HttpContext)
        {
            string path = _HttpContext.Request.Path.Value;
            if (path == null)
            {
                _HttpContext.Response.StatusCode = 400;
                return;
            }


            if (_HttpContext.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await _HttpContext.WebSockets.AcceptWebSocketAsync();
                await SendMessagesAsync(webSocket, path);
            }
            else
            {
                _HttpContext.Response.StatusCode = 400;
            }
        }

        private static async Task SendMessagesAsync(WebSocket webSocket, string? path)
        {
            if (path == null)
                return;

            var delay = TimeSpan.FromSeconds(.5);
            while (webSocket.State == WebSocketState.Open)
            {
                await Task.Delay(delay);
                try
                {
                    webSocket.ReceiveAsync(new ArraySegment<byte>(new byte[10]), CancellationToken.None);
                    var viewmodel = GetDataByPath(path);
                    if (viewmodel == null)
                        continue;
                    await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(viewmodel))), System.Net.WebSockets.WebSocketMessageType.Text, true, CancellationToken.None);

                }
                catch (WebSocketException)
                {
                    // 客戶端已斷開連線，停止傳送訊息
                    break;
                }
            }

        }

        private static object GetDataByPath(string path)
        {

            if (path == "/ws/VMSStatus")
            {
                using (AGVStatusDBHelper dBHelper = new AGVStatusDBHelper())
                {
                    clsAGVStateViewModel GenViewMode(clsAGVStateDto data)
                    {
                        var s = JsonConvert.DeserializeObject<clsAGVStateViewModel>(JsonConvert.SerializeObject(data));
                        s.StationName = AGVSMapManager.GetNameByTagStr(data.CurrentLocation);
                        return s;
                    };
                    return dBHelper.GetALL().OrderBy(a => a.AGV_Name).ToList().Select(data => GenViewMode(data));
                }
            }
            if (path == "/ws/EQStatus")
            {
                return StaEQPManagager.GetEQStates();
            }
            if (path == "/ws/VMSAliveCheck")
            {
                return true;
            }
            if (path == "/ws/TaskData")
            {
                clsTaskDto[] incompleteds = TaskManager.InCompletedTaskList.ToArray();
                clsTaskDto[] completeds = TaskManager.CompletedTaskList.ToArray();
                return new { incompleteds, completeds };
            }
            if (path == "/UncheckedAlarm")
            {
                return AlarmManagerCenter.uncheckedAlarms;
            }

            return "";
        }



    }

}
