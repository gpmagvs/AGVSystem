namespace AGVSystem.Models.TaskAllocation.HotRun
{
    public class HotRunScript
    {
        public int no { get; set; }
        public string agv_name { get; set; } = "";
        public int loop_num { get; set; }
        public int finish_num { get; set; }
        public int action_num { get; set; }

        public string state { get; set; } = "IDLE";
        public List<HotRunAction> actions { get; set; } = new List<HotRunAction>();

        public string comment { get; set; } = "Description";
        internal CancellationTokenSource cancellationTokenSource { get; set; }
    }
    public class HotRunAction
    {
        public int no { get; set; }
        public string action { get; set; } = "move";
        public int source_tag { get; set; }
        public int destine_tag { get; set; }
        public int destine_theta { get; set; } = -1;
    }
}
