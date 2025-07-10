using StarConflictsRevolt.Server.WebApi.Enums;
using StarConflictsRevolt.Server.WebApi.Eventing;
using StarConflictsRevolt.Server.WebApi.Models;

namespace StarConflictsRevolt.Server.WebApi.Services;

public interface IAiStrategy
{
    List<IGameEvent> GenerateCommands(Guid playerId, World world, ILogger logger);
}