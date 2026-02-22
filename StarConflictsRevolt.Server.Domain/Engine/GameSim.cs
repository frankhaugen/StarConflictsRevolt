using StarConflictsRevolt.Server.EventStorage.Abstractions;
using StarConflictsRevolt.Server.Domain.Commands;
using StarConflictsRevolt.Server.Domain.Events;
using StarConflictsRevolt.Server.Domain.Fleets;
using StarConflictsRevolt.Server.Domain.Planets;
using StarConflictsRevolt.Server.Domain.Stars;
using WorldState = StarConflictsRevolt.Server.Domain.World.World;

namespace StarConflictsRevolt.Server.Domain.Engine;

public sealed class GameSim : IGameSim
{
    private const int TicksPerMove = 6;
    private readonly ILogger<GameSim> _logger;

    public GameSim(ILogger<GameSim> logger)
    {
        _logger = logger;
    }

    public IReadOnlyList<IGameEvent> Execute(long tick, WorldState world, IGameCommand command)
    {
        return command switch
        {
            MoveFleet move => ExecuteMoveFleet(tick, world, move),
            QueueBuild => [new CommandRejected(tick, command.PlayerId, "QueueBuild not yet implemented")],
            StartRally => [new CommandRejected(tick, command.PlayerId, "StartRally not yet implemented")],
            StartMartialLaw => [new CommandRejected(tick, command.PlayerId, "StartMartialLaw not yet implemented")],
            _ => [new CommandRejected(tick, command.PlayerId, $"Unknown command type: {command.GetType().Name}")]
        };
    }

    private IReadOnlyList<IGameEvent> ExecuteMoveFleet(long tick, WorldState world, MoveFleet cmd)
    {
        Fleet? fleet = null;
        Guid? fromPlanetId = null;

        foreach (var system in world.Galaxy.StarSystems)
        {
            foreach (var planet in system.Planets)
            {
                var f = planet.Fleets.FirstOrDefault(x => x.Id == cmd.FleetId);
                if (f != null)
                {
                    fleet = f;
                    fromPlanetId = planet.Id;
                    break;
                }
            }
            if (fleet != null) break;
        }

        if (fleet == null)
            return [new CommandRejected(tick, cmd.PlayerId, $"Fleet {cmd.FleetId} not found")];

        if (fleet.OwnerId != cmd.PlayerId)
            return [new CommandRejected(tick, cmd.PlayerId, $"Player does not own fleet {cmd.FleetId}")];

        if (!fromPlanetId.HasValue)
            return [new CommandRejected(tick, cmd.PlayerId, "Fleet has no location")];

        Guid toPlanetId = ResolveToPlanetId(world, cmd.ToSystemId);
        if (toPlanetId == Guid.Empty)
            return [new CommandRejected(tick, cmd.PlayerId, $"Destination {cmd.ToSystemId} not found")];
        var etaTick = tick + TicksPerMove;
        return [new FleetOrderAccepted(tick, cmd.FleetId, fromPlanetId.Value, toPlanetId, etaTick)];
    }

    private static Guid ResolveToPlanetId(WorldState world, Guid systemOrPlanetId)
    {
        foreach (var system in world.Galaxy.StarSystems)
        {
            if (system.Id == systemOrPlanetId && system.Planets.Count > 0)
                return system.Planets[0].Id;
            var planet = system.Planets.FirstOrDefault(p => p.Id == systemOrPlanetId);
            if (planet != null)
                return planet.Id;
        }
        return Guid.Empty;
    }
}
