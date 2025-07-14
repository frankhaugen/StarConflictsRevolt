using Microsoft.AspNetCore.SignalR;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Server.WebApi.Core.Domain.World;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;

public class WorldHub : Hub
{
    public async Task JoinWorld(string worldId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, worldId);
        // Retrieve the world for this session
        if (Guid.TryParse(worldId, out var sessionId))
        {
            var aggregateManager = Context.GetHttpContext()?.RequestServices.GetService(typeof(SessionAggregateManager)) as SessionAggregateManager;
            var worldFactory = Context.GetHttpContext()?.RequestServices.GetService(typeof(WorldFactory)) as WorldFactory;
            if (aggregateManager != null && worldFactory != null)
            {
                var aggregate = aggregateManager.GetOrCreateAggregate(sessionId, worldFactory.CreateDefaultWorld());
                var world = aggregate.World;
                await Clients.Caller.SendAsync("FullWorld", world.ToDto());
            }
        }
    }

    // Server can call this to send a full world state
    public async Task SendFullWorldAsync(World world)
    {
        await Clients.All.SendAsync("FullWorld", world.ToDto());
    }

    // Server can call this to send updates
    public async Task SendUpdatesAsync(List<GameObjectUpdate> updates)
    {
        await Clients.All.SendAsync("ReceiveUpdates", updates);
    }
}