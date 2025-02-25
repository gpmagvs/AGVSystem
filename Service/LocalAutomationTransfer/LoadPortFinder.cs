using EquipmentManagment.MainEquipment;
using EquipmentManagment.WIP;
using System.Collections.Generic;

namespace AGVSystem.Service.LocalAutomationTransfer
{
    public class LoadPortFinder : UnloadPortFinder
    {
        private clsPortOfRack sourceRackPort;
        public LoadPortFinder(clsEQ sourcePortEQ, List<clsEQ> allEQList, List<clsPortOfRack> allRackPortList) : base(sourcePortEQ, allEQList, allRackPortList)
        {
        }

        public LoadPortFinder(clsEQ sourcePortEQ, clsPortOfRack sourceRackPort, List<clsEQ> allEQList, List<clsPortOfRack> allRackPortList) : base(sourcePortEQ, allEQList, allRackPortList)
        {
            this.sourceRackPort = sourceRackPort;
        }

        public override List<PortInfoDto> FindPorts()
        {
            List<PortInfoDto> output = new List<PortInfoDto>();
            if (sourcePortEQ != null)
            {

                PortInfoDto sourceInfo = new PortInfoDto()
                {
                    portEntity = sourcePortEQ,
                    portType = PORT_TYPE.EQ,
                    tagNumber = sourcePortEQ.EndPointOptions.TagID,
                    slot = sourcePortEQ.EndPointOptions.Height
                };

                if (!sourcePortEQ.IsCreateUnloadTaskAble() || sourceInfo.IsOrderRunning())
                    return new List<PortInfoDto>();

                List<string> downstreamEQNames = sourcePortEQ.EndPointOptions.ValidDownStreamEndPointNames;
                output.AddRange(otherEQList.Where(eq => downstreamEQNames.Contains(eq.EQName))
                              .Where(eq => eq.IsCreateLoadTaskAble())
                              .Select(eq => new PortInfoDto() { tagNumber = eq.EndPointOptions.TagID, slot = eq.EndPointOptions.Height })
                              .Where(info => !info.IsOrderRunning())
                              .ToList());
            }

            if (sourceRackPort != null)
            {

                output.AddRange(otherEQList.Where(eq => eq.EndPointOptions.SpeficRackContentInput == sourceRackPort.StoredRackContentType)
                                .Where(eq => eq.IsCreateLoadTaskAble())
                              .Select(eq => new PortInfoDto() { tagNumber = eq.EndPointOptions.TagID, slot = eq.EndPointOptions.Height })
                              .Where(info => !info.IsOrderRunning())
                              .ToList());
            }
            return output;
        }
    }
}
