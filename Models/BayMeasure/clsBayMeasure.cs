namespace AGVSystem.Models.BayMeasure
{
    public class clsBayMeasure
    {

        public string BayName { get; set; } = "";
        public List<string> PointNames { get; set; } = new List<string>();
        public List<string> SelectedPointNames { get; set; } = new List<string>();

        /// <summary>
        /// 量測順序
        /// </summary>
        public int Sequence { get; set; } = 1;

    }
}
