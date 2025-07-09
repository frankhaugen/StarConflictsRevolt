using StarConflictsRevolt.Server.Core.Models;
using StarConflictsRevolt.Server.Eventing;

namespace StarConflictsRevolt.Server.Core;

public abstract record PlayerController
{
    public Guid PlayerId { get; init; }
    
    public abstract List<StarConflictsRevolt.Server.Eventing.IGameEvent> GenerateCommands(World world);
}