using StarConflictsRevolt.Server.EventStorage.Abstractions;
using WorldState = StarConflictsRevolt.Server.Domain.World.World;

namespace StarConflictsRevolt.Server.Domain;

/// <summary>
/// Abstraction for a player (human or AI) that can generate commands from world state.
/// Implemented by the Application layer (e.g. PlayerController, HumanController).
/// </summary>
public interface IPlayerController
{
    Guid PlayerId { get; }
    string Name { get; }
    List<IGameCommand> GenerateCommands(WorldState world, long clientTick);
}
