using AGVSytemCommonNet6.HttpHelper;
using AGVSystem.VMS.GPMxYUNTech_FORK.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static AGVSytemCommonNet6.clsEnums;

namespace AGVSystem.VMS.GPMxYUNTech_FORK
{
    public class clsYunForkVMS : VMSEntity
    {

        public string APIHost = "http://192.168.56.1:5858/";

        public clsYunForkVMS(string AGV_Name)
        {
            agv_model = AGV_MODEL.YUNTECH_FORK_AGV;
            BaseProps = new AGVSytemCommonNet6.VMSBaseProp
            {
                AGV_Name = AGV_Name,
                AGV_SID = "001:001:100",
            };
        }

        override public async Task<bool> AliveCheck()
        {
            (HttpResponseMessage response, string content) response = await Http.Get(APIHost, "SystemTime");
            Console.WriteLine(response.content);
            if (response.response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine($"Alive Check Fail...{response.content}");
                return false;
            }
            return true;

        }

        /// <summary>
        /// 取得任務狀態
        /// </summary>
        /// <returns></returns>
        public async Task< GPMxYUNTech_FORK.Models.clsTaskStatus> GetTaskStatus()
        {

            (HttpResponseMessage response, string content) resp = await Http.Get(APIHost, "GetTaskStatus");
            Console.WriteLine(resp.content);

            if (resp.response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string json = GetJsonContent(resp.content);
                var _data = JsonConvert.DeserializeObject<JArray>(json);
                var _runningTasks = _data.First();
                var _completedTasks = _data.Last();
                GPMxYUNTech_FORK.Models.clsTaskStatus task_status = new GPMxYUNTech_FORK.Models.clsTaskStatus();
                task_status.RunningTasks = JsonConvert.DeserializeObject<List<clsTaskStatusItem>>(_runningTasks.ToString());
                task_status.CompletedTasks = JsonConvert.DeserializeObject<List<clsTaskStatusItem>>(_completedTasks.ToString());
                return task_status;
            }
            else
                return null;
        }


        /// <summary>
        /// 取得PLC IO 狀態
        /// </summary>
        /// <returns></returns>
        public async Task<clsPLCStatus> GetPLCIOStatus()
        {
            (HttpResponseMessage response, string content) resp = await Http.Get(APIHost, "GetPLC");
            Console.WriteLine(resp.content);

            if (resp.response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<clsPLCStatus>(GetJsonContent(resp.content));
            }
            else
                return null;

        }

        private string GetJsonContent(string respContent)
        {
            var msgObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(respContent);
            return msgObj["message"];
        }
    }
}
