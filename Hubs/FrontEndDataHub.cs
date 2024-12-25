using AGVSystem.Service;
using AGVSystem.ViewModel;
using AGVSystemCommonNet6.DATABASE;
using Microsoft.AspNetCore.SignalR;
using NuGet.Protocol.Plugins;
using System.Collections.Concurrent;

public class FrontEndDataHub : Hub
{
    AGVSDbContext dbContext;
    public FrontEndDataHub(AGVSDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public static ConcurrentDictionary<string, IClientProxy> connectedClients = new ConcurrentDictionary<string, IClientProxy>();

    public override async Task OnConnectedAsync()
    {
        connectedClients.TryAdd(Context.ConnectionId, Clients.User(Context.ConnectionId));
        SendDataOnConnected();
        await base.OnConnectedAsync();

    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        string _id = Context.ConnectionId;
        connectedClients.TryRemove(_id, out _);
        return base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(string user, string message)
    {
        if (message == "fetch-data")
        {
            await Clients.All.SendAsync("ReceiveData", "VMS", FrontEndDataBrocastService._previousData);
        }
        //return Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    private async Task SendDataOnConnected()
    {
        try
        {
            await Clients.Caller.SendAsync("ReceiveData", "VMS", FrontEndDataBrocastService._previousData);
            using (AGVSDatabase dbHelper = new AGVSDatabase())
            {
                if (!AGVSDatabase.CheckPrimaryKeys(dbHelper, out List<string> primaryErrorTables))
                {
                    await Clients.Caller.SendAsync("DatabaseState", new DatabaseState() { state = false, message = $"資料表 [{string.Join(",", primaryErrorTables)}] 需添加主鍵" });
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message + ex.StackTrace);
        }
    }
}