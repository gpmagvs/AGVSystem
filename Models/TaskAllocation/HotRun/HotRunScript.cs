

using AGVSystemCommonNet6.Notify;
using Microsoft.CodeAnalysis.Scripting;

namespace AGVSystem.Models.TaskAllocation.HotRun
{
    public class HotRunScript
    {

        public static event EventHandler OnHotRunScriptChanged;

        public int no { get; set; }
        /// <summary>
        /// 腳本ID(唯一)
        /// </summary>
        public string scriptID { get; set; } = "";
        public string agv_name { get; set; } = "";
        public int loop_num { get; set; }

        private int _finish_num = 0;
        public int finish_num
        {
            get => _finish_num;
            set
            {
                if (_finish_num != value)
                {
                    _finish_num = value;
                    OnHotRunScriptChanged?.Invoke(this, null);
                }
            }
        }
        public int action_num { get; set; }

        private string _state = "IDLE";
        public string state
        {
            get => _state;
            set
            {
                if (_state != value)
                {
                    _state = value;
                    OnHotRunScriptChanged?.Invoke(this, null);
                }
            }
        }

        public string RunningTaskInfo => $"{RunningAction.action.ToUpper()}-{RunningAction}";
        public List<HotRunAction> actions { get; set; } = new List<HotRunAction>();
        [NonSerialized]
        internal HotRunAction RunningAction = new HotRunAction();
        public RegularUnloadConfiguration RegularLoadSettings { get; set; } = new RegularUnloadConfiguration();

        public string comment { get; set; } = "Description";
        internal CancellationTokenSource cancellationTokenSource;
        internal event EventHandler OnScriptStopRequest;
        internal bool _StopFlag = false;
        internal bool StopFlag
        {
            get => _StopFlag;
            set
            {
                if (_StopFlag != value)
                {
                    _StopFlag = value;
                    if (value)
                    {
                        OnScriptStopRequest?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
        }

        private string _RealTimeMessage = "";
        public string RealTimeMessage => _RealTimeMessage;

        public bool IsRandomCarryRun { get; set; } = false;

        public bool IsRegularUnloadRequst { get; set; } = false;

        internal void SyncSetting(HotRunScript script)
        {
            this.no = script.no;
            this.scriptID = script.scriptID;
            this.actions = script.actions;
            this.comment = script.comment;
            this.action_num = script.action_num;
            this.loop_num = script.loop_num;
        }
        internal void UpdateRealTimeMessage(string msg, bool withTitle = true, bool notification = true)
        {
            if (withTitle)
                _RealTimeMessage = $"[Hot Run-{agv_name}]{RunningAction.action.ToUpper()}-{msg}";
            else
                _RealTimeMessage = msg;

            if (notification)
                NotifyServiceHelper.INFO(_RealTimeMessage);
        }
    }

    public class RegularUnloadConfiguration
    {
        /// <summary>
        /// 總是保持LoadRequest的設備
        /// </summary>
        public List<string> LoadRequestAlwaysOnEqNames { get; set; } = new List<string>();

        /// <summary>
        ///  出料請求設定
        /// </summary>
        public List<UnloadRequestEQSettings> UnloadRequestsSettings { get; set; } = new List<UnloadRequestEQSettings>();
        public class UnloadRequestEQSettings
        {
            public string EqName { get; set; } = "";

            /// <summary>
            /// 當出料完成後延遲多久再發送UnloadRequest
            /// </summary>
            public double UnloadRequestInterval { get; set; } = 10;

            /// <summary>
            ///  當腳本開始執行時延遲多久再發送UnloadRequest
            /// </summary>
            public double DelayTimeWhenScriptStart { get; set; } = 3;
        }

    }

    public class HotRunAction
    {
        public int no { get; set; }
        public string action { get; set; } = "move";
        public int source_tag { get; set; }
        public int source_slot { get; set; } = 0;
        public int destine_tag { get; set; }
        public int destine_slot { get; set; } = 0;
        public string destine_name { get; set; } = "";
        public int destine_theta { get; set; } = -1;
        public string cst_id { get; set; } = "";
    }
}
