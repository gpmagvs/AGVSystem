namespace AGVSystem.Models.Map
{
    /// <summary>
    /// 一個地圖分區
    /// </summary>
    public class clsMapRegion
    {
        public int RegionId { get; set; } = 0;
        public string RegionName { get; set; } = string.Empty;
        public string RegionDescription { get; set; } = string.Empty;

        public List<string> AGVPriorty { get; set; } 

    }
}
