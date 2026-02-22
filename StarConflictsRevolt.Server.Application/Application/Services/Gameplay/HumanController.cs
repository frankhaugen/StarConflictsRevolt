using StarConflictsRevolt.Server.EventStorage.Abstractions;
using WorldState = StarConflictsRevolt.Server.Domain.World.World;

namespace StarConflictsRevolt.Server.Application.Services.Gameplay;

internal class HumanController : PlayerController
{
    public string ConnectionId { get; set; } = string.Empty;

    public override List<IGameCommand> GenerateCommands(WorldState world, long clientTick)
    {
        return new List<IGameCommand>();
    }
}