using AGVSystem.VMS;
using AGVSystemCommonNet6.DATABASE;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

                List<AGVSystemCommonNet6.Availability.RTAvailabilityDto> runData = orderedData.FindAll(d => d.Main_Status == AGVSytemCommonNet6.clsEnums.MAIN_STATUS.RUN);
                List<AGVSystemCommonNet6.Availability.RTAvailabilityDto> idleData = orderedData.FindAll(d => d.Main_Status == AGVSytemCommonNet6.clsEnums.MAIN_STATUS.IDLE);
                List<AGVSystemCommonNet6.Availability.RTAvailabilityDto> downData = orderedData.FindAll(d => d.Main_Status == AGVSytemCommonNet6.clsEnums.MAIN_STATUS.DOWN);
                List<AGVSystemCommonNet6.Availability.RTAvailabilityDto> chargeData = orderedData.FindAll(d => d.Main_Status == AGVSytemCommonNet6.clsEnums.MAIN_STATUS.Charging);
                List<AGVSystemCommonNet6.Availability.RTAvailabilityDto> unknownData = orderedData.FindAll(d => d.Main_Status == AGVSytemCommonNet6.clsEnums.MAIN_STATUS.Unknown);

                var run = runData.Select(d => new { from = d.StartTime.AddHours(8), to = d.EndTime.AddHours(8) });
                var idle = idleData.Select(d => new { from = d.StartTime.AddHours(8), to = d.EndTime.AddHours(8) });
                var down = downData.Select(d => new { from = d.StartTime.AddHours(8), to = d.EndTime.AddHours(8) });
                var charge = chargeData.Select(d => new { from = d.StartTime.AddHours(8), to = d.EndTime.AddHours(8) });
                var unknown = unknownData.Select(d => new { from = d.StartTime.AddHours(8), to = d.EndTime.AddHours(8) });

                output.Add(name, new { run, idle, down, charge, unknown });

            }

            return Ok(output);
        }
    }
}
