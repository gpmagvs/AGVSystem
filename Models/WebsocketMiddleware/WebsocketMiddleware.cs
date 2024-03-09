using AGVSystem.Controllers;
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
using AGVSystemCommonNet6.Log;
using AGVSystemCommonNet6.Microservices.VMS;

using AGVSystemCommonNet6.ViewModels;
using EquipmentManagment.Manager;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net.WebSockets;
using System.Text;

namespace AGVSystem.Models.WebsocketMiddleware
{
    public class WebsocketMiddleware
    {
        private static Dictionary<string, List<WebSocket>> clients = new Dictionary<string, List<WebSocket>>();
        internal static Dictionary<string, string> UsersRouter = new Dictionary<string, string>();
        public enum WEBSOCKET_CHANNELS
        {
            EQ_STATUS,
            VMS_ALIVE_CHECK,
            VMS_STATUS,
            UNCHECKED_ALARM,
            AGV_LOCATION_UPLOAD,
            HOTRUN,
            TASK_DATA,
            SYSTEM_STATUS
        }
        public class clsChannelModel
        {
            public clsChannelModel(string route_name)
            {
                this.route_name = route_name;
            }
            public string route_name { get; }
            public object data { get; set; }
        }

        public static Dictionary<WEBSOCKET_CHANNELS, clsChannelModel> websocket_api_routes = new Dictionary<WEBSOCKET_CHANNELS, clsChannelModel>
        {
            { WEBSOCKET_CHANNELS.EQ_STATUS,new clsChannelModel("/ws/EQStatus")},
            { WEBSOCKET_CHANNELS.VMS_ALIVE_CHECK,new clsChannelModel("/ws/VMSAliveCheck") },
            { WEBSOCKET_CHANNELS.VMS_STATUS,new clsChannelModel("/ws/VMSStatus") },
            { WEBSOCKET_CHANNELS.UNCHECKED_ALARM,new clsChannelModel("/UncheckedAlarm") },
            { WEBSOCKET_CHANNELS.AGV_LOCATION_UPLOAD,new clsChannelModel("/ws/AGVLocationUpload") },
            { WEBSOCKET_CHANNELS.HOTRUN,new clsChannelModel("/ws/HotRun") },
            { WEBSOCKET_CHANNELS.TASK_DATA,new clsChannelModel("/ws/TaskData") },
            { WEBSOCKET_CHANNELS.SYSTEM_STATUS,new clsChannelModel("/ws/SystemStatus") },
        };
        internal static List<string> EditMapUsers
        {
            get
            {
                return UsersRouter.Where(user => user.Value == "/map").Select(user => user.Key).ToList();
            }
        }
        public static async Task ClientRequest(HttpContext _HttpContext, string user_id = "")
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
                clsWebsocktClientHandler clientHander = new clsWebsocktClientHandler(webSocket, path, user_id);
                clientHander.OnDataFetching += (path) => { return GetDataByPath(path); };
                clientHander.OnClientLeve += (obj, user_id) => { UserLeave(user_id); };
                await clientHander.StartBrocast();
            }
            else
            {
                _HttpContext.Response.StatusCode = 400;
            }
        }


        private static async Task<List<clsAGVStateViewModel>> GetAGV_StatesData_FromVMS(DbSet<AGVSystemCommonNet6.AGVDispatch.clsTaskDto> tasks)
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

                    if (vm.TaskName == "")
                    {
                        vm.TaskSourceStationName = vm.TaskDestineStationName = "";
                    }
                    else
                    {
                        var task_ = tasks.FirstOrDefault(tk => tk.TaskName == vm.TaskName);
                        if (task_ != null)
                        {
                            vm.TaskSourceStationName = task_.Action != ACTION_TYPE.Carry ? vm.StationName : AGVSMapManager.GetNameByTagStr(task_.From_Station);
                            vm.TaskDestineStationName = AGVSMapManager.GetNameByTagStr(task_.To_Station);
                        }
                    }

                    return vm;
                }
                return output;
            }
            catch (Exception)
            {
                return null;
            }
        }


        internal static async Task StartCollectWebUIUsingDatas()
        {

            await Task.Delay(100);
            var db = new AGVSDatabase();
            while (true)
            {
                await Task.Delay(100);
                try
                {
                    websocket_api_routes[WEBSOCKET_CHANNELS.EQ_STATUS].data = new
                    {
                        EQPData = StaEQPManagager.GetEQStates(),
                        ChargeStationData = StaEQPManagager.GetChargeStationStates(),
                        WIPsData = GetWIPDataViewModels()
                    };
                    websocket_api_routes[WEBSOCKET_CHANNELS.VMS_ALIVE_CHECK].data = true;
                    websocket_api_routes[WEBSOCKET_CHANNELS.AGV_LOCATION_UPLOAD].data = AGVSMapManager.AGVUploadCoordinationStore;
                    websocket_api_routes[WEBSOCKET_CHANNELS.HOTRUN].data = HotRunScriptManager.HotRunScripts;
                    try
                    {
                        websocket_api_routes[WEBSOCKET_CHANNELS.UNCHECKED_ALARM].data = AlarmManagerCenter.uncheckedAlarms;
                        var incompleted_tasks = db.tables.Tasks.Where(t => t.State == TASK_RUN_STATUS.WAIT | t.State == TASK_RUN_STATUS.NAVIGATING).OrderByDescending(t => t.Priority).AsNoTracking().ToList();
                        var completed_tasks = db.tables.Tasks.Where(t => t.State != TASK_RUN_STATUS.WAIT && t.State != TASK_RUN_STATUS.NAVIGATING).OrderByDescending(t => t.FinishTime).Take(20).AsNoTracking().ToList();
                        websocket_api_routes[WEBSOCKET_CHANNELS.TASK_DATA].data = new { incompleteds = incompleted_tasks, completeds = completed_tasks };

                        var vmsData = await GetAGV_StatesData_FromVMS(db.tables.Tasks);
                        if (vmsData != null)
                            websocket_api_routes[WEBSOCKET_CHANNELS.VMS_STATUS].data = vmsData;
                    }
                    catch (Exception ex)
                    {
                    }
                }
                catch (Exception ex)
                {
                    continue;
                }
            }

        }

        private static List<ViewModel.WIPDataViewModel> GetWIPDataViewModels()
        {
            return StaEQPManagager.RacksList.Select(wip => new ViewModel.WIPDataViewModel()
            {
                WIPName = wip.EQName,
                Columns = wip.RackOption.Columns,
                Rows = wip.RackOption.Rows,
                Ports = wip.PortsStatus.ToList()
            }).ToList();
        }

        private static object GetDataByPath(string path)
        {
            var _ws = websocket_api_routes.Values.FirstOrDefault(r => r.route_name == path);
            if (_ws == null)
                return null;
            return _ws.data;
        }
        internal static void UserJoin(string user_id)
        {
            if (UsersRouter.TryAdd(user_id, "/"))
            {
                LOG.TRACE($"User-{user_id} 從瀏覽器開始使用系統");
            }
        }
        internal static void UserLeave(string user_id)
        {
            if (UsersRouter.Remove(user_id))
            {
                LOG.TRACE($"User-{user_id} leave website.");
            }

        }

        internal static void UserChangeRoute(string userID, string current_route)
        {
            if (UsersRouter.TryGetValue(userID, out var _))
            {
                UsersRouter[userID] = current_route;
            }
        }
    }

}
