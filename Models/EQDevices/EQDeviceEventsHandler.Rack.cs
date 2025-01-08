using AGVSystem.Models.Map;
using AGVSystem.Service;
using AGVSystemCommonNet6.Alarm;
using AGVSystemCommonNet6.Configuration;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.Log;
using AGVSystemCommonNet6.MAP;
using AGVSystemCommonNet6.Microservices;
using AGVSystemCommonNet6.Microservices.MCS;
using AGVSystemCommonNet6.Microservices.VMS;
using AGVSystemCommonNet6.Notify;
using EquipmentManagment.Device;
using EquipmentManagment.MainEquipment;
using EquipmentManagment.WIP;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Diagnostics;

namespace AGVSystem.Models.EQDevices
{
    public partial class EQDeviceEventsHandler
    {
        private static async void HandlePortOfRackSensorFlash(object? sender, (clsRack rack, clsPortOfRack port) e)
        {
            BrocastRackData();
            string rackName = e.rack.EQName;
            string portId = e.port.Properties.ID;
            AlarmManagerCenter.AddAlarmAsync(ALARMS.Rack_Port_Sensor_Flash, ALARM_SOURCE.EQP, ALARM_LEVEL.WARNING, rackName, portId);
            NotifyServiceHelper.WARNING($"[{rackName}]-Port:{portId} Sensor Flash!");
        }


        private static void HandlePortOfRackSensorStatusChanged(object? sender, (clsRack rack, clsPortOfRack port) e)
        {
            BrocastRackData();
            string rackName = e.rack.EQName;
            string portId = e.port.Properties.ID;
            AlarmManagerCenter.SetAlarmCheckedAsync(rackName, portId, ALARMS.Rack_Port_Sensor_Flash);
        }


        private static void HandlePortCargoChangedToExist(object? sender, clsPortOfRack port)
        {
            string portLocID = port.GetLocID();

            bool _isTransferActionRunning = DatabaseCaches.TaskCaches.RunningTasks.Any(order => order.destinePortID == (portLocID) && order.currentProgress == AGVSystemCommonNet6.AGVDispatch.VehicleMovementStage.WorkingAtDestination);

            if (_isTransferActionRunning)
            {
                NotifyServiceHelper.INFO($"{portLocID} Carrier Placed by AGV!");
            }
            else
            {
                Task.Factory.StartNew(async () =>
                {
                    string locID = port.GetLocID();
                    string zoneID = port.GetParentRack().RackOption.DeviceID;

                    bool isTraySensorOn = port.MaterialExistSensorStates.Any(pair => (pair.Value == clsPortOfRack.SENSOR_STATUS.ON || pair.Value == clsPortOfRack.SENSOR_STATUS.FLASH)
                                                                                    && (pair.Key == clsPortOfRack.SENSOR_LOCATION.TRAY_1 || pair.Key == clsPortOfRack.SENSOR_LOCATION.TRAY_2));

                    string tunid = isTraySensorOn ? await AGVSConfigulator.GetTrayUnknownFlowID() : await AGVSConfigulator.GetRackUnknownFlowID();

                    if (string.IsNullOrEmpty(port.CarrierID))
                    {
                        await Task.Delay(600);
                        UpdateCarrierID(tunid);
                        NotifyServiceHelper.INFO($"{portLocID}因非搬運過程但偵測到在席有貨,已生成未知帳-{tunid}");
                        await MCSCIMService.CarrierInstallCompletedReport(tunid, locID, zoneID, 0);
                        await Task.Delay(100);
                        BrocastRackData();
                    }
                    await ZoneCapacityChangeEventReport(port.GetParentRack());
                });

            }
            BrocastRackData();

            void UpdateCarrierID(string tunid)
            {
                port.InstallBy = PortStatusAbstract.CARRIER_SOURCE.MANUAL;
                port.CarrierID = tunid;
                port.InstallTime = DateTime.Now;
                if (port.IsRackPortIsEQ(out clsEQ eqInport))
                {
                    eqInport.PortStatus.CarrierID = port.CarrierID;
                    eqInport.PortStatus.InstallTime = port.InstallTime;
                    eqInport.PortStatus.InstallBy = port.InstallBy;
                }

                rackService.UpdateRackPortCarrierIDOfDatabase(port, tunid);
            }
        }


        /// <summary>
        /// Rack Port 貨物消失時的處置(不用carrier remove,因為可能是貨物被手動搬走，須把carrier id留著)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="port"></param>
        private static void HandlePortCargoChangeToDisappear(object? sender, clsPortOfRack port)
        {
            BrocastRackData();
            Task.Factory.StartNew(async () =>
            {
                bool hasCarrierID = !string.IsNullOrEmpty(port.CarrierID);

                if (hasCarrierID)
                {
                    //TODO 系統提示 [有帳無料]
                    NotifyServiceHelper.WARNING($"[{port.GetParentRack().RackOption.Name} - Port:{port.PortNo}] 現在有帳無料!");
                }

                await ZoneCapacityChangeEventReport(port.GetParentRack());
            });
        }


    }
}
