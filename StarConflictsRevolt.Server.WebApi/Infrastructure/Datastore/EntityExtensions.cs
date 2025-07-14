using StarConflictsRevolt.Server.WebApi.Core.Domain.Fleets;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Galaxies;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Planets;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Sessions;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Stars;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Structures;
using StarConflictsRevolt.Server.WebApi.Core.Domain.World;

namespace StarConflictsRevolt.Server.WebApi.Infrastructure.Datastore;

// Mostly mapping to the sibling record objects in the Core project.
public static class EntityExtensions
{
    public static Galaxy ToModel(this Core.Domain.Gameplay.Galaxy galaxy)
    {
        return new Galaxy(galaxy.StarSystems.Select(x => x.ToModel()).ToList())
        {
            Id = galaxy.Id
        };
    }

    public static StarSystem ToModel(this Core.Domain.Gameplay.StarSystem starSystem)
    {
        return new StarSystem(starSystem.Id, starSystem.Name, starSystem.Planets.Select(x => x.ToModel()).ToList(), starSystem.Coordinates);
    }

    public static Session ToModel(this Core.Domain.Gameplay.Session session)
    {
        return new Session(session.Id, session.SessionName, session.Created, session.IsActive, session.Ended, session.SessionType);
    }

    public static World ToModel(this Core.Domain.Gameplay.World world)
    {
        return new World(world.Id, world.Galaxy.ToModel());
    }

    // Basic mapping for simple cases
    public static Planet ToModel(this Core.Domain.Gameplay.Planet planet)
    {
        return new Planet(planet.Name, planet.Radius, planet.Mass, planet.RotationSpeed, planet.OrbitSpeed, planet.DistanceFromSun, new List<Fleet>(), new List<Structure>())
        {
            Id = planet.Id
        };
    }

    public static Ship ToModel(this Core.Domain.Gameplay.Ship ship)
    {
        return new Ship(ship.Id, ship.Model, ship.IsUnderConstruction);
    }
}