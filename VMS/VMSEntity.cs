using AGVSytemCommonNet6;
using AGVSytemCommonNet6.AGVMessage;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Net.Sockets;
using System.Text;
using static AGVSystem.VMS.clsTaskStatus;
using static AGVSytemCommonNet6.clsEnums;

namespace AGVSystem.VMS
{
    public class VMSEntity
    {

        private List<clsTaskStatus> TasksBuffer = new List<clsTaskStatus>();

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

        public AGV_MODEL agv_model { get; protected set; }
        public clsTaskStatus ExecutingTask { get; private set; } = new clsTaskStatus { Task_Run_Status = TASK_EXECUTE_STATE.FINISH };

        public VMSEntity()
        {
            TaskDispatchWorker();
        }
        public VMSEntity(Socket client_socket = null, string AGV_Name = "")
        {
            this.agv_model = AGV_MODEL.FORK_AGV;
            this.BaseProps.AGV_Name = AGV_Name;
            if (client_socket != null)
            {
                this.client_socket = client_socket;
                socket_state = new clsSocketState(client_socket);
                client_socket.BeginReceive(socket_state.buffer, 0, 65535, SocketFlags.None, new AsyncCallback(DataRecievedCallBack), socket_state);

            }
            TaskDispatchWorker();
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
                online_state_req = ONLINE_STATE.OFFLINE;
                Console.WriteLine(ex.ToString());
                Running_Status.AGV_Status = (int)MAIN_STATUS.DOWN;
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
                    if (return_code == 0)
                    {
                        online_state_req = Online_State;
                    }

                    break;
                case "0105":
                    var Running_Status = (cls_0105_RunningStatusReportHeader)headerContent;
                    if (Running_Status.Last_Visited_Node != this.Running_Status.Last_Visited_Node)
                    {
                        Console.WriteLine($"AGV Position Change TO {Running_Status.Last_Visited_Node}");
                    }
                    this.Running_Status = Running_Status;
                    headerKey_reply = "0106";
                    headerDataReply = new clsReturnCode()
                    {
                        Process_Result = 0,
                        Return_Code = 0
                    };
                    break;
                case "0106":
                    break;
                case "0302":
                    clsReturnCode task_download_ack = (clsReturnCode)headerContent;
                    if (task_download_ack.Return_Code == 0)
                    {
                        //vms接受
                        ExecutingTask.Task_Run_Status = TASK_EXECUTE_STATE.EXECUTING;
                        ExecutingTask.UpdateExecutingTaskState(TASK_EXECUTE_STATE.EXECUTING);
                    }
                    else
                    {
                    }
                    break;
                case "0303":

                    cls_0303_TaskFeedback task_feedback = (cls_0303_TaskFeedback)headerContent;
                    RunningTaskUpdate(task_feedback);
                    headerKey_reply = "0304";
                    headerDataReply = new clsReturnCode()
                    {
                        Process_Result = 0,
                        Return_Code = 0
                    };
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

        private void RunningTaskUpdate(cls_0303_TaskFeedback task_feedback)
        {
            Console.WriteLine($"{task_feedback.Task_Simplex} : Current Path Point = {task_feedback.Point_Index}");
            ExecutingTask.UpdateExecutingTaskState(task_feedback);
            if (ExecutingTask.WaitForRunTaskSimplex.Count != 0)
            {
                var taskSimplex = ExecutingTask.WaitForRunTaskSimplex.First();
                Send0301TaskDownload(ExecutingTask.TaskLinks.First(t => t.Task_Simplex == taskSimplex));
            }
            else
            {
                ExecutingTask.Task_Run_Status = TASK_EXECUTE_STATE.FINISH;
            }
        }

        private bool IsAGVOnlineModeChangeable(ONLINE_STATE mode_request, out string message)
        {
            message = "";
            MAIN_STATUS current_agv_status = Running_Status.GetAGVStatus();
            if (mode_request == ONLINE_STATE.ONLINE)
            {
                if (Running_Status.Last_Visited_Node == 0)
                {
                    message = "AGV 必須停在Tag上";
                    return false;
                }
                if (current_agv_status != MAIN_STATUS.IDLE)
                {
                    message = "AGV 狀態不可上線";
                    return false;
                }
            }
            return true;
        }
        private int IsAGVOnlineModeChangeable(cls_0103_OnlineRequestHeader online_request, out string message)
        {
            ONLINE_STATE mode_req = online_request.GetOnlineReqMode();

            return IsAGVOnlineModeChangeable(mode_req, out message) ? 0 : 1;
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
        public async Task<(bool success, string message)> OnlineModeChangeRequest(ONLINE_STATE mode)
        {
            if (!IsAGVOnlineModeChangeable(mode, out string message))
            {
                Console.WriteLine($"AGV無法進行{mode}動作:{message}");
                return (false, message);
            }

            online_state_req = mode;

            //開始監視

            CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            while (Online_State != online_state_req)
            {
                await Task.Delay(1);
                if (cts.IsCancellationRequested)
                {
                    return (false, "Timeout,VMS尚未切換ONLine.OFFLine狀態");
                }
            }
            cts.Dispose();
            online_state_req = Online_State;
            return (true, message);

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

        /// <summary>
        /// 會將任務暫存 不會立即執行
        /// </summary>
        /// <param name="taskLinks"></param>
        internal void JoinTask(List<cls_0301_TaskDownloadHeader> taskLinks)
        {
            TasksBuffer.Add(new clsTaskStatus
            {
                TaskName = taskLinks.First().Task_Name,
                SliceTask_Run_States = taskLinks.ToDictionary(task => task.Task_Simplex, task => TASK_EXECUTE_STATE.WAIT),
                TaskLinks = taskLinks
            });
        }

        internal void TaskDispatchWorker()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(100);
                    if (Running_Status.AGV_Status != 1| Online_State != ONLINE_STATE.ONLINE) //非IDLE &　Online狀態
                        continue;

                    if (ExecutingTask.Task_Run_Status == TASK_EXECUTE_STATE.EXECUTING)
                        continue;
                    if (TasksBuffer.Count == 0)
                        continue;

                    List<clsTaskStatus> task_ordered_by_priority = TasksBuffer.FindAll(t => t.Task_Run_Status == TASK_EXECUTE_STATE.WAIT).OrderByDescending(t => t.Priority).ToList();
                    if (task_ordered_by_priority.Count == 0)
                        continue;


                    ExecutingTask = task_ordered_by_priority.First();
                    ExecutingTask.Task_Run_Status = TASK_EXECUTE_STATE.WAIT;
                    Send0301TaskDownload(ExecutingTask.TaskLinks.First());
                }

            });
        }
        private uint system_bytes_active_send = 320000;
        private void Send0301TaskDownload(cls_0301_TaskDownloadHeader _0301_TaskDownloadHeader)
        {
            string message_sendout = CreateMessageToSendOut(new clsAGVSMessage
            {
                EQName = BaseProps.AGV_Name,
                SID = BaseProps.AGV_SID,
                System_Bytes = system_bytes_active_send,
                Header = new Dictionary<string, clsHeader>()
                {
                    { "0301", _0301_TaskDownloadHeader }
                }
            });
            system_bytes_active_send += 1;
            ExecutingTask.RunningTask_Simplex = _0301_TaskDownloadHeader.Task_Simplex;
            client_socket.Send(Encoding.ASCII.GetBytes(message_sendout));

        }
    }
    public class clsTaskStatus
    {

        public enum TASK_EXECUTE_STATE
        {
            WAIT,
            EXECUTING,
            FINISH,
            CANCELED
        }

        public TASK_EXECUTE_STATE Task_Run_Status { get; set; } = TASK_EXECUTE_STATE.WAIT;
        public string TaskName { get; set; }

        /// <summary>
        /// 優先度
        /// </summary>
        public int Priority { get; set; } = 50;
        public List<cls_0301_TaskDownloadHeader> TaskLinks { get; internal set; }

        /// <summary>
        /// 等待執行的子任務
        /// </summary>
        public List<string> WaitForRunTaskSimplex
        {
            get
            {
                return SliceTask_Run_States.ToList().FindAll(t => t.Value == TASK_EXECUTE_STATE.WAIT).Select(t => t.Key).ToList();
            }
        }
        /// <summary>
        /// 已經完成的子任務
        /// </summary>
        public List<string> FinishRunTaskSimplex
        {
            get
            {
                return SliceTask_Run_States.ToList().FindAll(t => t.Value == TASK_EXECUTE_STATE.FINISH).Select(t => t.Key).ToList();
            }
        }

        public string RunningTask_Simplex { get; internal set; }

        /// <summary>
        /// 存放每一個切片任務的執行狀態
        /// </summary>

        public Dictionary<string, TASK_EXECUTE_STATE> SliceTask_Run_States = new Dictionary<string, TASK_EXECUTE_STATE>();

        internal cls_0301_TaskDownloadHeader GetFirstSimplexTaskToExecuting()
        {
            return TaskLinks.First();
        }

        internal void UpdateExecutingTaskState(TASK_EXECUTE_STATE task_state)
        {
            SliceTask_Run_States[RunningTask_Simplex] = task_state;
        }

        internal void UpdateExecutingTaskState(cls_0303_TaskFeedback task_feedback)
        {
            List<int> running_status_code = new List<int> { 1, 2, 3 };
            bool isRunning = running_status_code.Contains(task_feedback.Task_Status);
            SliceTask_Run_States[task_feedback.Task_Simplex] = isRunning ? TASK_EXECUTE_STATE.EXECUTING : TASK_EXECUTE_STATE.FINISH;
        }
    }
}
