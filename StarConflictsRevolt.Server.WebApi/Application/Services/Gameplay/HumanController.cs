using StarConflictsRevolt.Server.WebApi.Core.Domain.Events;
using StarConflictsRevolt.Server.WebApi.Core.Domain.World;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;

internal class HumanController : PlayerController
{
    public string ConnectionId { get; set; }

    /// <inheritdoc />
    public override List<IGameEvent> GenerateCommands(World world)
    {
        // Human players do not generate commands automatically.
        // This method can be overridden to handle specific game logic if needed.
        return new List<IGameEvent>();
    }
}