using System.Configuration;

namespace AGVSystem
{
    public class AppSettings
    {
        private const string SYS_CONFIG_SECTION_KEY = "SystemConfigs";
        private static IConfiguration _config
        {
            get
            {
                ConfigurationBuilder builder = new ConfigurationBuilder();
                builder.SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
                return builder.Build();
            }
        }
        public static bool Public => _config.GetValue<bool>($"{SYS_CONFIG_SECTION_KEY}:Public");
        public static string VMSHost => _config.GetConnectionString("VMS_HOST");
        public static string DBConnection => _config.GetConnectionString("DefaultConnection");
        public static bool UseEQEmu => _config.GetValue<bool>($"{SYS_CONFIG_SECTION_KEY}:UseEQEmu");
        public static string EquipmentManagementConfigFolder => _config.GetValue<string>($"{SYS_CONFIG_SECTION_KEY}:EquipmentManagementConfigFolder");
        public static string MapFile => _config.GetValue<string>($"{SYS_CONFIG_SECTION_KEY}:MapFile");
        public static string MapRegionConfigFile => _config.GetValue<string>($"{SYS_CONFIG_SECTION_KEY}:MapRegionConfigFile");
    }
}
