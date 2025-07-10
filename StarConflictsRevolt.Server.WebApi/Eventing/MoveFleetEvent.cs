using StarConflictsRevolt.Server.WebApi.Enums;
using StarConflictsRevolt.Server.WebApi.Models;

namespace StarConflictsRevolt.Server.WebApi.Eventing;

public record MoveFleetEvent(Guid PlayerId, Guid FleetId, Guid FromPlanetId, Guid ToPlanetId) : IGameEvent
{
    public void ApplyTo(World world, Microsoft.Extensions.Logging.ILogger logger)
    {
        // Find the fleet and validate the move
        Fleet? fleet = null;
        Planet? fromPlanet = null;
        Planet? toPlanet = null;

        foreach (var system in world.Galaxy.StarSystems)
        {
            // Find source planet and fleet
            var sourcePlanet = system.Planets.FirstOrDefault(p => p.Id == FromPlanetId);
            if (sourcePlanet != null)
            {
                fleet = sourcePlanet.Fleets.FirstOrDefault(f => f.Id == FleetId);
                fromPlanet = sourcePlanet;
            }

            // Find destination planet
            var destPlanet = system.Planets.FirstOrDefault(p => p.Id == ToPlanetId);
            if (destPlanet != null)
            {
                toPlanet = destPlanet;
            }
        }

        if (fleet == null)
        {
            logger.LogWarning("Fleet {FleetId} not found on planet {FromPlanetId}", FleetId, FromPlanetId);
            return;
        }

        if (fromPlanet == null || toPlanet == null)
        {
            logger.LogWarning("Source or destination planet not found for fleet movement");
            return;
        }

        // Validate ownership
        if (fleet.OwnerId != PlayerId)
        {
            logger.LogWarning("Player {PlayerId} does not own fleet {FleetId}", PlayerId, FleetId);
            return;
        }

        // Calculate travel time based on distance and fleet speed
        var distance = CalculateDistance(fromPlanet, toPlanet);
        var travelTime = CalculateTravelTime(distance, fleet.Ships);
        var departureTime = DateTime.UtcNow;
        var arrivalTime = departureTime.AddSeconds(travelTime);

        // Remove fleet from source planet
        fromPlanet.Fleets.Remove(fleet);

        // Create updated fleet with movement state
        var updatedFleet = fleet with
        {
            LocationPlanetId = null, // Fleet is in transit
            Status = FleetStatus.Moving,
            DestinationPlanetId = ToPlanetId,
            DepartureTime = departureTime,
            ArrivalTime = arrivalTime
        };

        // Add fleet to destination planet (it will arrive when the time comes)
        toPlanet.Fleets.Add(updatedFleet);

        logger.LogInformation("Fleet {FleetId} started moving from {FromPlanet} to {ToPlanet}, arrival in {TravelTime}s", 
            FleetId, FromPlanetId, ToPlanetId, travelTime);
    }

    private static double CalculateDistance(Planet fromPlanet, Planet toPlanet)
    {
        // Simple distance calculation based on orbital positions
        var distanceFromSun = Math.Abs(fromPlanet.DistanceFromSun - toPlanet.DistanceFromSun);
        return distanceFromSun * 100; // Scale factor for game balance
    }

    private static double CalculateTravelTime(double distance, List<Ship> ships)
    {
        if (!ships.Any())
            return 60; // Default 1 minute for empty fleets

        // Calculate average speed of the fleet
        var avgSpeed = ships.Average(s => s.Speed);
        var travelTime = distance / avgSpeed;
        
        // Clamp between 30 seconds and 5 minutes for game balance
        return Math.Max(30, Math.Min(300, travelTime));
    }
}