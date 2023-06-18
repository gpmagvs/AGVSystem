using EquipmentManagment;

namespace AGVSystem.Models
{
    public class clsAGVSConfigs
    {
        public string ConfigFolder { get; set; } = @"C:\AGVS_Configs";
        public clsEQManagementConfigs EqManagementConfigs { get; set; } = new clsEQManagementConfigs()
        {
            EQConfigPath = "EQConfigs.json",
            WIPConfigPath = "WIPConfigs.json"
        };
    }
}
