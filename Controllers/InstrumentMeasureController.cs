using AGVSystem.Models.BayMeasure;
using AGVSystem.Models.Map;
using AGVSystem.TaskManagers;
using AGVSystem.ViewModel;
using AGVSystemCommonNet6.AGVDispatch.Model;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.MAP;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AGVSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InstrumentMeasureController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Get([FromBody] clsMeasureResultQueryOption options)
        {
            clsMeasureResultQueryResult result = new clsMeasureResultQueryResult()
            {
                Page = options.Page,
                AGVName = options.AGVName,
                DataNumPerPage = options.DataNumPerPage,
            };

            using (var database = new AGVSDatabase())
            {
                var all_data_linqing = database.tables.InstrumentMeasureResult.AsNoTracking().Where(
                    data => data.StartTime >= options.StartTime && data.StartTime <= options.EndTime &&
                    (options.AGVName == "ALL" ? true : data.AGVName == options.AGVName) &&
                     (options.Result == clsMeasureResultQueryOption.MeasureResult.ALL ? true : options.Result == clsMeasureResultQueryOption.MeasureResult.SUCCESS ? data.result == "done" : data.result == "error")
                    );
                result.TotalDataNum = all_data_linqing.Count();
                result.dataList = all_data_linqing.Skip(options.SkipDataNum).Take(options.DataNumPerPage).ToList();
            }


            return Ok(result);
        }

        [HttpGet("GetBaysTableData")]
        public async Task<IActionResult> GetBaysTableData()
        {
            List<KeyValuePair<string, Bay>> bayskeypair = AGVSMapManager.CurrentMap.Bays.OrderBy(b => b.Key).ToList();
            int sequence = 1;
            List<clsBayMeasure> baystable = new List<clsBayMeasure>();
            foreach (var item in bayskeypair)
            {
                var bay = new clsBayMeasure()
                {
                    BayName = item.Key,
                    PointNames = item.Value.Points,
                    SelectedPointNames = item.Value.Points,
                    Sequence = sequence,
                };
                sequence += 1;
                baystable.Add(bay);
            }
            return Ok(baystable);
        }



        [HttpGet("GetMeasureSchedules")]
        public async Task<IActionResult> GetMeasureSchedules()
        {
            return Ok(ScheduleMeasureManager.ScheduleMeasureList);
        }


        [HttpPost("AddNewMeasureSchedule")]
        public async Task<IActionResult> AddNewMeasureSchedule([FromBody] clsMeasureScript schedule)
        {
            bool add_success =ScheduleMeasureManager.AddNewSchedule(schedule);
            return Ok(new
            {
                 result= add_success,
                 message=""
            });
        }
    }
}
