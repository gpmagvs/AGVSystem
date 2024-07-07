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
                Ports = wip.PortsStatus.ToList(),
                ColumnsTagMap = wip.RackOption.ColumnTagMap
            }).ToList();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _ = Task.Run(async () =>
             {
                 bool _foring = false;
                 while (true)
                 {
                     await Task.Delay(350);
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
                         await _hubContext.Clients.All.SendAsync("ReceiveData", "VMS", data);

                         //if (_foring || !JsonConvert.SerializeObject(data).Equals(JsonConvert.SerializeObject(_previousData)))
                         //{
                         //    _foring = false;
                         //    _previousData = data;
                         //    _ = Task.Factory.StartNew(async () =>
                         //    {
                         //        await _hubContext.Clients.All.SendAsync("ReceiveData", "VMS", data);
                         //    });
                         //    continue;
                         //}
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
