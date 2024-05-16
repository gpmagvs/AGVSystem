

using AGVSystemCommonNet6.Notify;
using Microsoft.CodeAnalysis.Scripting;

namespace AGVSystem.Models.TaskAllocation.HotRun
{
    public class HotRunScript
    {
        public int no { get; set; }
        /// <summary>
        /// ¸}¥»ID(°ß¤@)
        /// </summary>
        public string scriptID { get; set; } = "";
        public string agv_name { get; set; } = "";
        public int loop_num { get; set; }
        public int finish_num { get; set; }
        public int action_num { get; set; }

        public string state { get; set; } = "IDLE";

        public string RunningTaskInfo => $"{RunningAction.action.ToUpper()}-{RunningAction}";
        public List<HotRunAction> actions { get; set; } = new List<HotRunAction>();
        [NonSerialized]
        internal HotRunAction RunningAction = new HotRunAction();
        public string comment { get; set; } = "Description";
        internal CancellationTokenSource cancellationTokenSource;
        internal bool StopFlag { get; set; } = false;

        private string _RealTimeMessage = "";
        public string RealTimeMessage => _RealTimeMessage;

        public bool IsRandomCarryRun { get; set; } = false;

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
