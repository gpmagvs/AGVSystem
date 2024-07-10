using AGVSystem.Models.BayMeasure;
using AGVSystem.Models.Map;
using AGVSystem.TaskManagers;
using AGVSystem.ViewModel;
using AGVSystemCommonNet6.AGVDispatch.Model;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.MAP;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using RosSharp.RosBridgeClient.MessageTypes.Sensor;
using RosSharp.RosBridgeClient.MessageTypes.Std;
using static SQLite.SQLite3;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System;

namespace AGVSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InstrumentMeasureController : ControllerBase
    {
        private AGVSDbContext _dbcontent;

        public InstrumentMeasureController(AGVSDbContext dbcontent)
        {
            _dbcontent = dbcontent;
        }
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

        [HttpGet("GetBayNamesMeasured")]
        public async Task<IActionResult> GetBayNamesMeasured()
        {
            List<string> baynames = _dbcontent.InstrumentMeasureResult.AsNoTracking().Where(e => true)
                                                                          .Select(e => e.BayName)
                                                                          .Distinct()
                                                                          .ToList();
            return Ok(baynames);
        }

        [HttpGet("GetTagsOfBay")]
        public async Task<IActionResult> GetTagsOfBay(string bayName)
        {

            List<int> tags = _dbcontent.InstrumentMeasureResult.AsNoTracking().Where(e => e.BayName == bayName)
                                                                                     .Select(e => e.TagID)
                                                                                     .Distinct().
                                                                                     ToList();
            return Ok(tags);
        }

        [HttpGet("GetMeasureItems")]
        public async Task<IActionResult> GetMeasureItems()
        {
            Dictionary<string, string> itemMap = new Dictionary<string, string>
            {
                    {"illuminance","照度"},
                    {"decibel","分貝"},
                    {"temperature","溫度"},
                    {"humudity","濕度"},
                    {"IPA","IPA"},
                    {"TVOC","TVOC"},
                    {"Acetone","Acetone"},
                    {"partical_03um","Partical-0.3um"},
                    {"partical_05um","Partical-0.5um"},
                    {"partical_10um","Partical-1um"},
                    {"partical_30um","Partical-3um"},
                    {"partical_50um","Partical-5um"},
                    {"partical_100um","Partical-10um"},
                    { "PID","" }
            };
            return Ok(itemMap);
        }
        public class ItemTrendChartQueryForm
        {
            public string bayName { get; set; }
            public int tag { get; set; }
            public string item { get; set; }
            public string from { get; set; }
            public string to { get; set; }
        }
        [HttpPost("QueryItemTrendData")]
        public async Task<IActionResult> QueryItemTrendData([FromBody] ItemTrendChartQueryForm form)
        {
            DateTime fromTime = DateTime.Parse(form.from);
            DateTime toTime = DateTime.Parse(form.to);
            var resultOverview = _dbcontent.InstrumentMeasureResult.AsNoTracking()
                                                .Where(d => d.StartTime >= fromTime && d.StartTime <= toTime)
                                                .Where(d => d.BayName == form.bayName && d.TagID == form.tag)
                                                .ToList();
            var valList = resultOverview.OrderBy(d => d.StartTime)
                          .Select(d => new { Time = d.StartTime, Value = getValByQueryItem(d, form.item) })
                          .ToList();

            object getValByQueryItem(clsMeasureResult d, string item)
            {
                switch (item)
                {
                    case "illuminance":
                        return d.illuminance;
                    case "decibel":
                        return d.decibel;
                    case "temperature":
                        return d.temperature;
                    case "humudity":
                        return d.humudity;
                    case "IPA":
                        return d.IPA;
                    case "TVOC":
                        return d.TVOC;
                    case "Acetone":
                        return d.Acetone;
                    case "partical_03um":
                        return d.partical_03um;
                    case "partical_05um":
                        return d.partical_05um;
                    case "partical_10um":
                        return d.partical_10um;
                    case "partical_30um":
                        return d.partical_30um;
                    case "partical_50um":
                        return d.partical_50um;
                    case "partical_100um":
                        return d.partical_100um;
                    case "PID":
                        return d.BayName;
                    default:
                        return 0;
                }
            }
            return Ok(valList);
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
            bool add_success = ScheduleMeasureManager.AddNewSchedule(schedule);
            return Ok(new
            {
                result = add_success,
                message = ""
            });
        }
        [HttpPost("ModifyMeasureSchedule")]
        public async Task<IActionResult> ModifyMeasureSchedule(string time, string agv_name, [FromBody] clsMeasureScript new_schedule)
        {
            bool add_success = ScheduleMeasureManager.ModifySchedule(time, agv_name, new_schedule);
            return Ok(new
            {
                result = add_success,
                message = ""
            });
        }
        [HttpDelete("DeleteSchedule")]
        public async Task<IActionResult> DeleteSchedule(string time, string agv_name)
        {
            bool del_success = ScheduleMeasureManager.DeleteSchedule(time, agv_name);
            return Ok(new
            {
                result = del_success,
                message = ""
            });
        }
    }
}
