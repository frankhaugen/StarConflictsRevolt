using StarConflictsRevolt.Server.WebApi.Core.Domain.Gameplay;

namespace StarConflictsRevolt.Server.WebApi.Infrastructure.Datastore;

// Mostly mapping extensions for Models into Entities
public static class ModelExtensions
{
    public static Fleet ToEntity(this Core.Domain.Fleets.Fleet model)
    {
        return new Fleet
        {
            Id = model.Id,
            Name = model.Name,
            Ships = model.Ships.Select(s => ToEntity(s)).ToList()
        };
    }

    public static Ship ToEntity(this Core.Domain.Fleets.Ship model)
    {
        return new Ship
        {
            Id = model.Id,
            Model = model.Model,
            IsUnderConstruction = model.IsUnderConstruction
        };
    }

    public static Planet ToEntity(this Core.Domain.Planets.Planet model)
    {
        // Only map persistent properties; Fleets and Structures are not persisted directly
        return new Planet
        {
            Id = model.Id,
            Name = model.Name,
            Radius = model.Radius,
            Mass = model.Mass,
            RotationSpeed = model.RotationSpeed,
            OrbitSpeed = model.OrbitSpeed,
            DistanceFromSun = model.DistanceFromSun
        };
    }

    public static StarSystem ToEntity(this Core.Domain.Stars.StarSystem model)
    {
        return new StarSystem
        {
            Id = model.Id,
            Name = model.Name,
            Coordinates = model.Coordinates,
            Planets = model.Planets.Select(p => ToEntity(p)).ToList()
        };
    }

    public static Session ToEntity(this Core.Domain.Sessions.Session model)
    {
        return new Session
        {
            Id = model.Id,
            SessionName = model.SessionName,
            Created = model.Created,
            IsActive = model.IsActive,
            Ended = model.Ended
        };
    }
}