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


        internal static void StartCollectWebUIUsingDatas()
        {
            Thread thread = new Thread(async () =>
            {
                var db = new AGVSDatabase();
                while (true)
                {
                    Thread.Sleep(100);

                    try
                    {
                        //UIDatas["/ws/VMSStatus"] = db.tables.AgvStates.Where(stat => stat.Enabled).OrderBy(a => a.AGV_Name).AsNoTracking().ToList().Select(data => GenViewMode(data));


                        UIDatas["/ws/EQStatus"] = new { EQPData = StaEQPManagager.GetEQStates(), ChargeStationData = StaEQPManagager.GetChargeStationStates() };
                        UIDatas["/ws/VMSAliveCheck"] = true;
                        UIDatas["/ws/AGVLocationUpload"] = AGVSMapManager.AGVUploadCoordinationStore;
                        UIDatas["/ws/HotRun"] = HotRunScriptManager.HotRunScripts;

                        #region data fetched from database

                        #endregion
                        try
                        {
                            UIDatas["/UncheckedAlarm"] = AlarmManagerCenter.uncheckedAlarms;
                            var incompleted_tasks = db.tables.Tasks.Where(t => t.State == TASK_RUN_STATUS.WAIT | t.State == TASK_RUN_STATUS.NAVIGATING).OrderByDescending(t => t.Priority).AsNoTracking().ToList();
                            var completed_tasks = db.tables.Tasks.Where(t => t.State != TASK_RUN_STATUS.WAIT && t.State != TASK_RUN_STATUS.NAVIGATING).OrderByDescending(t => t.RecieveTime).Take(20).AsNoTracking().ToList();
                            UIDatas["/ws/TaskData"] = new { incompleteds = incompleted_tasks, completeds = completed_tasks };

                            var vmsData = await GetAGV_StatesData_FromVMS(db.tables.Tasks);
                            if (vmsData != null)
                                UIDatas["/ws/VMSStatus"] = vmsData;
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

            });
            thread.Start();


        }
        private static object GetDataByPath(string path)
        {
            if (UIDatas.TryGetValue(path, out object viewdata))
                return viewdata;
            else
                return null;
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
