
using AGVSystem.Models.Map;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.MAP;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json.Linq;

namespace AGVSystem.Service
{
    public class TrafficStateDataQueryService
    {
        public class clsStayData
        {
            public int Tag { get; set; }
            public double[] Coordination { get; set; } = new double[2];
            public double Duration { get; set; } = 0;
            public double DurationNormalized { get; set; } = 0;
        }

        AGVSDbContext dataBaseContext;
        public TrafficStateDataQueryService(AGVSDbContext dataBaseContext)
        {
            this.dataBaseContext = dataBaseContext;
        }

        internal async Task<Dictionary<int, clsStayData>> GetPointStayStateData(DateTime startTime, DateTime endTime)
        {
            try
            {

                var inTimeRangeDatas = dataBaseContext.PointPassTime.Where(p => p.Time >= startTime && p.Time <= endTime).ToList();
                var tags = inTimeRangeDatas.Select(item => item.Tag).Distinct().ToList();

                Dictionary<int, double> tagsStayDurations = tags.ToDictionary(tag => tag, tag => inTimeRangeDatas.Where(item => item.Tag == tag)
                                                                                                                 .Where(item => item.StageWhenReach == 3 && item.StageWhenLeaving == 3)
                                                                                                                 .Where(item => item.Duration > 1)
                                                                                                                 .Sum(item => item.Duration));
                tagsStayDurations = tagsStayDurations.Where(tag => tag.Value != 0).ToDictionary(t => t.Key, t => t.Value);
                double min = tagsStayDurations.Values.Min();
                double max = tagsStayDurations.Values.Max();
                double avg = tagsStayDurations.Values.Average();
                //正交化數據 0~1
                Dictionary<int, clsStayData> tagsStayDurationsNormalized = tagsStayDurations.ToDictionary(tag => tag.Key, tag => new clsStayData
                {
                    Tag = tag.Key,
                    Duration = tag.Value,
                    DurationNormalized = logNormalize(tag.Value, min, max),
                    Coordination = GetCoordinaitonOfTag(tag.Key)
                });


                double logNormalize(double value, double min, double max)
                {
                    // 防止負數和零值
                    if (value <= 0 || min <= 0 || max <= 0)
                        return 0;
                    // 確保 min < max
                    if (min >= max)
                        return 0;
                    try
                    {
                        double logValue = Math.Log(value);
                        double logMin = Math.Log(min);
                        double logMax = Math.Log(max);
                        // 執行正規化計算
                        return (logValue - logMin) / (logMax - logMin);
                    }
                    catch
                    {
                        return 0;
                    }
                }

                return tagsStayDurationsNormalized;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        private double[] GetCoordinaitonOfTag(int tag)
        {
            MapPoint pt = AGVSMapManager.GetMapPointByTag(tag);
            if (pt == null)
                return new double[2] { 0, 0 };
            return new double[2] { pt.X, pt.Y };
        }
    }
}
