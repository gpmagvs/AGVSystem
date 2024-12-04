using AGVSystem.TaskManagers;
using AGVSystemCommonNet6.AGVDispatch;
using AGVSystemCommonNet6.AGVDispatch.Messages;
using AGVSystemCommonNet6.AGVDispatch.Model;

namespace AGVSystem.Models.BayMeasure
{
    public class clsMeasureScript
    {
        public string ScriptName { get; set; } = "";

        public string Time { get; set; } = "00:00";
        public string AGVName { get; set; } = "AGV_001";

        public List<clsBayMeasure> Bays { get; set; } = new List<clsBayMeasure>();

        internal string key
        {
            get
            {
                return $"{AGVName}-{Time}";
            }
        }
        internal CancellationTokenSource StopTraceCts = new CancellationTokenSource();

        internal async void CreateTasks()
        {
            foreach (var bay in Bays)
            {
                var taskDto = new clsTaskDto
                {
                    Action = ACTION_TYPE.Measure,
                    DispatcherName = $"排程",
                    DesignatedAGVName = AGVName,
                    To_Station = bay.BayName,
                    Priority = 9,
                    TaskName = $"Measure_{DateTime.Now.ToString("yyyyMMdd_HHmmssffff")}",
                    RecieveTime = DateTime.Now
                };
                await TaskManager.AddTask(taskDto, TaskManager.TASK_RECIEVE_SOURCE.LOCAL_Auto);
            }
        }
    }
}
