using StarConflictsRevolt.Server.WebApi.Models;

namespace StarConflictsRevolt.Server.WebApi.Datastore.Extensions;

// Mostly mapping to the sibling record objects in the Core project.
public static class EntityExtensions
{
    public static Galaxy ToModel(this Entities.Galaxy galaxy)
    {
        return new Galaxy(galaxy.StarSystems.Select(x => x.ToModel()).ToList())
        {
            Id = galaxy.Id
        };
    }
    
    public static StarSystem ToModel(this Entities.StarSystem starSystem)
    {
        return new StarSystem(starSystem.Id, starSystem.Name, starSystem.Planets.Select(x => x.ToModel()).ToList(), starSystem.Coordinates);
    }
    
    public static Session ToModel(this Entities.Session session)
    {
        return new Session(session.Id, session.SessionName, session.Created, session.IsActive, session.Ended, session.SessionType);
    }
    public static World ToModel(this Entities.World world)
    {
        return new World(world.Id, world.Galaxy.ToModel());
    }
    
    // Basic mapping for simple cases
    public static Planet ToModel(this Entities.Planet planet)
    {
        return new Planet(planet.Name, planet.Radius, planet.Mass, planet.RotationSpeed, planet.OrbitSpeed, planet.DistanceFromSun, new(), new())
        {
            Id = planet.Id
        };
    }
    
    public static Ship ToModel(this Entities.Ship ship)
    {
        return new Ship(ship.Id, ship.Model, ship.IsUnderConstruction);
    }
    
}