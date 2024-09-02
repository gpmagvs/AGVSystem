namespace AGVSystem.ViewModel
{
    public class StationSelectOption
    {
        public string Type { get; set; } = "eq";//wip
        public int Value { get; set; } = -1;
        public string Label { get; set; } = "";
        public List<SlotOption> Slots { get; set; } = new List<SlotOption>();

        public List<DownStreamStationSelectOption> DownStreamStationOptions { get; set; } = new List<DownStreamStationSelectOption>();
    }

    public class DownStreamStationSelectOption
    {
        public string Type { get; set; } = "eq";//wip
        public int Value { get; set; } = -1;
        public string Label { get; set; } = "";
        public List<SlotOption> Slots { get; set; } = new List<SlotOption>();
    }

    public class SlotOption
    {
        public string Value { get; set; } = "";
        public string Label { get; set; } = "";
    }
}
