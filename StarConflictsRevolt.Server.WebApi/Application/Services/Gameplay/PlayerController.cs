using StarConflictsRevolt.Server.WebApi.Core.Domain.Events;
using StarConflictsRevolt.Server.WebApi.Core.Domain.World;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;

public class PlayerController
{
    public Guid PlayerId { get; init; }
    public string Name { get; init; } = "Unknown Player";
    public IAiStrategy? AiStrategy { get; set; }

    public virtual List<IGameEvent> GenerateCommands(World world)
    {
        if (AiStrategy != null)
            return AiStrategy.GenerateCommands(PlayerId, world, null!); // logger should be injected if needed
        return new List<IGameEvent>();
    }
}