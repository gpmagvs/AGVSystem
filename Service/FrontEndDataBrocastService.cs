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
        RackService rackSerivce;
        ILogger<FrontEndDataBrocastService> logger;
        public FrontEndDataBrocastService(IHubContext<FrontEndDataHub> hubContext, ILogger<FrontEndDataBrocastService> logger, IServiceScopeFactory serviceScopeFactory)
        {
            this.rackSerivce = serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<RackService>();
            _hubContext = hubContext;
            this.logger = logger;
        }
        internal static object _previousData = new object();
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            HotRunScriptManager.FrontendHub = this._hubContext;
            DatabaseCaches.Alarms.OnUnCheckedAlarmsChanged += HandleUnCheckedAlarmsChanged;
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
                                 ChargeStationData = StaEQPManagager.GetChargeStationStates(),
                             },
                             VMSAliveCheck = VMSSerivces.IsAlive,
                             AGVLocationUpload = AGVSMapManager.AGVUploadCoordinationStore,
                             HotRun = HotRunScriptManager.HotRunScripts,
                             TaskData = new { incompleteds = incompleted_tasks, completeds = completed_tasks }
                             //UncheckedAlarm = AlarmManagerCenter.uncheckedAlarms,
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

        private void HandleUnCheckedAlarmsChanged(object? sender, List<clsAlarmDto> e)
        {
            Task.Factory.StartNew(async () =>
            {
                await _hubContext.Clients.All.SendAsync("UnCheckedAlarms", e);
            });
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
