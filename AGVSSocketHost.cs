using System.Net;
using System.Net.Sockets;
using AGVSystem.VMS;

namespace AGVSystem
{
    public class AGVSSocketHost
    {
        private Socket hostSocket;
        public clsAGVSConfig Configs = new clsAGVSConfig();
        public AGVSSocketHost()
        {

        }
        public class clsAGVSConfig
        {
        }
        public void Start()
        {
            ServerBuild();
        }

        private void ServerBuild()
        {
            // 設定伺服器 IP 位址和 Port
            string ipAddress = "127.0.0.1";
            int port = 8888;
            IPAddress serverIPAddress = IPAddress.Parse(ipAddress);
            IPEndPoint serverEndPoint = new IPEndPoint(serverIPAddress, port);
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(serverEndPoint);
            serverSocket.Listen(10);
            BeginAcceptAsync(serverSocket);
        }

        private async void BeginAcceptAsync(Socket serverSocket)
        {

            await Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    Socket clientSocket = serverSocket.Accept();
                    VMSManager.TryAddVMS(clientSocket);
                }
            });
        }
    }
}
