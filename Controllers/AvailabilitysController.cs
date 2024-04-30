
using AGVSystemCommonNet6.DATABASE;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AGVSystemCommonNet6.Availability;

namespace AGVSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AvailabilitysController : ControllerBase
    {
        private readonly AGVSDbContext _DbContext;

        public AvailabilitysController(AGVSDbContext DbContext)
        {
            this._DbContext = DbContext;
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
            var startDate = DateTime.Parse(StartDate);
            var endDate = DateTime.Parse(EndDate);
            MTTRMTBFCount.MTTR_TimeCount(startDate, endDate, AGVName);
            endDate = endDate.AddDays(1);
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
                BarchartMTTR = new
                {
                    dates = MTTRMTBFCount.Mttr_date.ToArray(),
                    time = MTTRMTBFCount.Mttr_data.ToArray(),
                },
                BarchartMTBF = new
                {
                    date = datas.Select(dat => dat.Time.ToString()).ToArray(),
                    //time
                }
            };
            return Ok(dataset);
        }
    }
}
