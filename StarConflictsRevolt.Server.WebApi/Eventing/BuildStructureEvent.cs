namespace StarConflictsRevolt.Server.WebApi.Eventing;

using StarConflictsRevolt.Server.WebApi.Enums;

public record BuildStructureEvent(Guid PlayerId, Guid PlanetId, string StructureType) : IGameEvent
{
    public void ApplyTo(Models.World world, Microsoft.Extensions.Logging.ILogger logger)
    {
        foreach (var system in world.Galaxy.StarSystems)
        {
            var planet = system.Planets.FirstOrDefault(p => p.Id == PlanetId);
            if (planet != null)
            {
                var structure = new Models.Structure(
                    Enum.TryParse<StructureVariant>(StructureType, out var variant) ? variant : StructureVariant.ConstructionYard,
                    planet
                );
                planet.Structures.Add(structure);
                logger.LogInformation("Added structure {StructureType} to planet {PlanetId}", StructureType, PlanetId);
            }
            else
            {
                logger.LogWarning("Planet {PlanetId} not found for BuildStructureEvent", PlanetId);
            }
        }
    }
}