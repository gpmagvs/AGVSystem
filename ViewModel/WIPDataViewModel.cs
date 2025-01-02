using EquipmentManagment.WIP;

namespace AGVSystem.ViewModel
{
    public class WIPDataViewModel
    {
        public string WIPName { get; set; } = "WEE~~";
        public string DeviceID { get; set; } = "";
        public int Columns { get; set; } = 1;
        public int Rows { get; set; } = 1;
        public bool IsOvenAsRacks { get; set; } = false;
        public Dictionary<int, int[]> ColumnsTagMap { get; set; } = new Dictionary<int, int[]>();
        public List<PortOfRackViewModel> Ports { get; set; } = new List<PortOfRackViewModel>();
        public bool IsAnyPortHasDataButNoCargo
        {
            get
            {
                if (this.IsOvenAsRacks)
                    return false;
                if (!Ports.Any()) return false;
                return Ports.Any(port => !port.CargoExist && !string.IsNullOrEmpty(port.CarrierID));
            }
        }
    }
}
