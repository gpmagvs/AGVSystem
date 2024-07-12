namespace AGVSystem.Models.TaskAllocation
{
    public class clsMaterialInfoDto
    {
        public string MaterialID { get; set; } = string.Empty;

        public string ActualID {  get; set; } = string.Empty;

        public string SourceStation {  get; set; } = string.Empty;

        public string TargetStation {  get; set; } = string.Empty;

        public string TaskSourceStation {  get; set; } = string.Empty;

        public string TaskTargetStatioin {  get; set; } = string.Empty;

        public string InstallStatus { get; set; } = "Unknown";

        public string IDStatus { get; set; } = "NG";

        public string MaterialType { get; set; } = "None";

        public string MaterialCondition { get; set; } = "Add";
    }
}
