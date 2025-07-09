using StarConflictsRevolt.Server.Core.Models;
using StarConflictsRevolt.Server.Eventing;

namespace StarConflictsRevolt.Server.Core;

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