namespace AGVSystem.Models.TaskAllocation
{
    public class clsTaskBaseDto
    {
        public string AGV_Name { get; set; }
        public string Action_Name { get; set; } = "move";
        /// <summary>
        /// 優先度
        /// </summary>
        public int Priority { get; set; } = 50;
    }
}
