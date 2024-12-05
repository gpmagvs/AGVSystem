using AGVSystem.Service;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

public class FrontEndDataHub : Hub
{

    public static ConcurrentDictionary<string, IClientProxy> connectedClients = new ConcurrentDictionary<string, IClientProxy>();

    public override async Task OnConnectedAsync()
    {
        connectedClients.TryAdd(Context.ConnectionId, Clients.User(Context.ConnectionId));
        await Clients.All.SendAsync("ReceiveData", "VMS", FrontEndDataBrocastService._previousData);
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
}