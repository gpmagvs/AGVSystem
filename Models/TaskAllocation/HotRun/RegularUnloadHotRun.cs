
using EquipmentManagment.Manager;
using System.Collections.Concurrent;

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
            foreach (KeyValuePair<string, EquipmentManagment.Emu.clsDIOModuleEmu> item in StaEQPEmulatorsManagager.EqEmulators)
            {
                item.Value.SetStatusBUSY();
            }
        }
        private void RaiseLoadRequests()
        {
            IEnumerable<EquipmentManagment.Emu.clsDIOModuleEmu> alwaysLoadEqEmus = StaEQPEmulatorsManagager.EqEmulators.Values.Where(emu => script.RegularLoadSettings.LoadRequestAlwaysOnEqNames.Contains(emu.options.Name));
            foreach (var item in alwaysLoadEqEmus)
            {
                item.SetStatusLoadable();
            }
        }

        private ConcurrentDictionary<string, ManualResetEvent> UnloadFinishWaitSignals = new ConcurrentDictionary<string, ManualResetEvent>();

        protected override async Task HotRun()
        {
            //return base.HotRun();

            foreach (RegularUnloadConfiguration.UnloadRequestEQSettings unloader in script.RegularLoadSettings.UnloadRequestsSettings)
            {
                Task.Run(async () =>
                {
                    ManualResetEvent _waitUnloadFinishResetEvent = await _UnloaderRegularProcess(unloader);
                    UnloadFinishWaitSignals.TryAdd(unloader.EqName, _waitUnloadFinishResetEvent);
                });
            }

            _waitDoneResetEvent.WaitOne();

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
                    await Task.Delay(TimeSpan.FromSeconds(unloader.UnloadRequestInterval));
                }
            });
            return _waitUnloadFinishResetEvent;
        }

        internal void SetUnloadEqAsBusy(string eQName)
        {
            if (UnloadFinishWaitSignals.TryGetValue(eQName, out ManualResetEvent? pauseSignal))
            {
                pauseSignal.Set();
            }
        }
    }
}
