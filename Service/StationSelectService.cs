using AGVSystem.Models.Map;
using AGVSystem.ViewModel;
using AGVSystemCommonNet6.MAP;
using AGVSystemCommonNet6.User;
using EquipmentManagment.MainEquipment;
using EquipmentManagment.Manager;
using EquipmentManagment.WIP;
using System.Collections.Generic;
using System.Linq;

namespace AGVSystem.Service
{
    public class StationSelectService
    {
        public List<StationSelectOption> GetSourceStationOptions(ERole userRole = ERole.GOD)
        {
            List<StationSelectOption> options = new List<StationSelectOption>();

            var eqs = StaEQPManagager.MainEQList;

            if (userRole == ERole.Operator)
            {
                eqs = eqs.Where(eq => eq.Unload_Request).ToList();
            }
            List<StationSelectOption> eqOptions = _GetEqOptions(eqs);
            List<StationSelectOption> wipOptions = _GetWIPOptions();


            options.AddRange(wipOptions);


            options.AddRange(eqOptions);

            return options;
        }
        public List<StationSelectOption> GetDestineStationOptions(ERole userRole = ERole.GOD)
        {
            List<StationSelectOption> options = new List<StationSelectOption>();

            var eqs = StaEQPManagager.MainEQList;

            if (userRole == ERole.Operator)
            {
                eqs = eqs.Where(eq => eq.Load_Request).ToList();
            }
            List<StationSelectOption> eqOptions = _GetEqOptions(eqs);
            List<StationSelectOption> wipOptions = _GetWIPOptions();
            options.AddRange(wipOptions);
            options.AddRange(eqOptions);

            return options;
        }


        private List<StationSelectOption> _GetEqOptions(List<clsEQ> eqs)
        {
            List<int> RackTags = StaEQPManagager.RacksList.SelectMany(rack => rack.RackOption.ColumnTagMap.Values.SelectMany(tag => tag)).ToList();
            List<int> EQTag = eqs.Where(eq => !RackTags.Contains(eq.EndPointOptions.TagID)).Select(eq => eq.EndPointOptions.TagID).ToList();
            List<StationSelectOption> eqOptions = EQTag.Select(tag => new StationSelectOption
            {
                Type = typeof(clsEQ).Name,
                Value = tag,
                Label = _GetMapPoint(tag).display,
                Slots = _GetSlotOption(tag)
            }).ToList();
            return eqOptions;
        }

        private List<StationSelectOption> _GetWIPOptions()
        {
            List<int> RackTags = StaEQPManagager.RacksList.SelectMany(rack => rack.RackOption.ColumnTagMap.Values.SelectMany(tag => tag)).ToList();
            List<StationSelectOption> wipOptions = RackTags.Select(tag => new StationSelectOption
            {
                Type = typeof(clsRack).Name,
                Value = tag,
                Label = _GetMapPoint(tag).display,
                Slots = _GetWIPSlotOption(tag)
            }).ToList();
            return wipOptions;
        }

        private List<SlotOption> _GetSlotOption(int tag)
        {
            return StaEQPManagager.MainEQList.Where(eq => eq.EndPointOptions.TagID == tag)
                                             .Select(eq => new SlotOption
                                             {
                                                 Value = eq.EndPointOptions.Height + "",
                                                 Label = eq.EndPointOptions.Name + $"(第{eq.EndPointOptions.Height + 1})層"
                                             })
                                             .ToList();
        }

        private List<SlotOption> _GetWIPSlotOption(int tag)
        {
            EquipmentManagment.WIP.clsRack? rack = StaEQPManagager.RacksList.FirstOrDefault(rack => rack.RackOption.ColumnTagMap.Values.SelectMany(tag => tag).Contains(tag));
            List<SlotOption> slotOptions = new List<SlotOption>();
            if (rack != null)
            {
                var eqInWIP = StaEQPManagager.MainEQList.FirstOrDefault(eq => eq.EndPointOptions.TagID == tag);
                for (int i = 0; i < rack.RackOption.Rows; i++)
                {
                    slotOptions.Add(new SlotOption
                    {
                        Value = i + "",
                        Label = $"第{i + 1}層"
                    });
                }
                if (eqInWIP != null)
                {
                    slotOptions[0].Label = eqInWIP.EndPointOptions.Name;
                }
            }
            return slotOptions;
        }
        private (int tag, string display) _GetMapPoint(int tag)
        {
            List<MapPoint> mapPoints = AGVSMapManager.CurrentMap.Points.Values.ToList();
            MapPoint point = mapPoints.FirstOrDefault(pt => pt.TagNumber == tag);
            if (point != null)
            {
                return (tag, point.Graph.Display);
            }
            else
            {
                return (tag, null);
            }
        }
    }
}
