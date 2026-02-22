using StarConflictsRevolt.Server.WebApi.Core.Domain.Enums;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Fleets;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Planets;

namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Events;

/// <summary>
/// Fact: a fleet reached its destination at this tick; finalize location and status.
/// </summary>
public sealed record FleetArrived(long Tick, Guid FleetId, Guid PlanetId) : IGameEvent
{
    public void ApplyTo(World.World world, ILogger logger)
    {
        Fleet? fleet = null;
        Planet? onPlanet = null;

        foreach (var system in world.Galaxy.StarSystems)
        {
            foreach (var planet in system.Planets)
            {
                var f = planet.Fleets.FirstOrDefault(x => x.Id == FleetId);
                if (f != null)
                {
                    fleet = f;
                    onPlanet = planet;
                    break;
                }
            }
            if (fleet != null) break;
        }

        if (fleet == null || onPlanet == null)
        {
            logger.LogWarning("FleetArrived: fleet {FleetId} or planet not found", FleetId);
            return;
        }

        if (fleet.Status != FleetStatus.Moving)
            return;

        var index = onPlanet.Fleets.IndexOf(fleet);
        if (index < 0) return;

        var arrived = fleet with
        {
            LocationPlanetId = PlanetId,
            Status = FleetStatus.Idle,
            DestinationPlanetId = null,
            DepartureTime = null,
            ArrivalTime = null,
            EtaTick = null
        };
        onPlanet.Fleets[index] = arrived;
        logger.LogInformation("Fleet {FleetId} arrived at planet {PlanetId} on tick {Tick}", FleetId, PlanetId, Tick);
    }
}
