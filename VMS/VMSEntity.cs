using AGVSytemCommon;
using AGVSytemCommon.AGVMessage;
using Newtonsoft.Json;
using System.Diagnostics.Eventing.Reader;
using System.Net.Sockets;
using System.Text;
using static AGVSytemCommon.clsEnums;

namespace AGVSystem.VMS
{
    public class VMSEntity
    {
        private Socket client_socket;

        public enum MESSAGE_DATA_FORMAT
        {
            JSON_CLASS,
            JSON_DICTIONARY
        }

        public MESSAGE_DATA_FORMAT MessageDataFormat = MESSAGE_DATA_FORMAT.JSON_DICTIONARY;
        public clsSocketState socket_state;


        public VMSBaseProp BaseProps = new VMSBaseProp();
        public ONLINE_STATE Online_State = ONLINE_STATE.OFFLINE;
        public cls_0105_RunningStatusReportHeader Running_Status = new cls_0105_RunningStatusReportHeader();


        private ONLINE_STATE online_state_req = ONLINE_STATE.OFFLINE;

        public VMSEntity()
        {
        }
        public VMSEntity(Socket client_socket = null, string AGV_Name = "")
        {
            this.BaseProps.AGV_Name = AGV_Name;
            if (client_socket != null)
            {
                this.client_socket = client_socket;
                socket_state = new clsSocketState(client_socket);
                client_socket.BeginReceive(socket_state.buffer, 0, 65535, SocketFlags.None, new AsyncCallback(DataRecievedCallBack), socket_state);
            }
        }

        private void DataRecievedCallBack(IAsyncResult ar)
        {
            try
            {
                clsSocketState state = (clsSocketState)ar.AsyncState;
                int rev_len = state.socket.EndReceive(ar);
                string rev_string = Encoding.ASCII.GetString(state.buffer, state.RecievedDataLen, rev_len);
                state.RecievedDataLen += rev_len;
                state.RevievedDataString += rev_string;

                if (state.RevievedDataString.EndsWith("*\r"))
                {
                    Console.WriteLine($"Recieved Data From : {client_socket.RemoteEndPoint}　Detectd End Char");

                    clsAGVSMessage[] messages = new clsAGVSMessage[0];
                    if (MessageDataFormat == MESSAGE_DATA_FORMAT.JSON_DICTIONARY)
                    {
                        messages = MessageParser.MsgParse(state.RevievedDataString);
                    }

                    HandleMessage(messages);

                    state.ResetBuffer();
                }

                state.socket.BeginReceive(state.buffer, state.RecievedDataLen, 65535 - state.RecievedDataLen, SocketFlags.None, new AsyncCallback(DataRecievedCallBack), state);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Running_Status.AGV_Status = (int)AGV_STATUS.DOWN;
                return;
            }

        }
        public void HandleMessage(clsAGVSMessage msg)
        {

            string headerKey = msg.Header.Keys.First();
            clsHeader headerContent = msg.Header.Values.First();
            Console.WriteLine($"Recieved Data From : {client_socket.RemoteEndPoint}　：　Header:{headerKey}");

            clsHeader headerDataReply = null;
            string headerKey_reply = "";
            clsAGVSMessage messageReply = new clsAGVSMessage()
            {
                EQName = msg.EQName,
                SID = msg.SID,
                System_Bytes = msg.System_Bytes,
                Header = new Dictionary<string, clsHeader>()
            };
            switch (headerKey)
            {

                case "0101":
                    BaseProps.AGV_Name = msg.EQName;
                    BaseProps.AGV_SID = msg.SID;

                    headerKey_reply = "0102";
                    headerDataReply = new cls_0102_OnlineModeQueryAckHeader()
                    {
                        Remote_Mode = (int)online_state_req
                    };
                    break;
                case "0103":
                    cls_0103_OnlineRequestHeader online_request = (cls_0103_OnlineRequestHeader)headerContent;
                    int return_code = IsAGVOnlineModeChangeable(online_request, out string message);


                    headerKey_reply = "0104";
                    headerDataReply = new clsReturnCode()
                    {
                        Process_Result = 0,
                        Return_Code = return_code
                    };
                    Online_State = online_request.Mode_Request == 0 ? ONLINE_STATE.OFFLINE : ONLINE_STATE.ONLINE;

                    break;
                case "0105":
                    Running_Status = (cls_0105_RunningStatusReportHeader)headerContent;
                    headerKey_reply = "0106";
                    headerDataReply = new clsReturnCode()
                    {
                        Process_Result = 0,
                        Return_Code = 0
                    };
                    break;
                case "0106":
                    break;
                default:
                    break;
            }

            if (headerDataReply != null)
            {
                messageReply.Header.Add(headerKey_reply, headerDataReply);
                string message_sendout = CreateMessageToSendOut(messageReply);
                client_socket.Send(Encoding.ASCII.GetBytes(message_sendout));
            }
        }
        private int IsAGVOnlineModeChangeable(ONLINE_STATE mode_request, out string message)
        {
            message = "";
            AGV_STATUS current_agv_status = Running_Status.GetAGVStatus();
            if (mode_request == ONLINE_STATE.ONLINE)
            {
                if (Running_Status.Last_Visited_Node == 0)
                {
                    message = "AGV 必須停在Tag上";
                    return 1;
                }
                if (current_agv_status != AGV_STATUS.IDLE)
                {
                    message = "AGV 狀態不可上線";
                    return 1;
                }
            }
            return 0;
        }
        private int IsAGVOnlineModeChangeable(cls_0103_OnlineRequestHeader online_request, out string message)
        {
            ONLINE_STATE mode_req = online_request.GetOnlineReqMode();
            return IsAGVOnlineModeChangeable(mode_req, out message);
        }

        public void HandleMessage(clsAGVSMessage[] messages)
        {
            foreach (var msg in messages)
            {
                HandleMessage(msg);

            }
        }

        /// <summary>
        /// 要求AGV上線
        /// </summary>
        /// <param name="mode"></param>
        public void OnlineModeChangeRequest(ONLINE_STATE mode)
        {
            if (IsAGVOnlineModeChangeable(mode, out string message) != 0)
            {
                Console.WriteLine($"AGV無法進行{mode}動作:{message}");
                return;
            }
            online_state_req = mode;
        }

        private string CreateMessageToSendOut(clsAGVSMessage messageReply)
        {
            if (MessageDataFormat == MESSAGE_DATA_FORMAT.JSON_CLASS)
            {
                return JsonConvert.SerializeObject(messageReply) + "*\r";
            }
            else
            {
                return JsonConvert.SerializeObject(MessageParser.GetDictionary(messageReply)) + "*\r";
            }
        }

        internal void UseNewSocket(Socket socket)
        {
            client_socket = socket;
            socket_state.socket = socket;
            socket_state.ResetBuffer();
            client_socket.BeginReceive(socket_state.buffer, 0, 65535, SocketFlags.None, new AsyncCallback(DataRecievedCallBack), socket_state);
        }


        /// <summary>
        /// 向車載系統詢問Alive狀態
        /// </summary>
        /// <returns>是否Alive</returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual async Task<bool> AliveCheck()
        {
            throw new NotImplementedException();
        }

    }
}
