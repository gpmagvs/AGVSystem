using System.Configuration;

namespace AGVSystem
{
    public class AppSettings
    {


        private static IConfiguration _config
        {
            get
            {
                ConfigurationBuilder builder = new ConfigurationBuilder();
                builder.SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
                return builder.Build();
            }
        }

        public static string VMSHost => _config.GetConnectionString("VMS_HOST");
        public static bool UseEQEmu => _config.GetValue<bool>("SystemConfigs:UseEQEmu");
        public static string EquipmentManagementConfigFolder => _config.GetValue<string>("SystemConfigs:EquipmentManagementConfigFolder");
        public static string MapFile => _config.GetValue<string>("SystemConfigs:MapFile");
        public static string MapRegionConfigFile => _config.GetValue<string>("SystemConfigs:MapRegionConfigFile");
    }
}
