using StarConflictsRevolt.Server.WebApi.Core.Domain.Enums;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Fleets;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Planets;

namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Events;

/// <summary>
/// Fact: a fleet move order was accepted; fleet is in transit until EtaTick.
/// </summary>
public sealed record FleetOrderAccepted(long Tick, Guid FleetId, Guid FromPlanetId, Guid ToPlanetId, long EtaTick) : IGameEvent
{
    public void ApplyTo(World.World world, ILogger logger)
    {
        Fleet? fleet = null;
        Planet? fromPlanet = null;
        Planet? toPlanet = null;

        foreach (var system in world.Galaxy.StarSystems)
        {
            var sourcePlanet = system.Planets.FirstOrDefault(p => p.Id == FromPlanetId);
            if (sourcePlanet != null)
            {
                fleet = sourcePlanet.Fleets.FirstOrDefault(f => f.Id == FleetId);
                fromPlanet = sourcePlanet;
            }

            var destPlanet = system.Planets.FirstOrDefault(p => p.Id == ToPlanetId);
            if (destPlanet != null) toPlanet = destPlanet;
        }

        if (fleet == null || fromPlanet == null || toPlanet == null)
        {
            logger.LogWarning("FleetOrderAccepted: fleet or planet not found. FleetId={FleetId}, From={From}, To={To}", FleetId, FromPlanetId, ToPlanetId);
            return;
        }

        fromPlanet.Fleets.Remove(fleet);

        var tickDurationSeconds = 5.0;
        var arrivalTime = DateTime.UtcNow.AddSeconds((EtaTick - Tick) * tickDurationSeconds);

        var updatedFleet = fleet with
        {
            LocationPlanetId = null,
            Status = FleetStatus.Moving,
            DestinationPlanetId = ToPlanetId,
            DepartureTime = DateTime.UtcNow,
            ArrivalTime = arrivalTime,
            EtaTick = EtaTick
        };

        toPlanet.Fleets.Add(updatedFleet);
        logger.LogInformation("Fleet {FleetId} in transit from {From} to {To}, ETA tick {EtaTick}", FleetId, FromPlanetId, ToPlanetId, EtaTick);
    }
}
