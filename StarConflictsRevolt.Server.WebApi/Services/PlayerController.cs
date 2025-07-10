using StarConflictsRevolt.Server.WebApi.Eventing;
using StarConflictsRevolt.Server.WebApi.Models;

namespace StarConflictsRevolt.Server.WebApi.Services;

public abstract class PlayerController
{
    public Guid PlayerId { get; init; }
    
    public abstract List<IGameEvent> GenerateCommands(World world);
}