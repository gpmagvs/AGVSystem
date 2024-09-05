using AGVSystem.TaskManagers;
using AGVSystemCommonNet6;
using AGVSystemCommonNet6.AGVDispatch;
using AGVSystemCommonNet6.AGVDispatch.Messages;
using AGVSystemCommonNet6.Alarm;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.DATABASE.Helpers;
using AGVSystemCommonNet6.Microservices.VMS;
using AGVSystemCommonNet6.Notify;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.ObjectModelRemoting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore.Update;
using Newtonsoft.Json;
using NLog;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.ContentModel;
using System.Threading;

namespace AGVSystem.Models.TaskAllocation.HotRun
{
    public class HotRunScriptManager
    {
        public static List<HotRunScript> HotRunScripts { get; set; } = new List<HotRunScript>();
        public static List<HotRunScript> RunningScriptList = new List<HotRunScript>();
        public static bool Save(List<HotRunScript> settings)
        {
            SaveSyncHotRunScripts(settings);
            HotRunScripts = settings;
            SaveScriptsToJsonFile(settings);
            return true;
        }

        private static void SaveScriptsToJsonFile(List<HotRunScript> settings)
        {
            var folder = "C://AGVS";
            Directory.CreateDirectory(folder);
            var filename = Path.Combine(folder, "HotRunScripts.json");
            System.IO.File.WriteAllText(filename, JsonConvert.SerializeObject(settings, Formatting.Indented));
        }

        public static async Task ReloadHotRunScripts()
        {
            var folder = "C://AGVS";
            Directory.CreateDirectory(folder);
            var filename = Path.Combine(folder, "HotRunScripts.json");
            if (File.Exists(filename))
            {
                HotRunScripts = JsonConvert.DeserializeObject<List<HotRunScript>>(System.IO.File.ReadAllText(filename));
                bool _AnyScriptIDCreated = false;
                foreach (var script in HotRunScripts)
                {
                    script.state = "IDLE";
                    if (string.IsNullOrEmpty(script.scriptID))
                    {
                        await Task.Delay(100);
                        script.scriptID = $"AutoCreatedID-{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-ffff")}";
                        _AnyScriptIDCreated = true;
                    }
                }

                Save(HotRunScripts);
            }
        }
        public static void Initialize()
        {
            ReloadHotRunScripts();
        }
        public static (bool, string) Run(string scriptID)
        {
            var script = HotRunScripts.FirstOrDefault(script => script.scriptID == scriptID);
            if (script != null)
            {
                script.cancellationTokenSource = new CancellationTokenSource();
                return StartHotRun(script);
            }
            else
                return (false, "Script not exist");
        }

        internal static void Stop(string scriptID)
        {
            var script = RunningScriptList.FirstOrDefault(script => script.scriptID == scriptID);
            if (script != null)
            {
                script.StopFlag = true;
                script.cancellationTokenSource?.Cancel();
                script.cancellationTokenSource = null;

                RunningScriptList.Remove(script);
            }
            else
            {
                var _script = HotRunScripts.FirstOrDefault(sc => sc.scriptID == scriptID);
                _script.state = "IDLE";
            }
        }

        private static void SaveSyncHotRunScripts(List<HotRunScript> newScripts)
        {
            var scriptsIDCollection = newScripts.Select(script => script.scriptID);
            var originalScriptIDCollection = HotRunScripts.Select(script => script.scriptID);
            if (scriptsIDCollection.Any())
            {
                //select out not exist scripts
                IEnumerable<HotRunScript> deletedScripts = HotRunScripts.TakeWhile(script => !scriptsIDCollection.Contains(script.scriptID));
                if (deletedScripts.Any())
                    StopScript(deletedScripts);


                //handle just edited maybe scripts
                IEnumerable<HotRunScript> remainOldScripts = HotRunScripts.Where(script => !deletedScripts.Contains(script));
                foreach (HotRunScript script in remainOldScripts)
                {
                    HotRunScript editedScriptFound = newScripts.First(editedScript => editedScript.scriptID == script.scriptID);
                    script.SyncSetting(editedScriptFound);
                }

                //new scripts
                var newToAddScripts = newScripts.Where(script => !originalScriptIDCollection.Contains(script.scriptID));
                foreach (var item in newToAddScripts)
                {
                    item.scriptID = $"{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-ffff")}";
                }
                HotRunScripts.AddRange(newToAddScripts);
                HotRunScripts = HotRunScripts.SkipWhile(script => deletedScripts.Any(s => s.scriptID == script.scriptID)).ToList();

            }
            else//新設定中腳本ID集合為空=>表示沒有任何腳本
            {
                HotRunScripts = new List<HotRunScript>();
            }
            SaveScriptsToJsonFile(HotRunScripts);
        }

        private static void StopScript(IEnumerable<HotRunScript> scripts)
        {
            foreach (var script in scripts)
            {
                script.StopFlag = true;
                script.cancellationTokenSource?.Cancel();
                script.cancellationTokenSource = null;
            }
        }

        private static (bool, string) StartHotRun(HotRunScript script)
        {
            if (script.IsRandomCarryRun)
            {
                return StartRandomCarryHotRun(script);
            }

            clsAGVStateDto GetAGVState()
            {
                return DatabaseCaches.Vehicle.VehicleStates.FirstOrDefault(s => s.AGV_Name == script.agv_name);
            }
            clsAGVStateDto? agv = GetAGVState();
            if (agv == null)
            {
                return (false, $"Null!");
            }
            var firstAction = GetActionByActionName(script.actions.First().action);
            if (firstAction == ACTION_TYPE.Load && agv.CurrentCarrierID == "")
            {
                Task<clsAlarmDto> res = AlarmManagerCenter.AddAlarmAsync(ALARMS.CANNOT_DISPATCH_LOAD_TASK_WHEN_AGV_NO_CARGO);
                return new(false, res.Result.Description_En);
            }
            else if ((firstAction == ACTION_TYPE.Unload || firstAction == ACTION_TYPE.Carry) && agv.CurrentCarrierID != "")
            {
                var alarm_code = firstAction == ACTION_TYPE.Unload ? ALARMS.TaskIsNotTransferButAGVCarrierExist : ALARMS.CANNOT_DISPATCH_CARRY_TASK_WHEN_AGV_HAS_CARGO;
                AlarmManagerCenter.AddAlarmAsync(alarm_code);
                return new(false, alarm_code.ToString());
            }

            Task.Factory.StartNew(async () =>
            {
                try
                {
                    script.StopFlag = false;
                    script.finish_num = 0;
                    script.state = "Running";
                    UpdateScriptState(script);
                    RunningScriptList.Add(script);
                    //等待AGV可做任務
                    var agvstates = GetAGVState();
                    Logger logger = LogManager.GetCurrentClassLogger();
                    logger.Info($"Hot Run Start. {script.scriptID}({script.comment})");
                    while (agvstates.OnlineStatus == clsEnums.ONLINE_STATE.OFFLINE || !agvstates.Connected || agvstates.MainStatus == clsEnums.MAIN_STATUS.DOWN || agvstates.MainStatus == clsEnums.MAIN_STATUS.RUN)
                    {
                        await Task.Delay(100);
                        if (script.StopFlag || script.cancellationTokenSource.IsCancellationRequested)
                        {
                            script.state = "IDLE";
                            throw new TaskCanceledException();
                        }
                        agvstates = GetAGVState();
                    }

                    while (script.finish_num != script.loop_num)
                    {
                        await Task.Delay(1);
                        if (script.StopFlag || script.cancellationTokenSource.IsCancellationRequested)
                        {
                            script.state = "IDLE";
                            return;
                        }
                        if (agv != null)
                        {
                            foreach (HotRunAction _action in script.actions)
                            {
                                script.RunningAction = _action;

                                var TaskName = $"HR_{_action.action.ToUpper()}_{DateTime.Now.ToString("yMdHHmmss")}";
                                await TaskManager.AddTask(new clsTaskDto
                                {
                                    Action = GetActionByActionName(_action.action),
                                    From_Station = _action.source_tag.ToString(),
                                    To_Station = _action.action == "measure" ? _action.destine_name : _action.destine_tag.ToString(),
                                    From_Slot = _action.source_slot.ToString(),
                                    To_Slot = _action.destine_slot.ToString(),
                                    DispatcherName = "Hot_Run",
                                    Carrier_ID = _action.cst_id,
                                    TaskName = TaskName,
                                    DesignatedAGVName = script.agv_name,
                                    bypass_eq_status_check = true,
                                });
                                script.state = "Running";
                                bool WaitTaskExecuting(string TaskName)
                                {
                                    return DatabaseCaches.TaskCaches.RunningTasks.Any(tk => tk.TaskName == TaskName);
                                }
                                script.UpdateRealTimeMessage($"等待任務開始");
                                while (!WaitTaskExecuting(TaskName))
                                {
                                    if (script.StopFlag || script.cancellationTokenSource.IsCancellationRequested)
                                    {
                                        script.state = "IDLE";
                                        throw new TaskCanceledException();
                                    }
                                    await Task.Delay(500);
                                }

                                script.UpdateRealTimeMessage($"等待任務結束");
                                while (WaitTaskExecuting(TaskName))
                                {
                                    if (script.StopFlag || script.cancellationTokenSource.IsCancellationRequested)
                                    {
                                        script.state = "IDLE";
                                        throw new TaskCanceledException();
                                    }
                                    await Task.Delay(500);
                                }

                                UpdateScriptState(script);
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
                }
                catch (TaskCanceledException ex)
                {
                    await NotifyServiceHelper.WARNING($"User Cancel Hot Run Script-{script.scriptID}({script.comment})");
                    script.state = "IDLE";
                    script.UpdateRealTimeMessage("已取消工作", false, false);
                    UpdateScriptState(script);
                }
                catch (Exception ex)
                {
                    script.state = "IDLE";
                    UpdateScriptState(script);
                }


            });
            return (true, "");
        }

        private static (bool, string) StartRandomCarryHotRun(HotRunScript script)
        {
            RandomCarryHotRun randomCarryHotRun = new RandomCarryHotRun(script);
            RunningScriptList.Add(script);
            randomCarryHotRun.StartAsync();
            return (true, "");
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
            if (action == "exchange_battery" || action == "exchangebattery")
                return ACTION_TYPE.ExchangeBattery;
            else
                return ACTION_TYPE.None;
        }

        private static void UpdateScriptState(HotRunScript script)
        {
            HotRunScript? _script = HotRunScripts.FirstOrDefault(s => s.scriptID == script.scriptID);
            _script.finish_num = script.finish_num;
            _script.state = script.state;

        }


    }
}
