﻿using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Server.WebApi.Models;

namespace StarConflictsRevolt.Server.WebApi.Services;

public static class WorldMappingExtensions
{
    public static WorldDto ToDto(this World world)
    {
        return new WorldDto(world.Id, world.Galaxy.ToDto());
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
        return new PlanetDto(planet.Id, planet.Name, planet.Radius, planet.Mass, planet.RotationSpeed, planet.OrbitSpeed, planet.DistanceFromSun);
    }
}