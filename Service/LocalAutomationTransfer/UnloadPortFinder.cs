using AGVSystemCommonNet6.AGVDispatch;
using AGVSystemCommonNet6.DATABASE;
using EquipmentManagment.MainEquipment;
using EquipmentManagment.Manager;
using EquipmentManagment.WIP;

namespace AGVSystem.Service.LocalAutomationTransfer
{
    /// <summary>
    /// 找需要移出貨物的設備或RACK PORT
    /// </summary>
    public class UnloadPortFinder
    {
        public readonly clsEQ sourcePortEQ;
        protected readonly List<clsEQ> otherEQList;
        protected readonly List<clsPortOfRack> allRackPortList;

        public UnloadPortFinder(clsEQ sourcePortEQ, List<clsEQ> allEQList, List<clsPortOfRack> allRackPortList)
        {
            this.sourcePortEQ = sourcePortEQ;
            otherEQList = sourcePortEQ == null ? allEQList : allEQList.SkipWhile(eq => eq.EQName == this.sourcePortEQ.EQName).ToList();
            this.allRackPortList = allRackPortList;
        }

        public virtual List<PortInfoDto> FindPorts()
        {

            var unloadEqList = otherEQList.Where(eq => eq.EndPointOptions.ValidDownStreamEndPointNames.Contains(sourcePortEQ.EQName))
                                          .ToHashSet();
            List<PortInfoDto> unloadPorts = new List<PortInfoDto>();
            if (unloadEqList.Any())
            {
                List<PortInfoDto> unloadEq = unloadEqList.Where(eq => eq.IsCreateUnloadTaskAble())
                                                            .Select(eq => new PortInfoDto { tagNumber = eq.EndPointOptions.TagID, slot = eq.EndPointOptions.Height, portType = PORT_TYPE.EQ, portEntity = eq })
                                                            .Where(info => !info.IsOrderRunning())
                                                            .ToList();
                if (unloadEq.Any())
                {
                    unloadPorts.AddRange(unloadEq);
                }
            }
            return unloadPorts;
        }

        public class PortInfoDto
        {

            public PORT_TYPE portType { get; set; } = PORT_TYPE.EQ;
            public int tagNumber { get; set; } = -1;
            public int slot { get; set; } = -1;
            public object portEntity { get; set; } = null;
            internal DateTime startWaitUnloadTime
            {
                get
                {
                    if (portType == PORT_TYPE.EQ)
                    {
                        return (portEntity as clsEQ).UnloadRequestRaiseTime;
                    }
                    else
                        return (portEntity as clsPortOfRack).InstallTime;
                }
            }
            public bool IsOrderRunning()
            {
                bool isWatingForExecute = DatabaseCaches.TaskCaches.WaitExecuteTasks.Any((order => (order.From_Station_Tag == tagNumber && order.GetFromSlotInt() == slot)
                                                                      || (order.To_Station_Tag == tagNumber && order.GetToSlotInt() == slot)));
                bool isExecutingAndWaitLoadUnload = DatabaseCaches.TaskCaches.RunningTasks.Any((order => (order.From_Station_Tag == tagNumber && order.GetFromSlotInt() == slot && (order.currentProgress != VehicleMovementStage.Traveling_To_Destine && order.currentProgress != VehicleMovementStage.WorkingAtDestination))
                                                                                            || (order.To_Station_Tag == tagNumber && order.GetToSlotInt() == slot)));

                if (isExecutingAndWaitLoadUnload)
                {

                }

                return isWatingForExecute || isExecutingAndWaitLoadUnload;
            }

            public override int GetHashCode()
            {
                return tagNumber + (slot * 1000);
            }
        }

        public enum PORT_TYPE
        {
            EQ, WIPPROT
        }
    }
}
