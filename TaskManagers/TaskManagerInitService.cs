
using AGVSystemCommonNet6.DATABASE;

namespace AGVSystem.TaskManagers
{
    public class TaskManagerInitService : IHostedService
    {
        DBDataService _dbDataService;
        public TaskManagerInitService(DBDataService dbDataService)
        {
            _dbDataService = dbDataService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            TaskManager.dbDataService = _dbDataService;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
        }
    }
}
