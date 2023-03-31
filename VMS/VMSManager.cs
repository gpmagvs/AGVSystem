using System.Net.Sockets;

namespace AGVSystem.VMS
{
    public class VMSManager
    {
        public static List<string> AcceptAGVCIP = new List<string>() { "192.168.0.101", "192.168.0.102", "192.168.0.103", "192.168.0.104", "127.0.0.1" };

        public static Dictionary<string, VMSEntity> VMSList = new Dictionary<string, VMSEntity>()
        {
            {"YUNAGV_01" , new GPMxYUNTech_FORK.clsYunForkVMS("YUNAGV_01"){} },
            {"YUNAGV_02" , new GPMxYUNTech_FORK.clsYunForkVMS("YUNAGV_02")},
            {"FORK_AGV_02" , new VMSEntity(AGV_Name:"FORK_AGV_02")},
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

        public static void AGVOnline(string ip)
        {
            var vms = GetVMSByIP(ip);
            if (vms != null)
            {
                vms.OnlineModeChangeRequest(AGVSytemCommon.clsEnums.ONLINE_STATE.ONLINE);
            }

        }

        internal static Dictionary<string, ViewModel.VMSViewModel> GetVMSViewData()
        {
            VMSList.First().Value.Running_Status.Last_Visited_Node = 71;
            VMSList.Last().Value.Running_Status.Last_Visited_Node = 53;
            return VMSList.ToDictionary(vms => vms.Key, vms => new ViewModel.VMSViewModel()
            {
                BaseProps = vms.Value.BaseProps,
                OnlineStatus = vms.Value.Online_State,
                RunningStatus = vms.Value.Running_Status,
            });
        }
    }
}
