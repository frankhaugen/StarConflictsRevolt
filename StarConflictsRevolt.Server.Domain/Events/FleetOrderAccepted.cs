using StarConflictsRevolt.Server.EventStorage.Abstractions;
using StarConflictsRevolt.Server.Domain.Enums;
using StarConflictsRevolt.Server.Domain.Fleets;
using StarConflictsRevolt.Server.Domain.Planets;
using WorldState = StarConflictsRevolt.Server.Domain.World.World;

namespace StarConflictsRevolt.Server.Domain.Events;

/// <summary>
/// Fact: a fleet move order was accepted; fleet is in transit until EtaTick.
/// </summary>
public sealed record FleetOrderAccepted(long Tick, Guid FleetId, Guid FromPlanetId, Guid ToPlanetId, long EtaTick) : IGameEvent
{
    public void ApplyTo(object world, ILogger logger)
    {
        var w = (WorldState)world;
        Fleet? fleet = null;
        Planet? fromPlanet = null;
        Planet? toPlanet = null;

        foreach (var system in w.Galaxy.StarSystems)
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
