using Microsoft.AspNetCore.SignalR;
using StarConflictsRevolt.Clients.Models;

namespace StarConflictsRevolt.Server.GameEngine;

public class WorldHub : Hub
{
    public async Task JoinWorld(string worldId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, worldId);
    }

    // Server can call this to send a full world state
    public async Task SendFullWorldAsync(WorldDto world)
    {
        await Clients.All.SendAsync("FullWorld", world);
    }

    // Server can call this to send updates
    public async Task SendUpdatesAsync(List<GameObjectUpdate> updates)
    {
        await Clients.All.SendAsync("ReceiveUpdates", updates);
    }
}