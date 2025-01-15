
using AGVSystem.Models.EQDevices;
using AGVSystem.Models.Sys;
using EquipmentManagment.MainEquipment;
using EquipmentManagment.Manager;

namespace AGVSystem.Service
{
    public class EQIOStatusMonitorBackgroundService : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(1).ContinueWith(async t =>
            {
                while (true)
                {
                    await Task.Delay(1000);
                    if (SystemModes.RunMode == AGVSystemCommonNet6.AGVDispatch.RunMode.RUN_MODE.MAINTAIN)
                        continue;
                    try
                    {

                        await FixToEQOrReserveSignalKeepOnAbnor();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                }
            });
        }

        private async Task FixToEQOrReserveSignalKeepOnAbnor()
        {
            List<clsEQ> abnormalyEqList = GeToEQOrReserveSignalKeepOnAbnormalyEQList();
            if (!abnormalyEqList.Any())
                return;

            await OFFAbnormalyToEQAndReserveSignal(abnormalyEqList);
        }

        private List<clsEQ> GeToEQOrReserveSignalKeepOnAbnormalyEQList()
        {
            List<clsEQ> signalOnEqList = StaEQPManagager.MainEQList.Where(_eq => _eq.CMD_Reserve_Up || _eq.CMD_Reserve_Low || _eq.To_EQ_Up || _eq.To_EQ_Low).ToList();
            if (!signalOnEqList.Any())
                return new List<clsEQ>();

            return signalOnEqList.Where(_eq => !_eq.IsEQPortAssignToOrder())
                                 .ToList();
        }

        private async Task OFFAbnormalyToEQAndReserveSignal(List<clsEQ> eqList)
        {
            List<Task> tasks = new List<Task>();

            foreach (var _eq in eqList)
            {
                tasks.Add(Task.Factory.StartNew(async () =>
                {
                    await _eq.CancelReserve();
                    await _eq.CancelToEQUpAndLow();

                    using CancellationTokenSource waitCts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                    while (_eq.CMD_Reserve_Up || _eq.CMD_Reserve_Low || _eq.To_EQ_Up || _eq.To_EQ_Low)
                    {
                        await Task.Delay(100);
                        if (waitCts.IsCancellationRequested)
                        {
                            break;
                            Console.WriteLine($"OFFAbnormalyToEQAndReserveSignal Fail");
                        }
                    }
                    Console.WriteLine($"OFFAbnormalyToEQAndReserveSignal Completed!");

                }));
            }

            await Task.WhenAll(tasks);
        }

    }
}
