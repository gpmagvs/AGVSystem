using AGVSystemCommonNet6.AGVDispatch.Messages;
using Microsoft.AspNetCore.SignalR;
using static AGVSystemCommonNet6.clsEnums;
using AGVSystemCommonNet6.MAP;
using AGVSystemCommonNet6;
using AGVSystem.Models.Map;
using AGVSystem.Models.TaskAllocation.HotRun;
using AGVSystemCommonNet6.Alarm;
using AGVSystemCommonNet6.Microservices.VMS;
using EquipmentManagment.Manager;
using AGVSystemCommonNet6.DATABASE;

namespace AGVSystem.Service
{
    public class FrontEndDataCollectionBackgroundService : BackgroundService
    {
        private readonly IHubContext<FrontEndDataHub> _hubContext;
        public FrontEndDataCollectionBackgroundService(IHubContext<FrontEndDataHub> hubContext)
        {
            _hubContext = hubContext;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var incompleted_tasks = DatabaseCaches.TaskCaches.InCompletedTasks;
                var completed_tasks = DatabaseCaches.TaskCaches.CompleteTasks;
                var data = new
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
                await _hubContext.Clients.All.SendAsync("ReceiveData", "VMS", data);
                await Task.Delay(150, stoppingToken);
            }
        }
        private List<ViewModel.WIPDataViewModel> GetWIPDataViewModels()
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
