namespace AGVSystem.Models.TaskAllocation
{
    public class clsUnloadTaskDto : clsLoadTaskDto
    {
        public clsUnloadTaskDto()
        {
            Action_Name = "unload";
        }
        public string CST_ID { get; set; }
    }
}
