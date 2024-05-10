
using AGVSystemCommonNet6.Alarm;

namespace AGVSystem.Models.Automation
{
    public class AlarmReportSaveAutomation : AutomationBase
    {
        public override async Task<(bool result, string message)> AutomationTaskAsync()
        {
            try
            {
                var interval = _GetQueryInterval();
                DateTime starttime = interval.startTime;
                DateTime endtime = interval.endtime;
                string filePath = AlarmManagerCenter.SaveTocsv(starttime, endtime, "ALL", null);
                return (true, $"異常歷史自動匯出任務已完成->{filePath}");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        protected (DateTime startTime, DateTime endtime) _GetQueryInterval()
        {
            DateTime endTime = DateTime.Now;
            DateTime startTime = endTime;
            switch (this.options.Period)
            {
                case AutomationOptions.PERIOD.WEEKLY:
                    startTime = endTime.AddDays(-7);
                    break;
                case AutomationOptions.PERIOD.DAILY:
                    startTime = endTime.AddDays(-1);
                    break;
                case AutomationOptions.PERIOD.HOURLY:
                    startTime = endTime.AddHours(-1);
                    break;
                default:
                    break;
            }
            return (startTime, endTime);
        }
    }
}
