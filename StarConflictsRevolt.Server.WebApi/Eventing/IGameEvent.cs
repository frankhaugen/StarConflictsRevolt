using StarConflictsRevolt.Server.WebApi.Models;

namespace StarConflictsRevolt.Server.WebApi.Eventing;

public interface IGameEvent
{
    void ApplyTo(World world, ILogger logger);
}

// --- Concrete Game Events ---