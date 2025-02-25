namespace AGVSystem.Models.Automation
{
    public class AutoTransferSettings
    {
        public AutoTransferSetting AutoTransfer { get; set; } = new AutoTransferSetting();
        public class AutoTransferSetting
        {
            public int test { get; set; } = 1;
        }
    }
}
