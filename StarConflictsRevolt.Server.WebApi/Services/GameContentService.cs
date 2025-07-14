using StarConflictsRevolt.Server.WebApi.Models;

namespace StarConflictsRevolt.Server.WebApi.Services;

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
                ships.Add(new Ship(Guid.NewGuid(), "Destroyer", false, 40, 40, 4, 8, 1.0));
                break;
                
            case "cruiser":
                ships.Add(new Ship(Guid.NewGuid(), "Cruiser", false, 60, 60, 6, 10, 1.0));
                break;
                
            case "transport":
                ships.Add(new Ship(Guid.NewGuid(), "Transport", false, 30, 30, 1, 4, 1.0));
                break;
                
            case "balanced":
                ships.Add(new Ship(Guid.NewGuid(), "Fighter", false, 20, 20, 2, 5, 2.0));
                ships.Add(new Ship(Guid.NewGuid(), "Scout", false, 10, 10, 1, 3, 3.0));
                break;
                
            case "combat":
                ships.Add(new Ship(Guid.NewGuid(), "Fighter", false, 20, 20, 2, 5, 2.0));
                ships.Add(new Ship(Guid.NewGuid(), "Fighter", false, 20, 20, 2, 5, 2.0));
                ships.Add(new Ship(Guid.NewGuid(), "Destroyer", false, 40, 40, 4, 8, 1.0));
                break;
                
            case "invasion":
                ships.Add(new Ship(Guid.NewGuid(), "Cruiser", false, 60, 60, 6, 10, 1.0));
                ships.Add(new Ship(Guid.NewGuid(), "Destroyer", false, 40, 40, 4, 8, 1.0));
                ships.Add(new Ship(Guid.NewGuid(), "Transport", false, 30, 30, 1, 4, 1.0));
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
            "mine" => Enums.StructureVariant.Mine,
            "refinery" => Enums.StructureVariant.Refinery,
            "shipyard" => Enums.StructureVariant.Shipyard,
            "training facility" => Enums.StructureVariant.TrainingFacility,
            "shield generator" => Enums.StructureVariant.ShieldGenerator,
            "construction yard" => Enums.StructureVariant.ConstructionYard,
            _ => Enums.StructureVariant.Mine
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
        var hasShipyard = planet.Structures.Any(s => s.Variant == Enums.StructureVariant.Shipyard);
        return hasShipyard;
    }

    public string GetGameBalanceInfo()
    {
        var shipTemplates = GetAllShipTemplates();
        var structureTemplates = GetAllStructureTemplates();
        var planetTypes = GetAllPlanetTypes();

        return $"Game Content Loaded: {shipTemplates.Count()} ship types, {structureTemplates.Count()} structure types, {planetTypes.Count()} planet types";
    }
} 