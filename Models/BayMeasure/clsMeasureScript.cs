namespace AGVSystem.Models.BayMeasure
{
    public class clsMeasureScript
    {
        public string ScriptName { get; set; } = "";
        public List<clsBayMeasure> Bays { get; set; } = new List<clsBayMeasure>();
    }
}
