using Newtonsoft.Json;

namespace AGVSystem.VMS.GPMxYUNTech_FORK.Models
{
    public class clsTaskStatus
    {
        public List<clsTaskStatusItem> RunningTasks { get; set; } = new List<clsTaskStatusItem>();
        public List<clsTaskStatusItem> CompletedTasks { get; set; } = new List<clsTaskStatusItem>();
    }

    public class clsTaskStatusItem
    {
        public string TaskSerialNumber { get; set; }
        public string TaskName { get; set; }
        public string TaskStatus { get; set; }
        public string TaskSource { get; set; }

        /// <summary>
        /// 固定Length=3 [0]:導航任務 [1] 對位任務 [2] Fork   
        /// </summary>
        public List<object> TaskList
        {
            get => _TaskList;
            set
            {
                if (_TaskList != value)
                {
                    _TaskList = value;
                    try
                    {
                        NavTaskStatus = JsonConvert.DeserializeObject<clsNavTaskStatus>(_TaskList[0].ToString());
                    }
                    catch (Exception)
                    {
                        NavTaskStatus = null;
                    }
                    try
                    {
                        AlignTaskStatus = JsonConvert.DeserializeObject<clsAlignTaskStatus>(_TaskList[1].ToString());

                    }
                    catch (Exception)
                    {
                        AlignTaskStatus = null;
                    }
                    try
                    {
                        ForkActionTaskStatus = JsonConvert.DeserializeObject<clsForkActionTaskStatus>(_TaskList[2].ToString());
                    }
                    catch (Exception)
                    {
                        ForkActionTaskStatus = null;
                    }
                }
            }
        }
        private List<object> _TaskList { get; set; }

        internal clsNavTaskStatus NavTaskStatus { get; private set; }
        internal clsAlignTaskStatus AlignTaskStatus { get; private set; }
        internal clsForkActionTaskStatus ForkActionTaskStatus { get; private set; }
    }


    /// <summary>
    /// 導航任務狀態
    /// </summary>
    public class clsNavTaskStatus
    {
        public string TaskSerialNumber { get; set; }
        public string StationName { get; set; }
        public string NavigationPoint { get; set; }
        public int ReturnCode { get; set; }
    }

    /// <summary>
    /// 對位任務狀態
    /// </summary>
    public class clsAlignTaskStatus
    {
        public string TaskSerialNumber { get; set; }
        public string StationName { get; set; }
        public string NavigationPoint { get; set; }
        public int ReturnCode { get; set; }
    }


    /// <summary>
    /// 枒杈動作任務狀態
    /// </summary>
    public class clsForkActionTaskStatus
    {
        public string TaskSerialNumber { get; set; }
        public string StationName { get; set; }
        public string NavigationPoint { get; set; }
        public int ReturnCode { get; set; }
    }
}
