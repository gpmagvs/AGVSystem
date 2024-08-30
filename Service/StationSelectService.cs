using AGVSystem.Models.Map;
using AGVSystem.ViewModel;
using AGVSystemCommonNet6.MAP;
using AGVSystemCommonNet6.User;
using EquipmentManagment.MainEquipment;
using EquipmentManagment.Manager;
using EquipmentManagment.WIP;
using Newtonsoft.Json;
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
            List<StationSelectOption> wipOptions = _GetWIPOptions(ref eqOptions);

            //下游設備
            CreateDownStreamOptionsOfEQ(ref eqOptions, ref wipOptions);
            CreateDownStreamOptionsOfWIP(ref eqOptions, ref wipOptions);

            options.AddRange(wipOptions);
            options.AddRange(eqOptions);

            return options;
        }


        /// <summary>
        /// If WIP as source, destine could be any where(if no consider LD/ULD state of Equipment)
        /// </summary>
        /// <param name="eqOptions"></param>
        /// <param name="wipOptions"></param>
        private void CreateDownStreamOptionsOfWIP(ref List<StationSelectOption> eqOptions, ref List<StationSelectOption> wipOptions)
        {
            foreach (StationSelectOption wipOpt in wipOptions)
            {
                wipOpt.DownStreamStationOptions = new List<DownStreamStationSelectOption>();

                //所有的EQ
                List<DownStreamStationSelectOption> downstreamEQsOptions = JsonConvert.DeserializeObject<List<DownStreamStationSelectOption>>(JsonConvert.SerializeObject(eqOptions));
                //所有的WIP
                List<DownStreamStationSelectOption> downstreamWipsOptions = JsonConvert.DeserializeObject<List<DownStreamStationSelectOption>>(JsonConvert.SerializeObject(wipOptions));
                wipOpt.DownStreamStationOptions.AddRange(downstreamWipsOptions);
                wipOpt.DownStreamStationOptions.AddRange(downstreamEQsOptions);
            }

        }

        /// <summary>
        /// If EQ as source, destine should be all wip and eqs where be assign as downstream of source eq.
        /// </summary>
        /// <param name="eqOptions"></param>
        /// <param name="wipOptions"></param>
        private void CreateDownStreamOptionsOfEQ(ref List<StationSelectOption> eqOptions, ref List<StationSelectOption> wipOptions)
        {
            foreach (StationSelectOption eqOpt in eqOptions)
            {
                eqOpt.DownStreamStationOptions = new List<DownStreamStationSelectOption>();
                int tag = eqOpt.Value;
                var eq = StaEQPManagager.GetEQByTag(tag);
                var downStramsNamesOfEQ = eq.EndPointOptions.ValidDownStreamEndPointNames;

                var downstreamEQs = StaEQPManagager.MainEQList.Where(_eq => downStramsNamesOfEQ.Contains(_eq.EndPointOptions.Name)).ToList();
                List<DownStreamStationSelectOption> downstreamEQsOptions = JsonConvert.DeserializeObject<List<DownStreamStationSelectOption>>(JsonConvert.SerializeObject(_GetEqOptions(downstreamEQs)));

                //所有的WIP
                List<DownStreamStationSelectOption> downstreamWipsOptions = JsonConvert.DeserializeObject<List<DownStreamStationSelectOption>>(JsonConvert.SerializeObject(wipOptions));


                eqOpt.DownStreamStationOptions.AddRange(downstreamWipsOptions);
                eqOpt.DownStreamStationOptions.AddRange(downstreamEQsOptions);
            }

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
            List<StationSelectOption> wipOptions = _GetWIPOptions(ref eqOptions);
            options.AddRange(wipOptions);
            options.AddRange(eqOptions);

            return options;
        }


        private List<StationSelectOption> _GetEqOptions(List<clsEQ> eqs)
        {
            List<int> EQTag = eqs.Select(eq => eq.EndPointOptions.TagID).ToList();
            List<StationSelectOption> eqOptions = EQTag.Select(tag => CreateOption(tag)).ToList();
            return eqOptions;

            StationSelectOption CreateOption(int tag)
            {
                List<SlotOption> slotOptions = _GetSlotOption(tag, out bool isEqLocatinRack);
                return new StationSelectOption
                {
                    Type = isEqLocatinRack ? "Mix" : typeof(clsEQ).Name,
                    Value = tag,
                    Label = _GetMapPoint(tag).display,
                    Slots = slotOptions
                };
            }
        }

        private List<StationSelectOption> _GetWIPOptions(ref List<StationSelectOption> eqOptions)
        {
            List<int> existEqTags = eqOptions.Select(opt => opt.Value).ToList();
            List<int> RackTags = StaEQPManagager.RacksList.SelectMany(rack => rack.RackOption.ColumnTagMap.Values.SelectMany(tag => tag)).ToList();

            List<StationSelectOption> wipOptions = RackTags.Where(tag => !existEqTags.Contains(tag)).Select(tag => new StationSelectOption
            {
                Type = typeof(clsRack).Name,
                Value = tag,
                Label = _GetMapPoint(tag).display,
                Slots = _GetWIPSlotOption(tag)
            }).ToList();
            return wipOptions;
        }

        private List<SlotOption> _GetSlotOption(int tag, out bool isEqLocatinRack)
        {
            List<int> RackTags = StaEQPManagager.RacksList.SelectMany(rack => rack.RackOption.ColumnTagMap.Values.SelectMany(tag => tag)).ToList();
            isEqLocatinRack = RackTags.Contains(tag);
            var slotOptions = StaEQPManagager.MainEQList.Where(eq => eq.EndPointOptions.TagID == tag)
                                             .Select(eq => new SlotOption
                                             {
                                                 Value = eq.EndPointOptions.Height + "",
                                                 Label = eq.EndPointOptions.Name + $"(第{eq.EndPointOptions.Height + 1})層"
                                             })
                                             .ToList();
            if (isEqLocatinRack)
            {
                var locatinRack = StaEQPManagager.RacksList.FirstOrDefault(rack => rack.PortsStatus.Any(port => port.TagNumbers.Contains(tag)));
                for (int i = 1; i < locatinRack.RackOption.Rows; i++)
                {
                    slotOptions.Add(new SlotOption
                    {
                        Value = i + "",
                        Label = $"第{i + 1}層"
                    });
                }

            }
            return slotOptions;
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
