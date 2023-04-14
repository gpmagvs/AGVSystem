using AGVSytemCommonNet6;
using AGVSytemCommonNet6.AGVMessage;
using static AGVSytemCommonNet6.clsEnums;

namespace AGVSystem.ViewModel
{
    public class VMSViewModel : IDisposable
    {
        private bool disposedValue;

        public cls_0105_RunningStatusReportHeader RunningStatus { get; set; }
        public ONLINE_STATE OnlineStatus { get; set; }
        public VMSBaseProp BaseProps { get; set; }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }

                RunningStatus = null;
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
