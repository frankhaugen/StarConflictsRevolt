using StarConflictsRevolt.Server.Core.Models;

namespace StarConflictsRevolt.Server.Datastore.Extensions;

// Mostly mapping to the sibling record objects in the Core project.
public static class EntityExtensions
{
    public static Galaxy ToModel(this Entities.Galaxy galaxy)
    {
        return new Galaxy(galaxy.Id, galaxy.StarSystems.Select(x => x.ToModel()));
    }
    
    public static StarSystem ToModel(this Entities.StarSystem starSystem)
    {
        return new StarSystem(starSystem.Id, starSystem.Name, starSystem.Planets.Select(x => x.ToModel()), starSystem.Coordinates);
    }
    
    public static Session ToModel(this Entities.Session session)
    {
        return new Session(session.Id, session.SessionName, session.Created, session.IsActive, session.Ended);
    }
    public static World ToModel(this Entities.World world)
    {
        return new World(world.Id, world.Galaxy.ToModel());
    }
    
    public static Planet ToModel(this Entities.Planet planet)
    {
        return new Planet(planet.Id, planet.Name, planet.Radius, planet.Mass, planet.RotationSpeed, planet.OrbitSpeed, planet.DistanceFromSun);
    }
    
    public static Ship ToModel(this Entities.Ship ship)
    {
        return new Ship(ship.Id, ship.Model, ship.IsUnderConstruction);
    }
    
}