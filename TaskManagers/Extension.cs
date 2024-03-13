using AGVSystemCommonNet6;
using EquipmentManagment.Device;
using EquipmentManagment.Device.Options;
using static AGVSystemCommonNet6.clsEnums;
namespace AGVSystem.TaskManagers
{
    public static class Extension
    {

        public static EquipmentManagment.Device.Options.VEHICLE_TYPE ConvertToEQAcceptAGVTYPE(this clsEnums.AGV_TYPE agv_type)
        {
            switch (agv_type)
            {
                case clsEnums.AGV_TYPE.FORK: return EquipmentManagment.Device.Options.VEHICLE_TYPE.FORK;
                case clsEnums.AGV_TYPE.SUBMERGED_SHIELD: return EquipmentManagment.Device.Options.VEHICLE_TYPE.SUBMERGED_SHIELD;
                default: return EquipmentManagment.Device.Options.VEHICLE_TYPE.ALL;
            }
        }


        public static clsEnums.AGV_TYPE ToAGVModel(this EquipmentManagment.Device.Options.VEHICLE_TYPE agv_type)
        {
            switch (agv_type)
            {
                case EquipmentManagment.Device.Options.VEHICLE_TYPE.FORK:
                    return clsEnums.AGV_TYPE.FORK;
                case EquipmentManagment.Device.Options.VEHICLE_TYPE.SUBMERGED_SHIELD:
                    return clsEnums.AGV_TYPE.SUBMERGED_SHIELD;
                default:
                    return clsEnums.AGV_TYPE.FORK;
            }
        }
    }
}
