namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Events;

public interface IGameEvent
{
    void ApplyTo(World.World world, ILogger logger);
}

// --- Concrete Game Events ---