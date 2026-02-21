using StarConflictsRevolt.Server.WebApi.Core.Domain.Commands;
using WorldState = StarConflictsRevolt.Server.WebApi.Core.Domain.World.World;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;

internal class HumanController : PlayerController
{
    public string ConnectionId { get; set; } = string.Empty;

    public override List<IGameCommand> GenerateCommands(WorldState world, long clientTick)
    {
        return new List<IGameCommand>();
    }
}