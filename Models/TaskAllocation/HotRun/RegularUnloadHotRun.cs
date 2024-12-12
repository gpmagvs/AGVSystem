
using EquipmentManagment.Manager;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Xml.Linq;

namespace AGVSystem.Models.TaskAllocation.HotRun
{
    public class RegularUnloadHotRun : RandomCarryHotRun
    {
        public RegularUnloadHotRun(HotRunScript script) : base(script)
        {
            script.OnScriptStopRequest += Script_OnScriptStopRequest;
        }

        private void Script_OnScriptStopRequest(object? sender, EventArgs e)
        {
            SetAllEQPNoAnyRequest();
            script.OnScriptStopRequest -= Script_OnScriptStopRequest;
            _waitDoneResetEvent.Set();
        }

        ManualResetEvent _waitDoneResetEvent = new ManualResetEvent(false);
        public override async Task StartAsync()
        {
            SetAllEQPNoAnyRequest();
            await Task.Delay(1000);
            RaiseLoadRequests();
            await base.StartAsync();
        }


        private void SetAllEQPNoAnyRequest()
        {
            foreach (KeyValuePair<string, EquipmentManagment.Emu.EQEmulatorBase> item in StaEQPEmulatorsManagager.EqEmulators)
            {
                item.Value.SetStatusBUSY();
            }
        }
        private void RaiseLoadRequests()
        {
            IEnumerable<EquipmentManagment.Emu.EQEmulatorBase> alwaysLoadEqEmus = StaEQPEmulatorsManagager.EqEmulators.Values.Where(emu => script.RegularLoadSettings.LoadRequestAlwaysOnEqNames.Contains(emu.options.Name));
            foreach (var item in alwaysLoadEqEmus)
            {
                item.SetStatusLoadable();
            }
        }

        public class UnloaderState
        {
            public ManualResetEvent waitSigals = new ManualResetEvent(false);
            internal DateTime nextUnloadTime = DateTime.MinValue;
        }

        private ConcurrentDictionary<string, UnloaderState> UnloaderStatesStore = new ConcurrentDictionary<string, UnloaderState>();

        public IHubContext<FrontEndDataHub> FrontendHub { get; internal set; }

        protected override async Task HotRun()
        {
            //return base.HotRun();

            foreach (RegularUnloadConfiguration.UnloadRequestEQSettings unloader in script.RegularLoadSettings.UnloadRequestsSettings)
            {
                Task.Run(async () =>
                {
                    ManualResetEvent _waitUnloadFinishResetEvent = await _UnloaderRegularProcess(unloader);
                    UnloaderStatesStore.TryAdd(unloader.EqName, new UnloaderState()
                    {
                        waitSigals = _waitUnloadFinishResetEvent
                    });
                });
            }
            DataPushOutUseFrontendHub();
            _waitDoneResetEvent.WaitOne();

        }

        private async Task DataPushOutUseFrontendHub()
        {
            while (!script.StopFlag)
            {
                await Task.Delay(1000);
                if (FrontendHub != null)
                {
                    try
                    {
                        var unloaderStates = UnloaderStatesStore.OrderBy(ke => ke.Key).ToDictionary(keypair => keypair.Key, keypair => new
                        {
                            nextUnloadTime = keypair.Value.nextUnloadTime,
                            timeRemain = TimeSpan.FromSeconds((keypair.Value.nextUnloadTime - DateTime.Now).Seconds).ToString()
                        });

                        await FrontendHub.Clients.All.SendAsync("RegularHotRunInfo", new
                        {
                            state = "running",
                            scriptID = script.scriptID,
                            unloaderStates = unloaderStates
                        });
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            await FrontendHub.Clients.All.SendAsync("RegularHotRunInfo", new { state = "stopped" });

        }

        private async Task<ManualResetEvent> _UnloaderRegularProcess(RegularUnloadConfiguration.UnloadRequestEQSettings unloader)
        {
            ManualResetEvent _waitUnloadFinishResetEvent = new ManualResetEvent(false);
            var _emu = StaEQPEmulatorsManagager.EqEmulators.Values.FirstOrDefault(emu => emu.options.Name == unloader.EqName);
            if (_emu == null)
                return null;
            _ = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(unloader.DelayTimeWhenScriptStart));
                while (!script.StopFlag)
                {
                    _emu.SetStatusUnloadable();
                    //wait unload finish 
                    _waitUnloadFinishResetEvent.Reset();
                    _waitUnloadFinishResetEvent.WaitOne();
                    _emu.SetStatusBUSY();

                    if (UnloaderStatesStore.TryGetValue(unloader.EqName, out UnloaderState? unloaderState))
                    {
                        DateTime nextUnloadTime = DateTime.Now.AddSeconds(unloader.UnloadRequestInterval);
                        unloaderState.nextUnloadTime = nextUnloadTime;
                    }
                    await Task.Delay(TimeSpan.FromSeconds(unloader.UnloadRequestInterval));
                }
            });
            return _waitUnloadFinishResetEvent;
        }

        internal void SetUnloadEqAsBusy(string eQName)
        {
            if (UnloaderStatesStore.TryGetValue(eQName, out UnloaderState? underState))
            {
                underState?.waitSigals.Set();
            }
        }
    }
}
