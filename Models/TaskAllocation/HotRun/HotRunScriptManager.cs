using AGVSystem.TaskManagers;
using AGVSystemCommonNet6;
using AGVSystemCommonNet6.AGVDispatch.Messages;
using AGVSystemCommonNet6.DATABASE.Helpers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NuGet.Configuration;
using NuGet.ContentModel;
using System.Threading;

namespace AGVSystem.Models.TaskAllocation.HotRun
{
    public class HotRunScriptManager
    {
        public static HotRunScript[] HotRunScripts { get; set; } = new HotRunScript[0];
        public static bool Save(HotRunScript[] settings)
        {
            HotRunScripts = settings;
            var folder = "C://AGVS";
            Directory.CreateDirectory(folder);
            var filename = Path.Combine(folder, "HotRunScripts.json");
            System.IO.File.WriteAllText(filename, JsonConvert.SerializeObject(settings, Formatting.Indented));
            return true;
        }
        public static void ReloadHotRunScripts()
        {
            var folder = "C://AGVS";
            Directory.CreateDirectory(folder);
            var filename = Path.Combine(folder, "HotRunScripts.json");
            if (File.Exists(filename))
            {
                HotRunScripts = JsonConvert.DeserializeObject<HotRunScript[]>(System.IO.File.ReadAllText(filename));
                foreach (var script in HotRunScripts)
                {
                    script.state = "IDLE";
                }
            }
        }
        public static void Initialize()
        {
            ReloadHotRunScripts();
        }
        public static void Run(int no)
        {
            var script = HotRunScripts.FirstOrDefault(script => script.no == no);
            if (script != null)
            {
                script.cancellationTokenSource = new CancellationTokenSource();
                StartHotRun(script);
            }
        }

        internal static void Stop(int no)
        {
            var script = HotRunScripts.FirstOrDefault(script => script.no == no);
            if (script != null)
            {
                script.cancellationTokenSource?.Cancel();
            }
        }
        private static void StartHotRun(HotRunScript script)
        {

            AGVStatusDBHelper agv_status_db = new AGVStatusDBHelper();
            clsAGVStateDto? agv = agv_status_db.GetALL().FirstOrDefault(agv => agv.AGV_Name == script.agv_name);

            Task.Factory.StartNew(async () =>
            {
                script.finish_num = 0;
                clsAGVStateDto GetAGVState()
                {
                    return agv_status_db.GetAGVStateByName(script.agv_name);
                }
                script.state = "Running";
                UpdateScriptState(script);

                //等待AGV可做任務
                var agvstates = GetAGVState();
                while (agvstates.OnlineStatus == clsEnums.ONLINE_STATE.OFFLINE | !agvstates.Connected | agvstates.MainStatus == clsEnums.MAIN_STATUS.DOWN | agvstates.MainStatus == clsEnums.MAIN_STATUS.RUN)
                {
                    await Task.Delay(100);
                    if (script.cancellationTokenSource.IsCancellationRequested)
                    {
                        script.state = "IDLE";
                        return;
                    }
                    agvstates = GetAGVState();
                }

                while (script.finish_num != script.loop_num)
                {
                    await Task.Delay(1);
                    if (script.cancellationTokenSource.IsCancellationRequested)
                    {
                        script.state = "IDLE";
                        return;
                    }
                    if (agv != null)
                    {
                        foreach (HotRunAction _action in script.actions)
                        {
                            var TaskName = $"HR_{DateTime.Now.ToString("yyMMdd_HHmmssffff")}";
                            await TaskManager.AddTask(new AGVSystemCommonNet6.TASK.clsTaskDto
                            {
                                Action = GetActionByActionName(_action.action),
                                From_Station = _action.source_tag.ToString(),
                                To_Station = _action.action == "measure" ? _action.destine_name : _action.destine_tag.ToString(),
                                DispatcherName = "Hot_Run",
                                Carrier_ID = "",
                                TaskName = TaskName,
                                DesignatedAGVName = script.agv_name
                            });
                            TaskDatabaseHelper dbH = new TaskDatabaseHelper();

                            var status = await dbH.GetTaskStateByID(TaskName);

                            while (status != TASK_RUN_STATUS.NAVIGATING)
                            {
                                status = await dbH.GetTaskStateByID(TaskName);
                                if (script.cancellationTokenSource.IsCancellationRequested)
                                {
                                    script.state = "IDLE";
                                    return;
                                }
                                if (status != TASK_RUN_STATUS.WAIT)
                                    break;
                                await Task.Delay(1000);
                            }

                            while (status != TASK_RUN_STATUS.ACTION_FINISH)
                            {
                                status = await dbH.GetTaskStateByID(TaskName);
                                if (script.cancellationTokenSource.IsCancellationRequested)
                                {
                                    script.state = "IDLE";
                                    return;
                                }

                                if (status != TASK_RUN_STATUS.NAVIGATING)
                                    break;

                                await Task.Delay(1000);
                            }
                        }

                        script.finish_num += 1;
                        UpdateScriptState(script);
                    }
                    else
                    {
                        return;
                    }
                }

                script.state = "IDLE";
                UpdateScriptState(script);
                Console.WriteLine("Hot Run Finish.");
            });
        }

        private static ACTION_TYPE GetActionByActionName(string action)
        {
            if (action == "move")
                return ACTION_TYPE.None;
            if (action == "charge")
                return ACTION_TYPE.Charge;
            if (action == "park")
                return ACTION_TYPE.Park;
            if (action == "load")
                return ACTION_TYPE.Load;
            if (action == "unload")
                return ACTION_TYPE.Unload;
            if (action == "carry")
                return ACTION_TYPE.Carry;
            if (action == "measure")
                return ACTION_TYPE.Measure;

            if (action == "exchange_battery")
                return ACTION_TYPE.ExchangeBattery;
            else
                return ACTION_TYPE.None;
        }

        private static void UpdateScriptState(HotRunScript script)
        {
            HotRunScript? _script = HotRunScripts.FirstOrDefault(s => s.no == script.no);
            _script.finish_num = script.finish_num;
            _script.state = script.state;

        }

    }
}
