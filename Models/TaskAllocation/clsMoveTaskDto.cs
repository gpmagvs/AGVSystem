namespace AGVSystem.Models.TaskAllocation
{
    public class clsMoveTaskDto : clsTaskBaseDto
    {
        public clsMoveTaskDto()
        {
            Action_Name = "move";
        }
        public int To_Tag { get; set; }
    }
}
