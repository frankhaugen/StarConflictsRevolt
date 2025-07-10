using StarConflictsRevolt.Server.WebApi.Datastore.Entities;

namespace StarConflictsRevolt.Server.WebApi.Datastore.Extensions
{
    // Mostly mapping extensions for Models into Entities
    public static class ModelExtensions
    {
        public static Fleet ToEntity(this Models.Fleet model)
        {
            return new Fleet
            {
                Id = model.Id,
                Name = model.Name,
                Ships = model.Ships.Select(s => ModelExtensions.ToEntity(s)).ToList()
            };
        }
    
        public static Ship ToEntity(this Models.Ship model)
        {
            return new Ship
            {
                Id = model.Id,
                Model = model.Model,
                IsUnderConstruction = model.IsUnderConstruction
            };
        }
    
        public static Planet ToEntity(this Models.Planet model)
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
    
        public static StarSystem ToEntity(this Models.StarSystem model)
        {
            return new StarSystem
            {
                Id = model.Id,
                Name = model.Name,
                Coordinates = model.Coordinates,
                Planets = model.Planets.Select(p => ModelExtensions.ToEntity(p)).ToList()
            };
        }
    
        public static Session ToEntity(this Models.Session model)
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
}
