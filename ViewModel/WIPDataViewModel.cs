using EquipmentManagment.WIP;

namespace AGVSystem.ViewModel
{
    public class WIPDataViewModel
    {
        public string WIPName { get; set; } = "WEE~~";
        public int Columns { get; set; } = 1;
        public int Rows { get; set; } = 1;
        public List<clsWIPPort> Ports { get; set; } = new List<clsWIPPort>();
    }
}
