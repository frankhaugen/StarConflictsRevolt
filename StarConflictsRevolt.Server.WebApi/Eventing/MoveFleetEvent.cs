namespace StarConflictsRevolt.Server.WebApi.Eventing;

public record MoveFleetEvent(Guid PlayerId, Guid FleetId, Guid FromPlanetId, Guid ToPlanetId) : IGameEvent
{
    public void ApplyTo(Models.World world, Microsoft.Extensions.Logging.ILogger logger)
    {
        foreach (var system in world.Galaxy.StarSystems)
        {
            var fromPlanet = system.Planets.FirstOrDefault(p => p.Id == FromPlanetId);
            var toPlanet = system.Planets.FirstOrDefault(p => p.Id == ToPlanetId);
            if (fromPlanet != null && toPlanet != null)
            {
                var fleet = fromPlanet.Fleets.FirstOrDefault(f => f.Id == FleetId);
                if (fleet != null)
                {
                    fromPlanet.Fleets.Remove(fleet);
                    fleet = fleet with { LocationPlanetId = toPlanet.Id };
                    toPlanet.Fleets.Add(fleet);
                    logger.LogInformation("Moved fleet {FleetId} from planet {FromPlanet} to planet {ToPlanet}", 
                        FleetId, FromPlanetId, ToPlanetId);
                }
            }
        }
    }
}