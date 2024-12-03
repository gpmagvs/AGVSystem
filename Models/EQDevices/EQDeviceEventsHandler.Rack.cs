﻿using AGVSystem.Models.Map;
using AGVSystem.Service;
using AGVSystemCommonNet6.Alarm;
using AGVSystemCommonNet6.Configuration;
using AGVSystemCommonNet6.Log;
using AGVSystemCommonNet6.MAP;
using AGVSystemCommonNet6.Microservices;
using AGVSystemCommonNet6.Microservices.MCS;
using AGVSystemCommonNet6.Microservices.VMS;
using AGVSystemCommonNet6.Notify;
using EquipmentManagment.Device;
using EquipmentManagment.MainEquipment;
using EquipmentManagment.WIP;
using System;
using System.Diagnostics;

namespace AGVSystem.Models.EQDevices
{
    public partial class EQDeviceEventsHandler
    {
        private static async void HandlePortOfRackSensorFlash(object? sender, (clsRack rack, clsPortOfRack port) e)
        {
            string rackName = e.rack.EQName;
            string portId = e.port.Properties.ID;
            AlarmManagerCenter.AddAlarmAsync(ALARMS.Rack_Port_Sensor_Flash, ALARM_SOURCE.EQP, ALARM_LEVEL.WARNING, rackName, portId);
            NotifyServiceHelper.WARNING($"[{rackName}]-Port:{portId} Sensor Flash!");
        }


        private static void HandlePortOfRackSensorStatusChanged(object? sender, (clsRack rack, clsPortOfRack port) e)
        {
            string rackName = e.rack.EQName;
            string portId = e.port.Properties.ID;
            AlarmManagerCenter.SetAlarmCheckedAsync(rackName, portId, ALARMS.Rack_Port_Sensor_Flash);
        }


        private static void HandlePortCargoChangedToExist(object? sender, clsPortOfRack port)
        {
            Task.Factory.StartNew(async () =>
            {
                string locID = port.GetLocID();
                string zoneID = port.GetParentRack().RackOption.DeviceID;
                string tunid = await AGVSConfigulator.GetTrayUnknownFlowID();

                if (string.IsNullOrEmpty(port.CarrierID))
                    UpdateCarrierID(tunid);
                await ZoneCapacityChangeEventReport(port.GetParentRack());
            });



            void UpdateCarrierID(string tunid)
            {
                port.CarrierID = tunid;
                if (port.IsRackPortIsEQ(out clsEQ eqInport))
                    eqInport.PortStatus.CarrierID = port.CarrierID;
            }
        }


        /// <summary>
        /// Rack Port 貨物消失時的處置(不用carrier remove,因為可能是貨物被手動搬走，須把carrier id留著)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="port"></param>
        private static void HandlePortCargoChangeToDisappear(object? sender, clsPortOfRack port)
        {
            Task.Factory.StartNew(async () =>
            {
                bool hasCarrierID = string.IsNullOrEmpty(port.CarrierID);

                if (hasCarrierID)
                {
                    //TODO 系統提示 [有帳無料]
                }

                await ZoneCapacityChangeEventReport(port.GetParentRack());
            });
        }
    }
}
