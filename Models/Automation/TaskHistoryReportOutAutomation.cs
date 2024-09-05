
using AGVSystemCommonNet6.DATABASE.Helpers;

namespace AGVSystem.Models.Automation
{
    public class TaskHistoryReportOutAutomation : AlarmReportSaveAutomation
    {
        public override async Task<(bool result, string message)> AutomationTaskAsync()
        {
            try
            {
                var interval = _GetQueryInterval();
                DateTime starttime = interval.startTime;
                DateTime endtime = interval.endtime;
                var filePath = TaskDatabaseHelper.AutoSaveTocsv(starttime, endtime);
                return (true, $"任務歷史自動匯出任務已完成->{filePath}");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
