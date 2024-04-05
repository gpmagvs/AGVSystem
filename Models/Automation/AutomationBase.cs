
using Microsoft.Build.ObjectModelRemoting;
using System.Net.Http.Headers;

namespace AGVSystem.Models.Automation
{
    public abstract class AutomationBase
    {
        public AutomationOptions options { get; set; } = new AutomationOptions();
        public event EventHandler OnAutomationTaskStart;
        public event EventHandler<string> OnAutomationTaskExecuteFail;
        public event EventHandler<string> OnAutomationTaskExecuteSuccess;
        public async Task Start()
        {

            OnAutomationTaskStart?.Invoke(this, EventArgs.Empty);
            if (options.ImmediateStart)
            {
                ExecuteTaskAndInvokeReulst();
            }
            while (true)
            {
                if (_IsTimeMatch())
                {
                    ExecuteTaskAndInvokeReulst();
                }
                await Task.Delay(_GetDelayTimespan());
            }
        }

        public abstract Task<(bool result, string message)> AutomationTaskAsync();

        private async Task ExecuteTaskAndInvokeReulst()
        {
            (bool result, string message) result = await AutomationTaskAsync();
            _InvokeAutomationResult(result);
        }
        private void _InvokeAutomationResult((bool reuslt, string message) executeResult)
        {
            if (executeResult.reuslt)
            {
                OnAutomationTaskExecuteSuccess?.Invoke(this, executeResult.message);
            }
            else
            {
                OnAutomationTaskExecuteFail?.Invoke(this, executeResult.message);
            }
        }

        private bool _IsTimeMatch()
        {
            switch (options.Period)
            {
                case AutomationOptions.PERIOD.WEEKLY:
                    return (int)DateTime.Now.DayOfWeek == options.DayInWeekly;
                case AutomationOptions.PERIOD.DAILY:
                    return (int)DateTime.Now.Hour == options.HourInDaily;
                case AutomationOptions.PERIOD.HOURLY:
                    return (int)DateTime.Now.Minute == options.MintInHOURLY;
                default:
                    return false;
            }
        }
        private TimeSpan _GetDelayTimespan()
        {
            switch (options.Period)
            {
                case AutomationOptions.PERIOD.WEEKLY:
                    return TimeSpan.FromDays(1);

                case AutomationOptions.PERIOD.DAILY:
                    return TimeSpan.FromHours(1);
                case AutomationOptions.PERIOD.HOURLY:
                    return TimeSpan.FromMinutes(1);
                default:
                    return TimeSpan.FromHours(1);
            }
        }
    }

    public class AutomationOptions
    {
        public string TaskName { get; set; } = "Automation task";
        /// <summary>
        /// 0:Sunday, 1:Monday ... , 6:Saturday
        /// </summary>
        public int DayInWeekly { get; set; } = 1;
        public int HourInDaily { get; set; } = 0;

        public int MintInHOURLY { get; set; } = 0;

        public PERIOD Period { get; set; } = PERIOD.DAILY;

        /// <summary>
        /// 首次啟動流程是否先執行一次任務
        /// </summary>

        public bool ImmediateStart { get; set; } = false;
        public enum PERIOD
        {
            WEEKLY,
            DAILY,
            HOURLY,
        }
    }
}
