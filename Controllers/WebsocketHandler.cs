using AGVSystem.Models.Map;
using AGVSystem.Models.TaskAllocation.HotRun;
using AGVSystem.TaskManagers;
using AGVSystemCommonNet6;
using AGVSystemCommonNet6.Alarm;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.DATABASE.Helpers;
using AGVSystemCommonNet6.TASK;
using EquipmentManagment.Manager;
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

        private static Dictionary<string, object> UIDatas = new Dictionary<string, object>()
        {
            {"/ws/EQStatus",  new object() },
            {"/ws/VMSAliveCheck",new object()   },
            {"/ws/VMSStatus", new object()  },
            {"/UncheckedAlarm",  new object() },
            {"/ws/AGVLocationUpload", new object()  },
            {"/ws/HotRun", new object()  },
            {"/ws/TaskData", new object()  },
        };

        internal static void StartCollectWebUIUsingDatas()
        {
            Thread thread = new Thread(async () =>
            {
                while (true)
                {
                    await Task.Delay(100);
                    using (AGVStatusDBHelper dBHelper = new AGVStatusDBHelper())
                    {
                        clsAGVStateViewModel GenViewMode(clsAGVStateDto data)
                        {
                            var s = JsonConvert.DeserializeObject<clsAGVStateViewModel>(JsonConvert.SerializeObject(data));
                            s.StationName = AGVSMapManager.GetNameByTagStr(data.CurrentLocation);
                            return s;
                        };
                        UIDatas["/ws/VMSStatus"] = dBHelper.GetALL().OrderBy(a => a.AGV_Name).ToList().Select(data => GenViewMode(data));
                    }

                    UIDatas["/ws/EQStatus"] = new { EQPData = StaEQPManagager.GetEQStates(), ChargeStationData = StaEQPManagager.GetChargeStationStates() };
                    UIDatas["/ws/VMSAliveCheck"] = true;
                    UIDatas["/UncheckedAlarm"] = AlarmManagerCenter.uncheckedAlarms;
                    UIDatas["/ws/AGVLocationUpload"] = AGVSMapManager.AGVUploadCoordinationStore;
                    UIDatas["/ws/HotRun"] = HotRunScriptManager.HotRunScripts;

                    using (TaskDatabaseHelper taskDB = new TaskDatabaseHelper())
                    {
                        UIDatas["/ws/TaskData"] = new { incompleteds = taskDB.GetALLInCompletedTask(true), completeds = taskDB.GetALLCompletedTask(20, true) };
                    }

                }

            });
            thread.Start();


        }
        private static async Task SendMessagesAsync(WebSocket webSocket, string? path)
        {
            if (path == null)
                return;

            var delay = TimeSpan.FromSeconds(GetPublishDelay(path));
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
        private static double GetPublishDelay(string path)
        {
            if (path == "/ws/HotRun" | path == "/ws/UncheckedAlarm" | path == "/ws/VMSAliveCheck" | path == "/ws/AGVLocationUpload")
                return 1;
            else
                return 0.5;
        }
        private static object GetDataByPath(string path)
        {
            if (UIDatas.TryGetValue(path, out object viewdata))
                return viewdata;
            else
                return null;

        }



    }

}
