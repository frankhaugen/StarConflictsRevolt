namespace StarConflictsRevolt.Server.Datastore.Extensions
{
    // Mostly mapping extensions for Models into Entities
    public static class ModelExtensions
    {
        public static StarConflictsRevolt.Server.Datastore.Entities.Fleet ToEntity(this StarConflictsRevolt.Server.Core.Models.Fleet model)
        {
            return new StarConflictsRevolt.Server.Datastore.Entities.Fleet
            {
                Id = model.Id,
                Name = model.Name,
                Ships = model.Ships.Select(s => StarConflictsRevolt.Server.Datastore.Extensions.ModelExtensions.ToEntity(s)).ToList()
            };
        }
    
        public static StarConflictsRevolt.Server.Datastore.Entities.Ship ToEntity(this StarConflictsRevolt.Server.Core.Models.Ship model)
        {
            return new StarConflictsRevolt.Server.Datastore.Entities.Ship
            {
                Id = model.Id,
                Model = model.Model,
                IsUnderConstruction = model.IsUnderConstruction
            };
        }
    
        public static StarConflictsRevolt.Server.Datastore.Entities.Planet ToEntity(this StarConflictsRevolt.Server.Core.Models.Planet model)
        {
            // Only map persistent properties; Fleets and Structures are not persisted directly
            return new StarConflictsRevolt.Server.Datastore.Entities.Planet
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
    
        public static StarConflictsRevolt.Server.Datastore.Entities.StarSystem ToEntity(this StarConflictsRevolt.Server.Core.Models.StarSystem model)
        {
            return new StarConflictsRevolt.Server.Datastore.Entities.StarSystem
            {
                Id = model.Id,
                Name = model.Name,
                Coordinates = model.Coordinates,
                Planets = model.Planets.Select(p => StarConflictsRevolt.Server.Datastore.Extensions.ModelExtensions.ToEntity(p)).ToList()
            };
        }
    
        public static StarConflictsRevolt.Server.Datastore.Entities.Session ToEntity(this StarConflictsRevolt.Server.Core.Models.Session model)
        {
            return new StarConflictsRevolt.Server.Datastore.Entities.Session
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
