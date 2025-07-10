namespace StarConflictsRevolt.Server.WebApi.Eventing;

public record AttackEvent(Guid PlayerId, Guid AttackerFleetId, Guid DefenderFleetId, Guid LocationPlanetId) : IGameEvent
{
    public void ApplyTo(Models.World world, Microsoft.Extensions.Logging.ILogger logger)
    {
        foreach (var system in world.Galaxy.StarSystems)
        {
            var planet = system.Planets.FirstOrDefault(p => p.Id == LocationPlanetId);
            if (planet != null)
            {
                var defender = planet.Fleets.FirstOrDefault(f => f.Id == DefenderFleetId);
                if (defender != null)
                {
                    planet.Fleets.Remove(defender);
                    logger.LogInformation("Removed defender fleet {DefenderFleetId} from planet {PlanetId}", DefenderFleetId, LocationPlanetId);
                }
            }
        }
    }
}