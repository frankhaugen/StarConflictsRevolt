using StarConflictsRevolt.Server.EventStorage.Abstractions;
using WorldState = StarConflictsRevolt.Server.Domain.World.World;

namespace StarConflictsRevolt.Server.AI;

public interface IAiStrategy
{
    List<IGameCommand> GenerateCommands(Guid playerId, WorldState world, long clientTick, ILogger logger);
}
