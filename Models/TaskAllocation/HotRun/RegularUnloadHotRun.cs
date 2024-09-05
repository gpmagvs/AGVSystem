
using EquipmentManagment.Manager;

namespace AGVSystem.Models.TaskAllocation.HotRun
{
    public class RegularUnloadHotRun : RandomCarryHotRun
    {
        public RegularUnloadHotRun(HotRunScript script) : base(script)
        {
        }


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


        protected override Task HotRun()
        {
            return base.HotRun();
        }
    }
}
