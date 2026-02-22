using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Server.Domain.Enums;
using StarConflictsRevolt.Server.Domain.Fleets;
using StarConflictsRevolt.Server.Domain.Galaxies;
using StarConflictsRevolt.Server.Domain.Planets;
using StarConflictsRevolt.Server.Domain.Stars;
using StarConflictsRevolt.Server.Domain.World;

namespace StarConflictsRevolt.Server.Application.Services.Gameplay;

public static class WorldMappingExtensions
{
    public static WorldDto ToDto(this World world, Guid? playerId = null)
    {
        GameStateInfoDto? playerStateDto = null;
        if (playerId.HasValue && world.PlayerStates.TryGetValue(playerId.Value, out var playerState))
        {
            playerStateDto = new GameStateInfoDto
            {
                PlayerId = playerState.PlayerId,
                PlayerName = world.Players.FirstOrDefault(p => p.PlayerId == playerState.PlayerId)?.Name ?? "Unknown",
                Credits = playerState.Credits,
                Materials = playerState.Materials,
                Fuel = playerState.Fuel
            };
        }
        return new WorldDto(world.Id, world.Galaxy.ToDto(), playerStateDto);
    }

    public static GalaxyDto ToDto(this Galaxy galaxy)
    {
        return new GalaxyDto(Guid.NewGuid(), galaxy.StarSystems.Select(s => s.ToDto()));
    }

    public static StarSystemDto ToDto(this StarSystem system)
    {
        return new StarSystemDto(system.Id, system.Name, system.Planets.Select(p => p.ToDto()), system.Coordinates);
    }

    public static PlanetDto ToDto(this Planet planet)
    {
        var fleets = (planet.Fleets ?? new List<Fleet>()).Select(f => f.ToDto()).ToList();
        return new PlanetDto(planet.Id, planet.Name, planet.Radius, planet.Mass, planet.RotationSpeed, planet.OrbitSpeed, planet.DistanceFromSun, fleets);
    }

    public static FleetDto ToDto(this Fleet fleet)
    {
        var shipCount = fleet.Ships?.Count ?? 0;
        return new FleetDto(
            fleet.Id,
            fleet.Name,
            shipCount,
            fleet.LocationPlanetId,
            fleet.OwnerId,
            fleet.Status.ToString(),
            fleet.DestinationPlanetId,
            fleet.ArrivalTime
        );
    }
}