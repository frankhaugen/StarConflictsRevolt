using Microsoft.AspNetCore.SignalR;
using StarConflictsRevolt.Server.Domain.Commands;
using StarConflictsRevolt.Server.Simulation.Engine;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;

/// <summary>
/// SignalR hub: methods map 1:1 to commands. Client sends command; server enqueues via ICommandIngress.
/// </summary>
public sealed class GameHub : Hub
{
    private readonly ICommandIngress _ingress;

    public GameHub(ICommandIngress ingress)
    {
        _ingress = ingress;
    }

    public Task MoveFleet(Guid sessionId, Guid fleetId, Guid toSystemId, long clientTick)
    {
        var playerId = ResolvePlayerId();
        return _ingress.SubmitAsync(new GameSessionId(sessionId), new MoveFleet(playerId, clientTick, fleetId, toSystemId), Context.ConnectionAborted).AsTask();
    }

    public Task QueueBuild(Guid sessionId, Guid systemId, string design, int count, long clientTick)
    {
        var playerId = ResolvePlayerId();
        return _ingress.SubmitAsync(new GameSessionId(sessionId), new QueueBuild(playerId, clientTick, systemId, design, count), Context.ConnectionAborted).AsTask();
    }

    public Task StartRally(Guid sessionId, Guid regionId, long clientTick)
    {
        var playerId = ResolvePlayerId();
        return _ingress.SubmitAsync(new GameSessionId(sessionId), new StartRally(playerId, clientTick, regionId), Context.ConnectionAborted).AsTask();
    }

    public Task StartMartialLaw(Guid sessionId, Guid systemId, long clientTick)
    {
        var playerId = ResolvePlayerId();
        return _ingress.SubmitAsync(new GameSessionId(sessionId), new StartMartialLaw(playerId, clientTick, systemId), Context.ConnectionAborted).AsTask();
    }

    private Guid ResolvePlayerId()
    {
        var id = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(id) && Guid.TryParse(id, out var guid))
            return guid;
        return Guid.Empty;
    }
}
