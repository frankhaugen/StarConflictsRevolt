using StarConflictsRevolt.Server.WebApi.Core.Domain.Commands;
using WorldState = StarConflictsRevolt.Server.WebApi.Core.Domain.World.World;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;

public interface IAiStrategy
{
    List<IGameCommand> GenerateCommands(Guid playerId, WorldState world, long clientTick, ILogger logger);
}