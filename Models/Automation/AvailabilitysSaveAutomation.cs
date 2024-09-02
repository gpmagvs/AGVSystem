using AGVSystemCommonNet6.Configuration;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.DATABASE.Helpers;
using NuGet.Packaging;
using System.Data;
using System.Text;
using static Microsoft.AspNetCore.Razor.Language.TagHelperMetadata;
namespace AGVSystem.Models.Automation
{
    public class AvailabilitysSaveAutomation : TaskHistoryReportOutAutomation
    {
        public override async Task<(bool result, string message)> AutomationTaskAsync()
        {
            try
            {
                var interval = _GetQueryInterval();
                DateTime startDate = interval.startTime;
                DateTime endDate = interval.endtime;

                string fileName = null;
                string YesterdayDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
                var folder = Path.Combine(Environment.CurrentDirectory, AGVSConfigulator.SysConfigs.AutoSendDailyData.SavePath + YesterdayDate);
                var _fileName = fileName is null ? YesterdayDate + ".csv" : fileName;
                Directory.CreateDirectory(folder);
                using var db = new AGVSDatabase();
                var AGV_Name = db.tables.AgvStates.Select(AGVName => AGVName.AGV_Name);
                for (int i = 0; i <= AGV_Name.Count(); i++)
                {
                    string AGVName = AGV_Name.Skip(i).FirstOrDefault();
                    if (AGVName != null)
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Columns.Add("AGV名稱");
                        dataTable.Columns.Add("IDLE時間");
                        dataTable.Columns.Add("RUN時間");
                        dataTable.Columns.Add("DOWN時間");
                        dataTable.Columns.Add("Charge時間");
                        string FilePath = Path.Combine(folder, AGVName+"_" + _fileName);
                        var datas = db.tables.Availabilitys.Where(dat => dat.AGVName == AGVName && dat.Time >= startDate && dat.Time <= endDate);
                        var idle_time = datas.Sum(d => d.IDLE_TIME);
                        var run_time = datas.Sum(d => d.RUN_TIME);
                        var down_time = datas.Sum(d => d.DOWN_TIME);
                        var charge_time = datas.Sum(d => d.CHARGE_TIME);
                        List<string> list = new List<string> { ",,,," };
                        foreach (var data in list)
                        {
                            dataTable.Rows.Add(AGVName, idle_time, run_time, down_time, charge_time);
                        }
                        WriteDataTableToCsv(dataTable, FilePath, Encoding.UTF8);
                        //File.WriteAllLines(FilePath, list, Encoding.UTF8);
                    }
                    else {  }
                }
                return (true, $"每日AGV稼動率自動匯出任務已完成->");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        private void WriteDataTableToCsv(DataTable dataTable, string filePath, Encoding encoding)
        {
            using (StreamWriter writer = new StreamWriter(filePath, false, encoding))
            {
                writer.WriteLine(string.Join(",", dataTable.Columns.Cast<DataColumn>().Select(col => EscapeCsvField(col.ColumnName))));

                // 寫出每一列的資料
                foreach (DataRow row in dataTable.Rows)
                {
                    writer.WriteLine(string.Join(",", row.ItemArray.Select(field => EscapeCsvField(field.ToString()))));
                }
            }
        }
        private string EscapeCsvField(string field)
        {
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            {
                // 使用雙引號包裹並且將內部的雙引號轉義
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }
            else
            {
                return field;
            }
        }
    }
}
