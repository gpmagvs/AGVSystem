﻿using AGVSystemCommonNet6.AGVDispatch.Messages;
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
using EquipmentManagment.WIP;
using System.Xml.Linq;
using EquipmentManagment.Device;

namespace AGVSystem.Service
{
    public class FrontEndDataCollectionBackgroundService : BackgroundService
    {
        private readonly IHubContext<FrontEndDataHub> _hubContext;
        public FrontEndDataCollectionBackgroundService(IHubContext<FrontEndDataHub> hubContext)
        {
            _hubContext = hubContext;
        }
        internal static object _previousData = new object();

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            clsPortOfRack.OnRackPortSensorStatusChanged += (sender, e) =>
            {
                _SendDataToClient(_previousData);
            };
            PortStatusAbstract.CarrierIDChanged += (sender, e) =>
            {
                _SendDataToClient(_previousData);
            };

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(100, stoppingToken);
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
                    HotRun = HotRunScriptManager.HotRunScripts,
                    UncheckedAlarm = AlarmManagerCenter.uncheckedAlarms,
                    TaskData = new { incompleteds = incompleted_tasks, completeds = completed_tasks }
                };

                try
                {
                    //use json string to compare the data is changed or not
                    if (!JsonConvert.SerializeObject(data).Equals(JsonConvert.SerializeObject(_previousData)))
                    {
                        await _SendDataToClient(data);
                        _previousData = data;
                        data = null;
                        continue;
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }
            async Task _SendDataToClient(object _data)
            {
                await _hubContext.Clients.All.SendAsync("ReceiveData", "VMS", _data);
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
