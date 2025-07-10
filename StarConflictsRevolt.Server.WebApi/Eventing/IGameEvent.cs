namespace StarConflictsRevolt.Server.WebApi.Eventing;

public interface IGameEvent 
{
    void ApplyTo(Models.World world, Microsoft.Extensions.Logging.ILogger logger);
}

// --- Concrete Game Events ---