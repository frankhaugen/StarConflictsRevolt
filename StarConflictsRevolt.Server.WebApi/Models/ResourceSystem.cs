namespace StarConflictsRevolt.Server.WebApi.Models;

public class ResourceSystem
{
    public static readonly Dictionary<ResourceType, ResourceDefinition> ResourceDefinitions = new()
    {
        [ResourceType.Credits] = new()
        {
            Name = "Credits",
            Description = "Universal currency",
            BaseValue = 1,
            StorageLimit = 10000,
            ConversionRates = new Dictionary<ResourceType, double>
            {
                [ResourceType.Materials] = 2.0, // 2 materials = 1 credit
                [ResourceType.Fuel] = 3.0,      // 3 fuel = 1 credit
                [ResourceType.Food] = 1.5       // 1.5 food = 1 credit
            }
        },
        
        [ResourceType.Materials] = new()
        {
            Name = "Materials",
            Description = "Raw construction materials",
            BaseValue = 2,
            StorageLimit = 5000,
            ConversionRates = new Dictionary<ResourceType, double>
            {
                [ResourceType.Credits] = 0.5,   // 1 material = 0.5 credits
                [ResourceType.Fuel] = 1.5,      // 1.5 materials = 1 fuel
                [ResourceType.Food] = 0.75      // 0.75 materials = 1 food
            }
        },
        
        [ResourceType.Fuel] = new()
        {
            Name = "Fuel",
            Description = "Starship fuel and energy",
            BaseValue = 3,
            StorageLimit = 3000,
            ConversionRates = new Dictionary<ResourceType, double>
            {
                [ResourceType.Credits] = 0.33,  // 1 fuel = 0.33 credits
                [ResourceType.Materials] = 0.67, // 1 fuel = 0.67 materials
                [ResourceType.Food] = 0.5       // 1 fuel = 0.5 food
            }
        },
        
        [ResourceType.Food] = new()
        {
            Name = "Food",
            Description = "Agricultural products",
            BaseValue = 1.5,
            StorageLimit = 8000,
            ConversionRates = new Dictionary<ResourceType, double>
            {
                [ResourceType.Credits] = 0.67,  // 1 food = 0.67 credits
                [ResourceType.Materials] = 1.33, // 1 food = 1.33 materials
                [ResourceType.Fuel] = 2.0       // 2 food = 1 fuel
            }
        }
    };
    
    // Convert resources between types
    public static bool TryConvertResource(ResourceType fromType, ResourceType toType, int amount, out int convertedAmount)
    {
        convertedAmount = 0;
        
        if (!ResourceDefinitions.ContainsKey(fromType) || !ResourceDefinitions.ContainsKey(toType))
            return false;
        
        if (fromType == toType)
        {
            convertedAmount = amount;
            return true;
        }
        
        var fromResource = ResourceDefinitions[fromType];
        if (!fromResource.ConversionRates.ContainsKey(toType))
            return false;
        
        convertedAmount = (int)(amount * fromResource.ConversionRates[toType]);
        return true;
    }
    
    // Calculate resource value in credits
    public static int GetResourceValue(ResourceType type, int amount)
    {
        if (!ResourceDefinitions.ContainsKey(type))
            return 0;
        
        return (int)(amount * ResourceDefinitions[type].BaseValue);
    }
    
    // Check if resource transfer is valid
    public static bool CanTransferResource(ResourceType type, int currentAmount, int transferAmount, int storageLimit)
    {
        if (transferAmount <= 0)
            return false;
        
        if (currentAmount + transferAmount > storageLimit)
            return false;
        
        return true;
    }
    
    // Calculate maintenance costs for structures
    public static Dictionary<ResourceType, int> CalculateMaintenanceCosts(List<Structure> structures)
    {
        var costs = new Dictionary<ResourceType, int>
        {
            [ResourceType.Credits] = 0,
            [ResourceType.Materials] = 0,
            [ResourceType.Fuel] = 0,
            [ResourceType.Food] = 0
        };
        
        foreach (var structure in structures)
        {
            var template = GetStructureMaintenanceCost(structure.Variant);
            foreach (var cost in template)
            {
                costs[cost.Key] += cost.Value;
            }
        }
        
        return costs;
    }
    
    // Get maintenance costs for a structure type
    private static Dictionary<ResourceType, int> GetStructureMaintenanceCost(Enums.StructureVariant variant)
    {
        return variant switch
        {
            Enums.StructureVariant.Mine => new() { [ResourceType.Credits] = 5 },
            Enums.StructureVariant.Refinery => new() { [ResourceType.Credits] = 8, [ResourceType.Materials] = 2 },
            Enums.StructureVariant.Shipyard => new() { [ResourceType.Credits] = 15, [ResourceType.Materials] = 5, [ResourceType.Fuel] = 3 },
            Enums.StructureVariant.TrainingFacility => new() { [ResourceType.Credits] = 10, [ResourceType.Materials] = 3 },
            Enums.StructureVariant.ShieldGenerator => new() { [ResourceType.Credits] = 20, [ResourceType.Fuel] = 5 },
            Enums.StructureVariant.ConstructionYard => new() { [ResourceType.Credits] = 12, [ResourceType.Materials] = 4 },
            _ => new() { [ResourceType.Credits] = 5 }
        };
    }
    
    // Calculate resource production for a planet
    public static Dictionary<ResourceType, int> CalculateResourceProduction(Planet planet)
    {
        var production = new Dictionary<ResourceType, int>
        {
            [ResourceType.Credits] = planet.PlanetType?.CreditsBonus ?? 0,
            [ResourceType.Materials] = planet.PlanetType?.MaterialsBonus ?? 0,
            [ResourceType.Fuel] = planet.PlanetType?.FuelBonus ?? 0,
            [ResourceType.Food] = planet.PlanetType?.FoodBonus ?? 0
        };
        
        // Add structure bonuses
        foreach (var structure in planet.Structures)
        {
            var structureProduction = GetStructureProduction(structure.Variant);
            foreach (var resource in structureProduction)
            {
                production[resource.Key] += resource.Value;
            }
        }
        
        // Apply planet production rate multiplier
        foreach (var resource in production.Keys.ToList())
        {
            production[resource] = (int)(production[resource] * planet.ProductionRate);
        }
        
        return production;
    }
    
    // Get production bonuses for a structure type
    private static Dictionary<ResourceType, int> GetStructureProduction(Enums.StructureVariant variant)
    {
        return variant switch
        {
            Enums.StructureVariant.Mine => new() { [ResourceType.Materials] = 10 },
            Enums.StructureVariant.Refinery => new() { [ResourceType.Fuel] = 5 },
            Enums.StructureVariant.Shipyard => new() { [ResourceType.Credits] = 5 },
            Enums.StructureVariant.TrainingFacility => new() { [ResourceType.Credits] = 3 },
            Enums.StructureVariant.ShieldGenerator => new() { [ResourceType.Credits] = 2 },
            Enums.StructureVariant.ConstructionYard => new() { [ResourceType.Credits] = 4 },
            _ => new()
        };
    }
}

public class ResourceDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double BaseValue { get; set; } // Value in credits
    public int StorageLimit { get; set; }
    public Dictionary<ResourceType, double> ConversionRates { get; set; } = new();
}

public enum ResourceType
{
    Credits,
    Materials,
    Fuel,
    Food
} 