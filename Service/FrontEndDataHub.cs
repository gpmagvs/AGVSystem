﻿using AGVSystem.Service;
using Microsoft.AspNetCore.SignalR;

public class FrontEndDataHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await Clients.All.SendAsync("ReceiveData", "VMS", FrontEndDataBrocastService._previousData);
        await base.OnConnectedAsync();
    }
}