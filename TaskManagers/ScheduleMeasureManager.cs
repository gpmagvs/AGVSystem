using AGVSystem.Models.BayMeasure;
using AGVSystem.Models.TaskAllocation.HotRun;
using AGVSystemCommonNet6.Log;
using Newtonsoft.Json;
using System.Globalization;

namespace AGVSystem.TaskManagers
{
    public class ScheduleMeasureManager
    {

        public const string ConfigFileName = "ScheduleMeasureScripts.json";
        private static CancellationTokenSource stop_schedule_trace = new CancellationTokenSource();
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
            if (DateTime.TryParseExact(schedule.Time, "yyyy/MM/dd HH:mm:ss", CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces, out DateTime dt))
            {
                schedule.Time = dt.ToString("HH:mm");
            }
            else if (DateTime.TryParseExact(schedule.Time, "yyyy/MM/dd HH:mm", CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces, out dt))
            {
                schedule.Time = dt.ToString("HH:mm");
            }
            var index = ScheduleMeasureList.FindIndex(s => s.key == schedule.key);
            if (index == -1)
            {
                ScheduleMeasureList.Add(schedule);
                StartTracceSchedule(schedule);
            }
            else
            {
                ScheduleMeasureList[index].StopTraceCts.Cancel();
                ScheduleMeasureList[index] = schedule;
                StartTracceSchedule(schedule);
            }

            File.WriteAllText(ConfigFileFullName, JsonConvert.SerializeObject(ScheduleMeasureList, Formatting.Indented));
            return true;
        }

        /// <summary>
        /// 修改排程設定
        /// </summary>
        /// <param name="time"></param>
        /// <param name="agv_name"></param>
        /// <param name="new_schedule"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        internal static bool ModifySchedule(string time, string agv_name, clsMeasureScript new_schedule)
        {
            var index = ScheduleMeasureList.FindIndex(s => s.Time == time && s.AGVName == agv_name);
            if (index == -1)
                return false;
            else
            {
                ScheduleMeasureList[index].StopTraceCts.Cancel();
                ScheduleMeasureList[index] = new_schedule;
            }

            File.WriteAllText(ConfigFileFullName, JsonConvert.SerializeObject(ScheduleMeasureList, Formatting.Indented));
            return true;
        }
        internal static bool DeleteSchedule(string time, string agv_name)
        {
            var index = ScheduleMeasureList.FindIndex(s => s.Time == time && s.AGVName == agv_name);
            if (index == -1)
                return true;
            else
            {
                ScheduleMeasureList[index].StopTraceCts.Cancel();
                ScheduleMeasureList.RemoveAt(index);
            }

            File.WriteAllText(ConfigFileFullName, JsonConvert.SerializeObject(ScheduleMeasureList, Formatting.Indented));
            return true;
        }
        internal static void StopScheduleTrace()
        {
            stop_schedule_trace.Cancel();
            foreach (var schedule in ScheduleMeasureList)
            {
                schedule.StopTraceCts.Cancel();
            }
        }
        internal static async void StartSchedules()
        {
            StopScheduleTrace();
            await Task.Delay(1000);
            LOG.INFO($"Start 排程");
            ScheduleMeasureList.ForEach(schedule =>
            {
                StartTracceSchedule(schedule);
            });
        }

        private static void StartTracceSchedule(clsMeasureScript schedule)
        {
            var hour = int.Parse(schedule.Time.Split(':')[0]);
            var min = int.Parse(schedule.Time.Split(':')[1]);
            DateTime start_measure_time = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hour, min, 00);
            bool isScheduleAlreadyPassed = DateTime.Now > start_measure_time;
            if (isScheduleAlreadyPassed)
                start_measure_time = start_measure_time.AddDays(1);

            double wait_min = (start_measure_time - DateTime.Now).TotalMinutes;
            Task.Factory.StartNew(async () =>
            {
                CancellationTokenSource cancel_wait = new CancellationTokenSource();
                cancel_wait.CancelAfter(TimeSpan.FromMinutes(wait_min));
                LOG.INFO($"等待排程-{start_measure_time.ToString()}({schedule.AGVName}) 開始");
                while (!cancel_wait.IsCancellationRequested)
                {
                    if (stop_schedule_trace.IsCancellationRequested | schedule.StopTraceCts.IsCancellationRequested)
                    {
                        LOG.WARN($"{schedule.key} 排程取消({(schedule.StopTraceCts.IsCancellationRequested?"Stanalone":"ALL")})");
                        return;
                    }
                    await Task.Delay(1000);
                }

                LOG.INFO($"Start 排程-{start_measure_time.ToString()}(${schedule.AGVName})");
                schedule.CreateTasks();

                await Task.Delay(1000);
                StartTracceSchedule(schedule);

            });
        }

    }
}
