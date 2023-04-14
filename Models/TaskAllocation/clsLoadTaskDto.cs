using AGVSytemCommonNet6.MAP;
namespace AGVSystem.Models.TaskAllocation
{
    public class clsLoadTaskDto : clsMoveTaskDto
    {

        public clsLoadTaskDto()
        {
            Action_Name = "load";
        }
        public int To_Slot_Number { get; set; }
        public string CST_ID { get; set; }

    }
}
