namespace AGVSystem.Models.EQDevices
{
    public class RackPortAbnoramlStatus
    {
        public string rackName { get; private set; } = "";
        public string portNo { get; private set; } = "";
        public string abnormalDescription { get; private set; } = "";
        public RackPortAbnoramlStatus(string rackName, string portNo, string abnormalDescription)
        {
            this.rackName = rackName;
            this.portNo = portNo;
            this.abnormalDescription = abnormalDescription;
        }
    }
}
