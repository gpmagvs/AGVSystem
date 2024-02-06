using AGVSystemCommonNet6;
using EquipmentManagment.Device;
using EquipmentManagment.Device.Options;
using static AGVSystemCommonNet6.clsEnums;
namespace AGVSystem.TaskManagers
{
    public static class Extension
    {

        public static EquipmentManagment.Device.Options.AGV_TYPE ConvertToEQAcceptAGVTYPE(this AGV_MODEL agv_type)
        {
            switch (agv_type)
            {
                case AGV_MODEL.FORK_AGV: return EquipmentManagment.Device.Options.AGV_TYPE.FORK;
                case AGV_MODEL.SUBMERGED_SHIELD: return EquipmentManagment.Device.Options.AGV_TYPE.SUBMERGED_SHIELD;
                default: return EquipmentManagment.Device.Options.AGV_TYPE.ALL;
            }
        }


        public static AGV_MODEL ToAGVModel(this EquipmentManagment.Device.Options.AGV_TYPE agv_type)
        {
            switch (agv_type)
            {
                case EquipmentManagment.Device.Options.AGV_TYPE.FORK:
                    return AGV_MODEL.FORK_AGV;
                case EquipmentManagment.Device.Options.AGV_TYPE.SUBMERGED_SHIELD:
                    return AGV_MODEL.SUBMERGED_SHIELD;
                default:
                    return AGV_MODEL.FORK_AGV;
            }
        }
    }
}
