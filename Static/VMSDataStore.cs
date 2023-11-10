using AGVSystemCommonNet6.Microservices.VMS;

namespace AGVSystem.Static
{
    public class VMSDataStore
    {
        public static Dictionary<string, clsAGVOptions> VehicleConfigs = new Dictionary<string, clsAGVOptions>();
        public static void Initialize()
        {
            var _VehicleConfigs = VMSSerivces.ReadVMSVehicleGroupSetting(@"C:\AGVS\Vehicle.json");
            VehicleConfigs = _VehicleConfigs.Values.SelectMany(cong => cong.AGV_List).ToDictionary(v => v.Key, v => v.Value);
        }
    }
}
