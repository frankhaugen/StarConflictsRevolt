using StarConflictsRevolt.Server.WebApi.Eventing;
using StarConflictsRevolt.Server.WebApi.Models;

namespace StarConflictsRevolt.Server.WebApi.Services;

class HumanController: PlayerController
{
    /// <inheritdoc />
    public override List<IGameEvent> GenerateCommands(World world)
    {
        // Human players do not generate commands automatically.
        // This method can be overridden to handle specific game logic if needed.
        return new List<IGameEvent>();
    }

    public string ConnectionId { get; set; }

}