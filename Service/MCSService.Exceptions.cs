﻿using AGVSystemCommonNet6.AGVDispatch;
using AGVSystemCommonNet6.Alarm;
using static AGVSystemCommonNet6.Alarm.SECS_Alarm_Code.SECSHCACKAlarmCodeMapper;

namespace AGVSystem.Service.MCS
{
    public partial class MCSService
    {
        public class HasIDbutNoCargoException : Exception
        {
            public HasIDbutNoCargoException() : base() { }
            public HasIDbutNoCargoException(string message) : base(message)
            {

            }
        }

        public class AddOrderFailException : Exception
        {
            public readonly ALARMS alarmCode;
            public readonly MapResult alarmCodeMap;
            public readonly clsTaskDto order;
            public AddOrderFailException(string message, ALARMS alarmCode, clsTaskDto order, MapResult mapResult) : base(message)
            {
                this.alarmCode = alarmCode;
                this.order = order;
                this.alarmCodeMap = mapResult;
            }
        }

        public class ZoneIsFullException : Exception
        {
            public ZoneIsFullException(string message) : base(message)
            {

            }
        }

        public class SourceIsEmptyException : Exception
        {
            public SourceIsEmptyException(string message) : base(message)
            {
            }
        }
    }
}
