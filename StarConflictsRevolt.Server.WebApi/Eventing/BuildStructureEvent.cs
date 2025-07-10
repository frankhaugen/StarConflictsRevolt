using StarConflictsRevolt.Server.WebApi.Enums;
using StarConflictsRevolt.Server.WebApi.Models;

namespace StarConflictsRevolt.Server.WebApi.Eventing;

public record BuildStructureEvent(Guid PlayerId, Guid PlanetId, string StructureType) : IGameEvent
{
    public void ApplyTo(World world, Microsoft.Extensions.Logging.ILogger logger)
    {
        // Find the planet and its containing system
        Planet? planet = null;
        StarSystem? containingSystem = null;
        foreach (var system in world.Galaxy.StarSystems)
        {
            planet = system.Planets.FirstOrDefault(p => p.Id == PlanetId);
            if (planet != null)
            {
                containingSystem = system;
                break;
            }
        }

        if (planet == null || containingSystem == null)
        {
            logger.LogWarning("Planet {PlanetId} not found for BuildStructureEvent", PlanetId);
            return;
        }

        // Validate ownership
        if (planet.OwnerId != PlayerId)
        {
            logger.LogWarning("Player {PlayerId} does not own planet {PlanetId}", PlayerId, PlanetId);
            return;
        }

        // Parse structure type
        if (!Enum.TryParse<StructureVariant>(StructureType, out var variant))
        {
            logger.LogWarning("Invalid structure type: {StructureType}", StructureType);
            return;
        }

        // Check resource costs
        var (mineralsCost, energyCost) = GetStructureCosts(variant);
        if (planet.Minerals < mineralsCost || planet.Energy < energyCost)
        {
            logger.LogWarning("Insufficient resources to build {StructureType} on planet {PlanetId}", StructureType, PlanetId);
            return;
        }

        // Deduct resources and add structure
        var updatedPlanet = planet with
        {
            Minerals = planet.Minerals - mineralsCost,
            Energy = planet.Energy - energyCost,
            Structures = planet.Structures.Append(new Structure(
                variant,
                planet, // reference to the planet
                PlayerId,
                Health: 100,
                MaxHealth: 100,
                IsOperational: true,
                LastProductionTime: DateTime.UtcNow
            )).ToList()
        };

        // Replace the planet in the system's planet list
        var planetIndex = containingSystem.Planets.FindIndex(p => p.Id == PlanetId);
        if (planetIndex >= 0)
        {
            containingSystem.Planets[planetIndex] = updatedPlanet;
        }

        logger.LogInformation("Built {StructureType} on planet {PlanetId} for player {PlayerId}", StructureType, PlanetId, PlayerId);
    }

    private static (int minerals, int energy) GetStructureCosts(StructureVariant variant)
    {
        return variant switch
        {
            StructureVariant.ConstructionYard => (50, 25),
            StructureVariant.TrainingFacility => (30, 40),
            StructureVariant.Shipyard => (100, 75),
            StructureVariant.Mine => (20, 10),
            StructureVariant.Refinery => (40, 30),
            StructureVariant.ShieldGenerator => (80, 60),
            _ => (50, 25) // Default cost
        };
    }
}