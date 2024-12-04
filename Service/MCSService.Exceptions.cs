using AGVSystemCommonNet6.Alarm;

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
            public AddOrderFailException(string message, ALARMS alarmCode) : base(message)
            {
                this.alarmCode = alarmCode;
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
