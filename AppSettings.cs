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
    }
}
