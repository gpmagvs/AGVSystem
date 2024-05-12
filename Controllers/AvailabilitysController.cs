
using AGVSystemCommonNet6.DATABASE;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AGVSystemCommonNet6.Availability;
using AGVSystem.Service;
using System.Diagnostics;

namespace AGVSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AvailabilitysController : ControllerBase
    {
        private readonly AGVSDbContext _DbContext;
        private readonly MeanTimeQueryService _MeanTimeQuerier;

        public AvailabilitysController(AGVSDbContext DbContext, MeanTimeQueryService MeanTimeQuerier)
        {
            this._DbContext = DbContext;
            this._MeanTimeQuerier = MeanTimeQuerier;
        }

        [HttpGet("Today")]
        public async Task<IActionResult> Today()
        {
            var agv_names = _DbContext.RealTimeAvailabilitys.Where(d => d.StartTime.Date == DateTime.Now.Date).Select(d => d.AGVName).Distinct().ToList();
            agv_names.Sort();
            Dictionary<string, object> output = new Dictionary<string, object>();
            foreach (var name in agv_names)
            {
                IQueryable<AGVSystemCommonNet6.Availability.RTAvailabilityDto> dataQueryOut = _DbContext.RealTimeAvailabilitys.Where(d => d.AGVName == name && d.StartTime.Date == DateTime.Now.Date);
                var orderedData = dataQueryOut.OrderBy(d => d.StartTime).ToList();

                List<AGVSystemCommonNet6.Availability.RTAvailabilityDto> runData = orderedData.FindAll(d => d.Main_Status == AGVSystemCommonNet6.clsEnums.MAIN_STATUS.RUN);
                List<AGVSystemCommonNet6.Availability.RTAvailabilityDto> idleData = orderedData.FindAll(d => d.Main_Status == AGVSystemCommonNet6.clsEnums.MAIN_STATUS.IDLE);
                List<AGVSystemCommonNet6.Availability.RTAvailabilityDto> downData = orderedData.FindAll(d => d.Main_Status == AGVSystemCommonNet6.clsEnums.MAIN_STATUS.DOWN);
                List<AGVSystemCommonNet6.Availability.RTAvailabilityDto> chargeData = orderedData.FindAll(d => d.Main_Status == AGVSystemCommonNet6.clsEnums.MAIN_STATUS.Charging);
                List<AGVSystemCommonNet6.Availability.RTAvailabilityDto> unknownData = orderedData.FindAll(d => d.Main_Status == AGVSystemCommonNet6.clsEnums.MAIN_STATUS.Unknown);

                var run = runData.Select(d => new { from = d.StartTime.AddHours(8), to = d.EndTime.AddHours(8) });
                var idle = idleData.Select(d => new { from = d.StartTime.AddHours(8), to = d.EndTime.AddHours(8) });
                var down = downData.Select(d => new { from = d.StartTime.AddHours(8), to = d.EndTime.AddHours(8) });
                var charge = chargeData.Select(d => new { from = d.StartTime.AddHours(8), to = d.EndTime.AddHours(8) });
                var unknown = unknownData.Select(d => new { from = d.StartTime.AddHours(8), to = d.EndTime.AddHours(8) });

                output.Add(name, new { run, idle, down, charge, unknown });

            }

            return Ok(output);
        }

        [HttpGet("Query")]
        public async Task<IActionResult> Query(string AGVName, string StartDate, string EndDate)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            var startDate = DateTime.Parse(StartDate);
            var endDate = DateTime.Parse(EndDate);
            endDate = endDate.AddDays(1);
            //MTTRMTBFCount.MTTRMTBF_TimeCount(startDate, endDate, AGVName);
            //MTTRMTBFCount.MissTagCount(startDate, endDate, AGVName);

            Dictionary<DateTime, double> MTBF = _MeanTimeQuerier.GetMTBF(AGVName,startDate, endDate );
            Dictionary<DateTime, double> MTTR= _MeanTimeQuerier.GetMTTR(AGVName,startDate, endDate );

            stopwatch.Stop();
            Console.WriteLine($"MTBF/MTTR Data Prepare Time Spend: {stopwatch.Elapsed.TotalSeconds} s");
            stopwatch.Restart();
            using var db = new AGVSDatabase();
            var datas = db.tables.Availabilitys.Where(dat => dat.AGVName == AGVName && dat.Time >= startDate && dat.Time <= endDate);
            var idle_time = datas.Sum(d => d.IDLE_TIME);
            var run_time = datas.Sum(d => d.RUN_TIME);
            var down_time = datas.Sum(d => d.DOWN_TIME);
            var charge_time = datas.Sum(d => d.CHARGE_TIME);
            //['RUN', 'IDLE', 'DOWN', 'CHARGE', 'UNKNOWN']
            var dataset = new
            {
                total = new double[] { run_time, idle_time, down_time, charge_time },
                days = new
                {
                    dates = datas.Select(dat => dat.Time.ToString()).ToArray(),
                    idle = datas.Select(dat => dat.IDLE_TIME).ToArray(),
                    run = datas.Select(dat => dat.RUN_TIME).ToArray(),
                    down = datas.Select(dat => dat.DOWN_TIME).ToArray(),
                    charge = datas.Select(dat => dat.CHARGE_TIME).ToArray(),
                },
                BarchartMTTR = MTTR.ToDictionary(k=>k.Key.ToString("yyyy/MM/dd"),k=>k.Value),
                BarchartMTBF = MTBF.ToDictionary(k => k.Key.ToString("yyyy/MM/dd"), k => k.Value),
                BarchartMissTag = new
                {
                    dates = MTTRMTBFCount.MttrMtbf_date.ToArray(),
                    count = MTTRMTBFCount.MissTagcount.ToArray(),
                }
            };
            stopwatch.Stop();
            Console.WriteLine($"{stopwatch.Elapsed.TotalSeconds}");
            return Ok(dataset);
        }
    }
}
