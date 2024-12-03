using AGVSystem.Models.Map;
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
                if (string.IsNullOrEmpty(port.CarrierID))
                {
                    port.CarrierID = await AGVSConfigulator.GetTrayUnknownFlowID();

                    if (port.IsRackPortIsEQ(out clsEQ eqInport))
                        eqInport.PortStatus.CarrierID = port.CarrierID;
                    await MCSCIMService.CarrierInstallCompletedReport(port.CarrierID, port.GetLocID(), port.GetParentRack().RackOption.DeviceID, 1);
                }
                else
                {

                }
                ZoneCapacityChangeEventReport(port.GetParentRack());
            });
        }
        private static void HandlePortCargoChangeToDisappear(object? sender, clsPortOfRack port)
        {
            ZoneCapacityChangeEventReport(port.GetParentRack());
        }
    }
}
