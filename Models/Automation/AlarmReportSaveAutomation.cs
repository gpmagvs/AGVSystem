
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
                string filePath = AlarmManagerCenter.AutoSaveTocsv(starttime, endtime, "ALL", null);
                return (true, $"異常歷史自動匯出任務已完成->{filePath}");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        protected (DateTime startTime, DateTime endtime) _GetQueryInterval()
        {
            DateTime originalDate = DateTime.Now;
            switch (this.options.Period)
            {
                case AutomationOptions.PERIOD.WEEKLY:
                    originalDate = originalDate.AddDays(-7);
                    break;
                case AutomationOptions.PERIOD.DAILY:
                    originalDate = originalDate.AddDays(-1);
                    break;
                case AutomationOptions.PERIOD.HOURLY:
                    originalDate = originalDate.AddHours(-1);
                    break;
                default:
                    break;
            }
            DateTime endTime = new DateTime(originalDate.Year, originalDate.Month, originalDate.Day, 23, 59, 59);
            DateTime startTime = new DateTime(originalDate.Year, originalDate.Month, originalDate.Day, 00, 00, 00);
            return (startTime, endTime);
        }
    }
}
