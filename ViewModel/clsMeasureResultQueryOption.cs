using AGVSystemCommonNet6.AGVDispatch.Model;

namespace AGVSystem.ViewModel
{
    public class clsMeasureResultQueryOption
    {
        public enum MeasureResult
        {
            SUCCESS,
            FAIL,
            ALL
        }
        public string AGVName { get; set; } = "ALL";
        public MeasureResult Result { get; set; } = MeasureResult.ALL;

        public string StartTimeStr { get; set; } = "";
        public string EndTimeStr { get; set; } = "";

        public DateTime StartTime
        {
            get
            {
                if(StartTimeStr=="")
                    return DateTime.Now;
                return DateTime.Parse(StartTimeStr);
            }
        }
        public DateTime EndTime
        {
            get
            {
                if (EndTimeStr == "")
                    return DateTime.Now;
                return DateTime.Parse(EndTimeStr); 
            }
        }

        /// <summary>
        /// 每頁顯示數量
        /// </summary>
        public int DataNumPerPage { get; set; } = 30;
        /// <summary>
        /// 頁數(Start from 1)
        /// </summary>
        public int Page { get; set; } = 1;

        public int SkipDataNum
        {
            get
            {
                return DataNumPerPage * (Page - 1);
            }
        }
    }
    public class clsMeasureResultQueryResult : clsMeasureResultQueryOption
    {
        public int TotalDataNum { get; set; } = 0;
        public List<clsMeasureResult> dataList { get; set; } = new List<clsMeasureResult>();
    }
}
