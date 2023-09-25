using AGVSystem.Models.BayMeasure;
using AGVSystem.Models.TaskAllocation.HotRun;
using Newtonsoft.Json;

namespace AGVSystem.TaskManagers
{
    public class ScheduleMeasureManager
    {

        public const string ConfigFileName = "ScheduleMeasureScripts.json";
        public static string ConfigFileFullName
        {
            get
            {
                var folder = "C://AGVS";
                Directory.CreateDirectory(folder);
                var filename = Path.Combine(folder, ConfigFileName);
                return filename;
            }
        }
        public static List<clsMeasureScript> ScheduleMeasureList = new List<clsMeasureScript>();

        internal static void Initialize()
        {
            ReloadScheduleMeasureList();
        }

        internal static void ReloadScheduleMeasureList()
        {

            if (File.Exists(ConfigFileFullName))
            {
                ScheduleMeasureList = JsonConvert.DeserializeObject<List<clsMeasureScript>>(File.ReadAllText(ConfigFileFullName));
            }
            File.WriteAllText(ConfigFileFullName, JsonConvert.SerializeObject(ScheduleMeasureList, Formatting.Indented));
        }
        internal static bool AddNewSchedule(clsMeasureScript schedule)
        {
            ScheduleMeasureList.Add(schedule);
            File.WriteAllText(ConfigFileFullName, JsonConvert.SerializeObject(ScheduleMeasureList, Formatting.Indented));
            return true;
        }
    }
}
