using StarConflictsRevolt.Server.Core.Models;

namespace StarConflictsRevolt.Server.Core;

public abstract class PlayerController
{
    public Guid PlayerId { get; init; }
    
    public abstract List<StarConflictsRevolt.Server.Eventing.IGameEvent> GenerateCommands(World world);
}