using System.Net;
using System.Net.Sockets;

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
        public async Task<(bool success, string message)> Start()
        {
            return await ServerBuild();
        }

        private async Task<(bool success, string message)> ServerBuild()
        {
            try
            {
                // 設定伺服器 IP 位址和 Port
                string ipAddress = "127.0.0.1";
                int port = 15500;
                IPAddress serverIPAddress = IPAddress.Parse(ipAddress);
                IPEndPoint serverEndPoint = new IPEndPoint(serverIPAddress, port);
                Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Bind(serverEndPoint);
                serverSocket.Listen(10);
                BeginAcceptAsync(serverSocket);
                return (true, $"{ipAddress}:{port}");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        private async void BeginAcceptAsync(Socket serverSocket)
        {

            await Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    Socket clientSocket = serverSocket.Accept();
                }
            });
        }
    }
}
