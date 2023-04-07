namespace AGVSystem.Models.TaskAllocation
{
    public class clsCarryTaskDto : clsLoadTaskDto
    {
        public clsCarryTaskDto() { 
            Action_Name = "carry";
        }
        public int From_Tag { get; set; }
        public int From_Slot_Number { get; set; }
    }
}
