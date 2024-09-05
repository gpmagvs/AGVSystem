
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
        public int retryCnt { get; internal set; } = 0;

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
                    retryCnt = 0;
                    ExecuteTaskAndInvokeReulst();
                }
                await Task.Delay(_GetDelayTimespan());
            }
        }

        public abstract Task<(bool result, string message)> AutomationTaskAsync();

        public async Task<(bool result, string message)> ExecuteTaskAndInvokeReulst(bool autoRetry = true)
        {
            (bool success, string message) result = await AutomationTaskAsync();
            if (!result.success && autoRetry)
            {
                if (retryCnt < options.retryCntMax)
                {
                    result.message += $"(即將在{options.retryIntervalTimeSpan.Seconds}秒後重新嘗試執行-{retryCnt + 1})";
                    _InvokeAutomationResult(result);

                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(options.retryIntervalTimeSpan);
                        retryCnt++;
                        ExecuteTaskAndInvokeReulst();
                    });

                    return result;
                }
                else
                {
                    result.message += $"重新嘗試執行自動化任務已達上限({options.retryCntMax}))";
                    _InvokeAutomationResult(result);
                    retryCnt = 0;
                    return result;
                }
            }
            retryCnt = 0;
            _InvokeAutomationResult(result);
            return result;
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

        public int retryCntMax = 10;
        public TimeSpan retryIntervalTimeSpan = TimeSpan.FromSeconds(3);

        public enum PERIOD
        {
            WEEKLY,
            DAILY,
            HOURLY,
        }
    }
}
