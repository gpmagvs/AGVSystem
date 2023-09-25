using AGVSystemCommonNet6.AGVDispatch.Model;

namespace AGVSystem.Models.BayMeasure
{
    public class clsMeasureScript
    {
        public string ScriptName { get; set; } = "";

        public string Time { get; set; } = "00:00";
        public string AGVName { get; set; } = "AGV_001";

        public List<clsBayMeasure> Bays { get; set; } = new List<clsBayMeasure>();
    }
}
