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
                Ships = model.Ships.Select(s => s.ToEntity()).ToList()
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
            return new StarConflictsRevolt.Server.Datastore.Entities.Planet
            {
                Id = model.Id,
                Name = model.Name
            };
        }
    
        public static StarConflictsRevolt.Server.Datastore.Entities.StarSystem ToEntity(this StarConflictsRevolt.Server.Core.Models.StarSystem model)
        {
            return new StarConflictsRevolt.Server.Datastore.Entities.StarSystem
            {
                Id = model.Id,
                Name = model.Name,
                Coordinates = model.Coordinates,
                Planets = model.Planets.Select(p => p.ToEntity()).ToList()
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
