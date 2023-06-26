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
    }
}
