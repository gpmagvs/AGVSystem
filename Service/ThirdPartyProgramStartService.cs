
using AGVSystemCommonNet6.Sys;
using System.Diagnostics;

namespace AGVSystem.Service
{
    public class ThirdPartyProgramStartService : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _StartCIM();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
        }

        private void _StartCIM()
        {
            try
            {
                string cimApp = EnvironmentVariables.GetUserVariable("GPM_CIM_Path");
                if (string.IsNullOrEmpty(cimApp))
                    return;
                string appName = Path.GetFileNameWithoutExtension(cimApp);
                Process[] pros = Process.GetProcessesByName(appName);

                if (pros.Any())
                    return;
                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    UseShellExecute = false,
                    WorkingDirectory = Path.GetDirectoryName(cimApp),
                    FileName = cimApp,
                    CreateNoWindow = false,
                };
                Process.Start(startInfo);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
