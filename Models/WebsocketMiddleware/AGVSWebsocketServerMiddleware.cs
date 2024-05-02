using AGVSystem.Controllers;
using AGVSystem.Models.Map;
using AGVSystem.Models.TaskAllocation.HotRun;
using AGVSystemCommonNet6.AGVDispatch.Messages;
using AGVSystemCommonNet6;
using AGVSystemCommonNet6.Alarm;
using AGVSystemCommonNet6.HttpTools;
using AGVSystemCommonNet6.Microservices.VMS;
using EquipmentManagment.Manager;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using AGVSystemCommonNet6.DATABASE;
using System.Net.NetworkInformation;

namespace AGVSystem.Models.WebsocketMiddleware
{
    public class AGVSWebsocketServerMiddleware : WebsocketServerMiddleware
    {
        public static AGVSWebsocketServerMiddleware Middleware { get; set; } = new AGVSWebsocketServerMiddleware(300);

        public AGVSWebsocketServerMiddleware(int publish_duraction) : base(publish_duraction)
        {

        }

        AGVSDatabase db = new AGVSDatabase();
        public override List<string> channelMaps { get; set; } = new List<string>()
        {
            "/ws",
            "/ws/EQStatus",
            "/ws/VMSAliveCheck",
            "/ws/VMSStatus",
            "/UncheckedAlarm",
            "/ws/AGVLocationUpload",
            "/ws/HotRun",
            "/ws/TaskData",
            "/ws/SystemStatus",
        };

        protected override async Task CollectViewModelData()
        {
            try
            {
                //var vmsData = await GetAGV_StatesData_FromVMS(db.tables.Tasks);
                var incompleted_tasks = db.tables.Tasks.AsNoTracking().Where(t => t.State == TASK_RUN_STATUS.WAIT || t.State == TASK_RUN_STATUS.NAVIGATING).OrderByDescending(t => t.RecieveTime).ToList();
                var completed_tasks = db.tables.Tasks.AsNoTracking().Where(t => t.State != TASK_RUN_STATUS.WAIT && t.State != TASK_RUN_STATUS.NAVIGATING).OrderByDescending(t => t.FinishTime).Take(20).ToList();

                CurrentViewModelDataOfAllChannel["/ws"] = new
                {
                    EQStatus = new
                    {
                        EQPData = StaEQPManagager.GetEQStates(),
                        ChargeStationData = StaEQPManagager.GetChargeStationStates(),
                        WIPsData = GetWIPDataViewModels()
                    },
                    VMSAliveCheck = VMSSerivces.IsAlive,
                    AGVLocationUpload = AGVSMapManager.AGVUploadCoordinationStore,
                    HotRun = HotRunScriptManager.HotRunScripts,
                    UncheckedAlarm = AlarmManagerCenter.uncheckedAlarms,
                    //VMSStatus = vmsData,
                    TaskData = new { incompleteds = incompleted_tasks, completeds = completed_tasks }
                };
            }
            catch (Exception ex)
            {

                throw ex;
            }
           
        }

        //private static async Task<List<clsAGVStateViewModel>> GetAGV_StatesData_FromVMS(DbSet<AGVSystemCommonNet6.AGVDispatch.clsTaskDto> tasks)
        //{
        //    try
        //    {
        //        List<clsAGVStateViewModel> output = VMSSerivces.AgvStatesData.Select(d => GenViewMode(d)).ToList();
        //        clsAGVStateViewModel GenViewMode(clsAGVStateDto d)
        //        {
        //            clsAGVStateViewModel vm = JsonConvert.DeserializeObject<clsAGVStateViewModel>(JsonConvert.SerializeObject(d));
        //            vm.StationName = AGVSMapManager.GetNameByTagStr(vm.CurrentLocation);

        //            if (vm.TaskName == "")
        //            {
        //                vm.TaskSourceStationName = vm.TaskDestineStationName = "";
        //            }
        //            else
        //            {
        //                var task_ = tasks.FirstOrDefault(tk => tk.TaskName == vm.TaskName);
        //                if (task_ != null)
        //                {
        //                    vm.TaskSourceStationName = task_.Action != ACTION_TYPE.Carry ? vm.StationName : AGVSMapManager.GetNameByTagStr(task_.From_Station);
        //                    vm.TaskDestineStationName = AGVSMapManager.GetNameByTagStr(task_.To_Station);
        //                }
        //            }

        //            return vm;
        //        }
        //        return output.OrderBy(agv => agv.AGV_Name).ToList();
        //    }
        //    catch (Exception)
        //    {
        //        return null;
        //    }
        //}
        //
        private static List<ViewModel.WIPDataViewModel> GetWIPDataViewModels()
        {
            return StaEQPManagager.RacksList.Select(wip => new ViewModel.WIPDataViewModel()
            {
                WIPName = wip.EQName,
                Columns = wip.RackOption.Columns,
                Rows = wip.RackOption.Rows,
                Ports = wip.PortsStatus.ToList(),
                ColumnsTagMap = wip.RackOption.ColumnTagMap
            }).ToList();
        }
    }
}
