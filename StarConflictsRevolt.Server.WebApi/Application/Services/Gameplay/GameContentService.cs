using StarConflictsRevolt.Server.WebApi.Core.Domain.Enums;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Fleets;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Planets;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Resources;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Structures;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Technology;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Victory;
using VictoryCondition = StarConflictsRevolt.Server.WebApi.Core.Domain.Victory.VictoryCondition;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;

public class GameContentService
{
    private readonly ILogger<GameContentService> _logger;

    public GameContentService(ILogger<GameContentService> logger)
    {
        _logger = logger;
    }

    public IEnumerable<ShipTemplate> GetAllShipTemplates()
    {
        return new[]
        {
            ShipTemplate.Scout,
            ShipTemplate.Fighter,
            ShipTemplate.Destroyer,
            ShipTemplate.Cruiser,
            ShipTemplate.Transport
        };
    }

    public ShipTemplate? GetShipTemplate(string name)
    {
        return GetAllShipTemplates().FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public IEnumerable<StructureTemplate> GetAllStructureTemplates()
    {
        return new[]
        {
            StructureTemplate.Mine,
            StructureTemplate.Refinery,
            StructureTemplate.Shipyard,
            StructureTemplate.TrainingFacility,
            StructureTemplate.ShieldGenerator,
            StructureTemplate.ConstructionYard
        };
    }

    public StructureTemplate? GetStructureTemplate(string name)
    {
        return GetAllStructureTemplates().FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public IEnumerable<PlanetType> GetAllPlanetTypes()
    {
        return new[]
        {
            PlanetType.Terran,
            PlanetType.Desert,
            PlanetType.Ice,
            PlanetType.GasGiant,
            PlanetType.Asteroid,
            PlanetType.Ocean
        };
    }

    public PlanetType? GetPlanetType(string name)
    {
        return GetAllPlanetTypes().FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public Fleet CreateFleetFromTemplate(string templateName, Guid ownerId, Guid locationPlanetId)
    {
        var ships = new List<Ship>();

        switch (templateName.ToLower())
        {
            case "scout":
                ships.Add(new Ship(Guid.NewGuid(), "Scout", false, 10, 10, 1, 3, 3.0));
                break;

            case "fighter":
                ships.Add(new Ship(Guid.NewGuid(), "Fighter", false, 20, 20, 2, 5, 2.0));
                break;

            case "destroyer":
                ships.Add(new Ship(Guid.NewGuid(), "Destroyer", false, 40, 40, 4, 8));
                break;

            case "cruiser":
                ships.Add(new Ship(Guid.NewGuid(), "Cruiser", false, 60, 60, 6, 10));
                break;

            case "transport":
                ships.Add(new Ship(Guid.NewGuid(), "Transport", false, 30, 30, 1, 4));
                break;

            case "balanced":
                ships.Add(new Ship(Guid.NewGuid(), "Fighter", false, 20, 20, 2, 5, 2.0));
                ships.Add(new Ship(Guid.NewGuid(), "Scout", false, 10, 10, 1, 3, 3.0));
                break;

            case "combat":
                ships.Add(new Ship(Guid.NewGuid(), "Fighter", false, 20, 20, 2, 5, 2.0));
                ships.Add(new Ship(Guid.NewGuid(), "Fighter", false, 20, 20, 2, 5, 2.0));
                ships.Add(new Ship(Guid.NewGuid(), "Destroyer", false, 40, 40, 4, 8));
                break;

            case "invasion":
                ships.Add(new Ship(Guid.NewGuid(), "Cruiser", false, 60, 60, 6, 10));
                ships.Add(new Ship(Guid.NewGuid(), "Destroyer", false, 40, 40, 4, 8));
                ships.Add(new Ship(Guid.NewGuid(), "Transport", false, 30, 30, 1, 4));
                break;

            default:
                ships.Add(new Ship(Guid.NewGuid(), "Fighter", false, 20, 20, 2, 5, 2.0));
                break;
        }

        return new Fleet(Guid.NewGuid(), $"{templateName} Fleet", ships, locationPlanetId, ownerId);
    }

    public Structure CreateStructureFromTemplate(string templateName, Guid ownerId, Planet planet)
    {
        var variant = templateName.ToLower() switch
        {
            "mine" => StructureVariant.Mine,
            "refinery" => StructureVariant.Refinery,
            "shipyard" => StructureVariant.Shipyard,
            "training facility" => StructureVariant.TrainingFacility,
            "shield generator" => StructureVariant.ShieldGenerator,
            "construction yard" => StructureVariant.ConstructionYard,
            _ => StructureVariant.Mine
        };

        return new Structure(variant, planet, ownerId);
    }

    public int CalculateResourceCost(string resourceType, int amount)
    {
        return resourceType.ToLower() switch
        {
            "credits" => amount * 1,
            "materials" => amount * 2,
            "fuel" => amount * 3,
            _ => amount
        };
    }

    public bool CanBuildStructure(string structureName, Planet planet)
    {
        if (!planet.PlanetType?.CanBuildStructures ?? false)
            return false;

        if (planet.Structures.Count >= (planet.PlanetType?.MaxStructures ?? 5))
            return false;

        return true;
    }

    public bool CanBuildShip(string shipName, Planet planet)
    {
        // Check if planet has a shipyard
        var hasShipyard = planet.Structures.Any(s => s.Variant == StructureVariant.Shipyard);
        return hasShipyard;
    }


    public IEnumerable<StructureTemplate> GetStartingStructureSet(string setType)
    {
        return setType.ToLower() switch
        {
            "basic" => new[] { StructureTemplate.Mine, StructureTemplate.ConstructionYard },
            "military" => new[] { StructureTemplate.TrainingFacility, StructureTemplate.ShieldGenerator },
            "economic" => new[] { StructureTemplate.Mine, StructureTemplate.Refinery, StructureTemplate.Shipyard },
            "defensive" => new[] { StructureTemplate.ShieldGenerator, StructureTemplate.TrainingFacility },
            _ => new[] { StructureTemplate.Mine }
        };
    }

    // Technology system methods
    public IEnumerable<Technology> GetAllTechnologies()
    {
        return new[]
        {
            Technology.BasicWeapons,
            Technology.AdvancedWeapons,
            Technology.HeavyWeapons,
            Technology.BasicArmor,
            Technology.AdvancedArmor,
            Technology.ReinforcedHull,
            Technology.BasicEngines,
            Technology.AdvancedEngines,
            Technology.Hyperdrive,
            Technology.BasicMining,
            Technology.AdvancedMining,
            Technology.AutomatedMining,
            Technology.BasicConstruction,
            Technology.AdvancedConstruction,
            Technology.MegaConstruction
        };
    }

    public Technology? GetTechnology(string name)
    {
        return GetAllTechnologies().FirstOrDefault(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public IEnumerable<Technology> GetAvailableTechnologies(List<string> researchedTechnologies)
    {
        var allTechnologies = GetAllTechnologies();
        var available = new List<Technology>();

        foreach (var tech in allTechnologies)
        {
            if (researchedTechnologies.Contains(tech.Name))
                continue;

            // Check if all prerequisites are met
            var prerequisitesMet = tech.Prerequisites.All(prereq => researchedTechnologies.Contains(prereq));
            if (prerequisitesMet) available.Add(tech);
        }

        return available;
    }

    public bool CanResearchTechnology(string technologyName, List<string> researchedTechnologies, int availableCredits)
    {
        var tech = GetTechnology(technologyName);
        if (tech == null)
            return false;

        // Check if already researched
        if (researchedTechnologies.Contains(tech.Name))
            return false;

        // Check prerequisites
        var prerequisitesMet = tech.Prerequisites.All(prereq => researchedTechnologies.Contains(prereq));
        if (!prerequisitesMet)
            return false;

        // Check if player has enough credits
        return availableCredits >= tech.ResearchCost;
    }

    // Victory condition methods
    public IEnumerable<VictoryCondition> GetAllVictoryConditions()
    {
        return new[]
        {
            VictoryCondition.MilitaryVictory,
            VictoryCondition.EconomicVictory,
            VictoryCondition.TechnologyVictory,
            VictoryCondition.TimeVictory,
            VictoryCondition.DiplomaticVictory
        };
    }

    public VictoryCondition? GetVictoryCondition(string name)
    {
        return GetAllVictoryConditions().FirstOrDefault(v => v.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public VictoryCondition? GetVictoryCondition(VictoryType type)
    {
        return GetAllVictoryConditions().FirstOrDefault(v => v.Type == type);
    }

    // Resource system methods
    public Dictionary<ResourceType, ResourceDefinition> GetResourceDefinitions()
    {
        return ResourceSystem.ResourceDefinitions;
    }

    public ResourceDefinition? GetResourceDefinition(ResourceType type)
    {
        return ResourceSystem.ResourceDefinitions.TryGetValue(type, out var definition) ? definition : null;
    }

    public bool TryConvertResource(ResourceType fromType, ResourceType toType, int amount, out int convertedAmount)
    {
        return ResourceSystem.TryConvertResource(fromType, toType, amount, out convertedAmount);
    }

    public int GetResourceValue(ResourceType type, int amount)
    {
        return ResourceSystem.GetResourceValue(type, amount);
    }

    public Dictionary<ResourceType, int> CalculateMaintenanceCosts(List<Structure> structures)
    {
        return ResourceSystem.CalculateMaintenanceCosts(structures);
    }

    public Dictionary<ResourceType, int> CalculateResourceProduction(Planet planet)
    {
        return ResourceSystem.CalculateResourceProduction(planet);
    }

    // Game balance and information methods
    public string GetGameBalanceInfo()
    {
        var shipTemplates = GetAllShipTemplates();
        var structureTemplates = GetAllStructureTemplates();
        var planetTypes = GetAllPlanetTypes();
        var technologies = GetAllTechnologies();
        var victoryConditions = GetAllVictoryConditions();
        var resourceTypes = GetResourceDefinitions();

        return $"Game Content Loaded: {shipTemplates.Count()} ship types, {structureTemplates.Count()} structure types, " +
               $"{planetTypes.Count()} planet types, {technologies.Count()} technologies, " +
               $"{victoryConditions.Count()} victory conditions, {resourceTypes.Count()} resource types";
    }

    public Dictionary<string, object> GetGameStatistics()
    {
        return new Dictionary<string, object>
        {
            ["ShipTypes"] = GetAllShipTemplates().Count(),
            ["StructureTypes"] = GetAllStructureTemplates().Count(),
            ["PlanetTypes"] = GetAllPlanetTypes().Count(),
            ["Technologies"] = GetAllTechnologies().Count(),
            ["VictoryConditions"] = GetAllVictoryConditions().Count(),
            ["ResourceTypes"] = GetResourceDefinitions().Count(),
            ["TotalContentItems"] = GetAllShipTemplates().Count() +
                                    GetAllStructureTemplates().Count() +
                                    GetAllPlanetTypes().Count() +
                                    GetAllTechnologies().Count() +
                                    GetAllVictoryConditions().Count() +
                                    GetResourceDefinitions().Count()
        };
    }
}