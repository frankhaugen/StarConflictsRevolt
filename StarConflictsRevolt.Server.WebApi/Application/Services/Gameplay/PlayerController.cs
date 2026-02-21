using StarConflictsRevolt.Server.WebApi.Core.Domain.Commands;
using WorldState = StarConflictsRevolt.Server.WebApi.Core.Domain.World.World;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;

public class PlayerController
{
    public Guid PlayerId { get; init; }
    public string Name { get; init; } = "Unknown Player";
    public IAiStrategy? AiStrategy { get; set; }

    public virtual List<IGameCommand> GenerateCommands(WorldState world, long clientTick)
    {
        if (AiStrategy != null)
            return AiStrategy.GenerateCommands(PlayerId, world, clientTick, null!);
        return new List<IGameCommand>();
    }
}