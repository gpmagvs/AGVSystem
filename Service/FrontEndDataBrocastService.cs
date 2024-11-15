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
using Newtonsoft.Json;
using NLog.Time;

namespace AGVSystem.Service
{
    public class FrontEndDataBrocastService : IHostedService
    {
        private readonly IHubContext<FrontEndDataHub> _hubContext;
        ILogger<FrontEndDataBrocastService> logger;
        public FrontEndDataBrocastService(IHubContext<FrontEndDataHub> hubContext, ILogger<FrontEndDataBrocastService> logger)
        {
            _hubContext = hubContext;
            this.logger = logger;
        }
        internal static object _previousData = new object();

        private List<ViewModel.WIPDataViewModel> GetWIPDataViewModels()
        {
            return StaEQPManagager.RacksList.Select(wip => new ViewModel.WIPDataViewModel()
            {
                WIPName = wip.EQName,
                Columns = wip.RackOption.Columns,
                Rows = wip.RackOption.Rows,
                Ports = wip.RackOption.MaterialInfoFromEquipment ? wip.GetPortStatusWithEqInfo().ToList() : wip.PortsStatus.ToList(),
                ColumnsTagMap = wip.RackOption.ColumnTagMap,
                IsOvenAsRacks = wip.RackOption.MaterialInfoFromEquipment
            }).OrderBy(wip => wip.WIPName).ToList();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            HotRunScriptManager.FrontendHub = this._hubContext;


            _ = Task.Run(async () =>
             {
                 bool _foring = false;
                 while (true)
                 {
                     await Task.Delay(350);
                     if (!FrontEndDataHub.connectedClients.Any()) //當沒有任何客戶端連線，則不用創建數據去發布
                         continue;
                     try
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
                             TaskData = new { incompleteds = incompleted_tasks, completeds = completed_tasks }
                         };
                         //use json string to compare the data is changed or not                        
                         _hubContext.Clients.All.SendAsync("ReceiveData", "VMS", data);
                     }
                     catch (Exception ex)
                     {
                         logger.LogError(ex, "FrontEndDataBrocastService Error");
                         await _hubContext.Clients.All.SendAsync("Notify", "FrontEndDataBrocastService Error:" + ex.Message);
                         await Task.Delay(700);
                         continue;
                     }
                 }
             });

        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
