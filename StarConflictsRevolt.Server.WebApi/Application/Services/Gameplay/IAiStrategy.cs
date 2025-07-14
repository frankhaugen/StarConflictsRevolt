using StarConflictsRevolt.Server.WebApi.Core.Domain.Events;
using StarConflictsRevolt.Server.WebApi.Core.Domain.World;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;

public interface IAiStrategy
{
    List<IGameEvent> GenerateCommands(Guid playerId, World world, ILogger logger);
}