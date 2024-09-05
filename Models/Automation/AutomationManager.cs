using AGVSystemCommonNet6;
using AGVSystemCommonNet6.Configuration;
using Microsoft.Extensions.Options;
using NLog;

namespace AGVSystem.Models.Automation
{
    public static class AutomationManager
    {
        public static List<AutomationBase> AutomationTasks { get; private set; } = new List<AutomationBase>();

        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public static void AddAutomationTask(AutomationBase automation)
        {
            AutomationTasks.Add(automation);
        }
        public static void StartAllAutomationTasks()
        {
            foreach (AutomationBase automation in AutomationTasks)
            {
                automation.OnAutomationTaskStart += Automation_OnAutomationTaskStart;
                automation.OnAutomationTaskExecuteSuccess += Automation_OnAutomationTaskExecuteSuccess;
                automation.OnAutomationTaskExecuteFail += Automation_OnAutomationTaskExecuteFail;
                automation.Start();
            }
        }

        internal static void InitialzeDefaultTasks()
        {
            AddAutomationTask(new AlarmReportSaveAutomation()
            {
                options = new AutomationOptions()
                {
                    TaskName = "Alarm Report Save Automaiton",
                    Period = AutomationOptions.PERIOD.DAILY,
                    HourInDaily = AGVSConfigulator.SysConfigs.AutoSendDailyData.SaveTime,
                    ImmediateStart = false,
                }
            });


            AddAutomationTask(new TaskHistoryReportOutAutomation()
            {
                options = new AutomationOptions()
                {
                    TaskName = "Task History Report Save Automaiton",
                    Period = AutomationOptions.PERIOD.DAILY,
                    HourInDaily = AGVSConfigulator.SysConfigs.AutoSendDailyData.SaveTime,
                    ImmediateStart = false,
                }
            });
            AddAutomationTask(new AvailabilitysSaveAutomation()
            {
                options = new AutomationOptions()
                {
                    TaskName = "Availabilitys Report Save Automaiton",
                    Period = AutomationOptions.PERIOD.DAILY,
                    HourInDaily = AGVSConfigulator.SysConfigs.AutoSendDailyData.SaveTime,
                    ImmediateStart = false,
                }
            });
        }

        private static void Automation_OnAutomationTaskStart(object? sender, EventArgs e)
        {
            AutomationBase automation = (AutomationBase)sender;
            _logger.Info($"Automation Task-{automation.options.TaskName} START. {automation.options.ToJson(Newtonsoft.Json.Formatting.None)}");
        }

        private static void Automation_OnAutomationTaskExecuteFail(object? sender, string message)
        {
            AutomationBase automation = (AutomationBase)sender;
            _logger.Error(automation.options.TaskName + $"Task Executed Fail-{message}");
        }

        private static void Automation_OnAutomationTaskExecuteSuccess(object? sender, string message)
        {
            AutomationBase automation = (AutomationBase)sender;
            _logger.Info(automation.options.TaskName + $"Task Executed Success=>{message}");
        }
    }
}
