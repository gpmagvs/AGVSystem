using AGVSystemCommonNet6.Availability;
using AGVSystemCommonNet6.DATABASE;
using Microsoft.EntityFrameworkCore;
using static AGVSystemCommonNet6.clsEnums;

namespace AGVSystem.Service
{
    public class MeanTimeQueryService
    {
        private readonly AGVSDbContext _dbContent;

        public MeanTimeQueryService(AGVSDbContext dbContent)
        {
            _dbContent = dbContent;
        }

        /// <summary>
        /// Mean Time Between Failure = Total Operational Time / totoal number of failures
        /// </summary>
        public Dictionary<DateTime, double> GetMTBF(string agvName, DateTime fromTime, DateTime toTime)
        {
            var rawDatas = _GetDataInCondition(agvName, fromTime, toTime);
            int days = int.Parse(Math.Round((toTime - fromTime).TotalDays) + "");
            Dictionary<DateTime, double> _MTBFOfEeachDay = new Dictionary<DateTime, double>();
            for (int i = 0; i < days; i++)
            {
                var _fromTime = fromTime.AddDays(i);
                _fromTime = new DateTime(_fromTime.Year, _fromTime.Month, _fromTime.Day, 0, 0, 0);
                var _endTime = new DateTime(_fromTime.Year, _fromTime.Month, _fromTime.Day, 23, 59, 59);
                var dataInDay = rawDatas.Where(d => d.StartTime >= _fromTime && d.EndTime <= _endTime).ToList();
                var downStatesMoments = dataInDay.Where(d => d.Main_Status == MAIN_STATUS.DOWN);
                int numberOfFailures = downStatesMoments.Count();

                numberOfFailures = numberOfFailures == 0 ? 1 : numberOfFailures;
                double totalOperationalTime = dataInDay.Where(d => d.Main_Status != MAIN_STATUS.DOWN)
                                                        .Select(d => d.Duration).Sum();
                double _MTBF = totalOperationalTime / numberOfFailures;
                _MTBFOfEeachDay.Add(_fromTime, _MTBF);
            }
            return _MTBFOfEeachDay;
        }

        /// <summary>
        /// Mean Time To Repair (down->idle)
        /// </summary>
        /// <param name="agvName"></param>
        /// <param name="fromTime"></param>
        /// <param name="toTime"></param>
        public Dictionary<DateTime, double> GetMTTR(string agvName, DateTime fromTime, DateTime toTime)
        {
            var rawDatas = _GetDataInCondition(agvName, fromTime, toTime);
            int days = int.Parse(Math.Round((toTime - fromTime).TotalDays) + "");
            Dictionary<DateTime, double> _MTTROfEeachDay = new Dictionary<DateTime, double>();
            for (int i = 0; i < days; i++)
            {
                var _fromTime = fromTime.AddDays(i);
                _fromTime = new DateTime(_fromTime.Year, _fromTime.Month, _fromTime.Day, 0, 0, 0);
                var _endTime = new DateTime(_fromTime.Year, _fromTime.Month, _fromTime.Day, 23, 59, 59);

                var dataInDay = rawDatas.Where(d => d.StartTime >= _fromTime && d.EndTime <= _endTime).ToList();
                var downStatesMoments = dataInDay.Where(d => d.Main_Status == MAIN_STATUS.DOWN);
                double _MTTR = 0;
                if (downStatesMoments.Any())
                    _MTTR = downStatesMoments.Select(d => d.Duration).Average();
                _MTTROfEeachDay.Add(_fromTime, _MTTR);
            }
            return _MTTROfEeachDay;

        }

        private IEnumerable<RTAvailabilityDto> _GetDataInCondition(string agvName, DateTime fromTime, DateTime toTime)
        {
            return _dbContent.RealTimeAvailabilitys.AsNoTracking().Where(entity => entity.StartTime >= fromTime && entity.EndTime <= toTime)
                                                           .Where(entity => entity.AGVName == agvName);
        }
    }
}
