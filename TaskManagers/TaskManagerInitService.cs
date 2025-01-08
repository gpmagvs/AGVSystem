
using AGVSystemCommonNet6.DATABASE;

namespace AGVSystem.TaskManagers
{
    public class TaskManagerInitService : IHostedService
    {
        AGVSDbContext dbContext;
        public TaskManagerInitService(IServiceScopeFactory factory)
        {
            dbContext = factory.CreateScope().ServiceProvider.GetRequiredService<AGVSDbContext>();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            TaskManager._dbContext = dbContext;//為TaskManager注入 dbcontext實體.
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
        }
    }
}
