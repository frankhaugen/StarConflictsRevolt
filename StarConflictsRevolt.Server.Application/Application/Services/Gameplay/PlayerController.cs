using StarConflictsRevolt.Server.Domain;
using StarConflictsRevolt.Server.EventStorage.Abstractions;
using WorldState = StarConflictsRevolt.Server.Domain.World.World;

namespace StarConflictsRevolt.Server.Application.Services.Gameplay;

public class PlayerController : IPlayerController
{
    public Guid PlayerId { get; init; }
    public string Name { get; init; } = "Unknown Player";
    public StarConflictsRevolt.Server.AI.IAiStrategy? AiStrategy { get; set; }

    public virtual List<IGameCommand> GenerateCommands(WorldState world, long clientTick)
    {
        if (AiStrategy != null)
            return AiStrategy.GenerateCommands(PlayerId, world, clientTick, null!);
        return new List<IGameCommand>();
    }
}