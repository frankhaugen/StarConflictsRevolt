using StarConflictsRevolt.Server.EventStorage.Abstractions;
using StarConflictsRevolt.Server.Domain.Commands;
using StarConflictsRevolt.Server.Domain.Events;
using WorldState = StarConflictsRevolt.Server.Domain.World.World;

namespace StarConflictsRevolt.Server.Domain.Engine;

/// <summary>
/// Validates commands against current world and produces events (facts).
/// </summary>
public interface IGameSim
{
    IReadOnlyList<IGameEvent> Execute(long tick, WorldState world, IGameCommand command);
}
