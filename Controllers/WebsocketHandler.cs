using AGVSystem.Models.Map;
using AGVSystem.Models.TaskAllocation.HotRun;
using AGVSystem.Static;
using AGVSystem.TaskManagers;
using AGVSystemCommonNet6;
using AGVSystemCommonNet6.AGVDispatch.Messages;
using AGVSystemCommonNet6.Alarm;
using AGVSystemCommonNet6.Configuration;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.DATABASE.Helpers;
using AGVSystemCommonNet6.HttpTools;
using AGVSystemCommonNet6.Microservices.VMS;
using AGVSystemCommonNet6.TASK;
using AGVSystemCommonNet6.ViewModels;
using EquipmentManagment.Manager;
using Microsoft.EntityFrameworkCore;
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
        private static async Task<List<clsAGVStateViewModel>> GetAGV_StatesData_FromVMS()
        {
            try
            {
                List<clsAGVStateViewModel> output = VMSSerivces.AgvStatesData.Select(d => GenViewMode(d)).ToList();
                clsAGVStateViewModel GenViewMode(clsAGVStateDto d)
                {
                    clsAGVStateViewModel vm = JsonConvert.DeserializeObject<clsAGVStateViewModel>(JsonConvert.SerializeObject(d));
                    vm.StationName = AGVSMapManager.GetNameByTagStr(vm.CurrentLocation);
                    vm.IP = VMSDataStore.VehicleConfigs[vm.AGV_Name].HostIP;
                    vm.Port = VMSDataStore.VehicleConfigs[vm.AGV_Name].HostPort;
                    return vm;
                }
                return output;
            }
            catch (Exception)
            {
                return null;
            }
        }


        internal static void StartCollectWebUIUsingDatas()
        {
            Thread thread = new Thread(async () =>
            {
                var db = new AGVSDatabase();
                while (true)
                {
                    await Task.Delay(10);

                    try
                    {
                        //UIDatas["/ws/VMSStatus"] = db.tables.AgvStates.Where(stat => stat.Enabled).OrderBy(a => a.AGV_Name).AsNoTracking().ToList().Select(data => GenViewMode(data));
                        var vmsData = await GetAGV_StatesData_FromVMS();
                        if (vmsData != null)
                            UIDatas["/ws/VMSStatus"] = vmsData;
                        UIDatas["/ws/EQStatus"] = new { EQPData = StaEQPManagager.GetEQStates(), ChargeStationData = StaEQPManagager.GetChargeStationStates() };
                        UIDatas["/ws/VMSAliveCheck"] = true;
                        UIDatas["/UncheckedAlarm"] = AlarmManagerCenter.uncheckedAlarms;
                        UIDatas["/ws/AGVLocationUpload"] = AGVSMapManager.AGVUploadCoordinationStore;
                        UIDatas["/ws/HotRun"] = HotRunScriptManager.HotRunScripts;
                        var incompleted_tasks = db.tables.Tasks.Where(t => t.State == TASK_RUN_STATUS.WAIT | t.State == TASK_RUN_STATUS.NAVIGATING).AsNoTracking().ToList();
                        var completed_tasks = db.tables.Tasks.Where(t => t.State != TASK_RUN_STATUS.WAIT && t.State != TASK_RUN_STATUS.NAVIGATING).OrderByDescending(t => t.RecieveTime).Take(20).AsNoTracking().ToList();
                        UIDatas["/ws/TaskData"] = new { incompleteds = incompleted_tasks, completeds = completed_tasks };

                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                }

            });
            thread.Start();


        }
        private static async Task SendMessagesAsync(WebSocket webSocket, string? path)
        {
            if (path == null)
                return;

            var buff = new ArraySegment<byte>(new byte[10]);
            bool closeFlag = false;
            _ = Task.Factory.StartNew(async () =>
            {
                while (!closeFlag)
                {
                    await Task.Delay(10);
                    var data = GetDataByPath(path);
                    if (data == null)
                        continue;
                    if (data != null)
                    {
                        try
                        {
                            await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data))), WebSocketMessageType.Text, true, CancellationToken.None);
                            data = null;
                        }
                        catch (Exception)
                        {
                            return;
                        }
                    }
                }
            });

            while (true)
            {
                try
                {
                    await Task.Delay(100);
                    WebSocketReceiveResult result = await webSocket.ReceiveAsync(buff, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    break;
                }
            }
            closeFlag = true;
            webSocket.Dispose();
            GC.Collect();
        }
        private static double GetPublishDelay(string path)
        {
            if (path == "/ws/HotRun" | path == "/ws/VMSAliveCheck" | path == "/ws/AGVLocationUpload")
                return 1;
            else
                return 0.3;
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
