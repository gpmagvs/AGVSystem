using AGVSystem.Service;
using Microsoft.AspNetCore.SignalR;

public class FrontEndDataHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await Clients.All.SendAsync("ReceiveData", "VMS", FrontEndDataBrocastService._previousData);
        await base.OnConnectedAsync();

    }

    public async Task SendMessage(string user, string message)
    {
        if(message== "fetch-data")
        {
            await Clients.All.SendAsync("ReceiveData", "VMS", FrontEndDataBrocastService._previousData);
        }
        //return Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}