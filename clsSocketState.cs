using System.Net.Sockets;

namespace AGVSystem
{
    public class clsSocketState
    {
        public Socket socket { get; set; }
        public byte[] buffer = new byte[65535];
        public int RecievedDataLen { get; set; } = 0;
        public string RevievedDataString { get; set; }

        public clsSocketState(Socket socket)
        {
            this.socket = socket;
        }

        public void ResetBuffer()
        {
            buffer = null;
            buffer = new byte[65535];
            RecievedDataLen = 0;
            RevievedDataString = string.Empty;
        }
    }
}