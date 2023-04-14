using System.Net.Sockets;

namespace AGVSystem.VMS
{
    public class VMSManager
    {
        public static List<string> AcceptAGVCIP = new List<string>() { "192.168.0.101", "192.168.0.102", "192.168.0.103", "192.168.0.104", "127.0.0.1" };

        public static Dictionary<string, VMSEntity> VMSList = new Dictionary<string, VMSEntity>()
        {
        };
        public static bool TryAddVMS(Socket socket)
        {
            string client_IP = socket.RemoteEndPoint.ToString().Split(':')[0];
            if (AcceptAGVCIP.Contains(client_IP))
            {
                Console.WriteLine($"AGVC Client({client_IP}) connect in ");

                if (!VMSList.ContainsKey(client_IP))
                {
                    VMSList.Add(client_IP, new VMSEntity(socket));
                }
                else
                {
                    VMSList[client_IP].UseNewSocket(socket);
                }

                return true;
            }
            else
                return false;

        }
        public static VMSEntity GetVMSByIP(string ip)
        {
            if (VMSList.TryGetValue(ip, out VMSEntity vms))
                return vms;
            else
                return null;
        }
        public static VMSEntity GetVMSByAGV_Name(string AGV_Name)
        {
            VMSEntity? vms = VMSList.Values.FirstOrDefault(d => d.BaseProps.AGV_Name == AGV_Name);
            if (vms != null)
                return vms;
            else
                return null;
        }

        internal static async Task<(bool success, string message)> AGVOffline(string AGV_Name)
        {
            VMSEntity vms = GetVMSByAGV_Name(AGV_Name);
            if (vms == null)
                return (false, $"{AGV_Name} doesn't exist");

            return await vms.OnlineModeChangeRequest(AGVSytemCommonNet6.clsEnums.ONLINE_STATE.OFFLINE);
        }
        public static async Task<(bool success, string message)> AGVOnline(string AGV_Name)
        {
            VMSEntity vms = GetVMSByAGV_Name(AGV_Name);
            if (vms == null)
                return (false, $"{AGV_Name} doesn't exist");

            return await vms.OnlineModeChangeRequest(AGVSytemCommonNet6.clsEnums.ONLINE_STATE.ONLINE);
        }

        internal static Dictionary<string, ViewModel.VMSViewModel> GetVMSViewData()
        {
            return VMSList.ToDictionary(vms => vms.Key, vms => new ViewModel.VMSViewModel()
            {
                BaseProps = vms.Value.BaseProps,
                OnlineStatus = vms.Value.Online_State,
                RunningStatus = vms.Value.Running_Status,
            });
        }

    }
}
