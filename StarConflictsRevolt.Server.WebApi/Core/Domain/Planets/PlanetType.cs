namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Planets;

public class PlanetType
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Resource bonuses per turn
    public int CreditsBonus { get; set; }
    public int MaterialsBonus { get; set; }
    public int FuelBonus { get; set; }
    public int FoodBonus { get; set; }

    // Special characteristics
    public bool CanBuildStructures { get; set; } = true;
    public bool CanSupportPopulation { get; set; } = true;
    public int MaxStructures { get; set; } = 5;
    public int PopulationCapacity { get; set; } = 1000;

    // Visual characteristics
    public string Color { get; set; } = "#4A90E2";
    public string TerrainType { get; set; } = "Standard";

    public static PlanetType Terran => new()
    {
        Name = "Terran",
        Description = "Earth-like planet with balanced resources",
        CreditsBonus = 5,
        MaterialsBonus = 5,
        FuelBonus = 5,
        FoodBonus = 10,
        CanBuildStructures = true,
        CanSupportPopulation = true,
        MaxStructures = 6,
        PopulationCapacity = 1500,
        Color = "#4A90E2",
        TerrainType = "Continental"
    };

    public static PlanetType Desert => new()
    {
        Name = "Desert",
        Description = "Arid planet rich in materials",
        CreditsBonus = 0,
        MaterialsBonus = 15,
        FuelBonus = 0,
        FoodBonus = -5,
        CanBuildStructures = true,
        CanSupportPopulation = true,
        MaxStructures = 4,
        PopulationCapacity = 800,
        Color = "#D4A574",
        TerrainType = "Desert"
    };

    public static PlanetType Ice => new()
    {
        Name = "Ice",
        Description = "Frozen world with fuel deposits",
        CreditsBonus = 0,
        MaterialsBonus = -5,
        FuelBonus = 15,
        FoodBonus = 0,
        CanBuildStructures = true,
        CanSupportPopulation = true,
        MaxStructures = 4,
        PopulationCapacity = 600,
        Color = "#B8E6B8",
        TerrainType = "Ice"
    };

    public static PlanetType GasGiant => new()
    {
        Name = "Gas Giant",
        Description = "Massive gas planet with fuel reserves",
        CreditsBonus = 0,
        MaterialsBonus = 0,
        FuelBonus = 20,
        FoodBonus = 0,
        CanBuildStructures = false,
        CanSupportPopulation = false,
        MaxStructures = 0,
        PopulationCapacity = 0,
        Color = "#FFD700",
        TerrainType = "Gas"
    };

    public static PlanetType Asteroid => new()
    {
        Name = "Asteroid",
        Description = "Rocky body rich in minerals",
        CreditsBonus = 0,
        MaterialsBonus = 20,
        FuelBonus = 0,
        FoodBonus = -10,
        CanBuildStructures = true,
        CanSupportPopulation = false,
        MaxStructures = 3,
        PopulationCapacity = 0,
        Color = "#8B4513",
        TerrainType = "Rocky"
    };

    public static PlanetType Ocean => new()
    {
        Name = "Ocean",
        Description = "Water world with abundant food",
        CreditsBonus = 0,
        MaterialsBonus = -5,
        FuelBonus = 0,
        FoodBonus = 15,
        CanBuildStructures = true,
        CanSupportPopulation = true,
        MaxStructures = 4,
        PopulationCapacity = 1200,
        Color = "#0066CC",
        TerrainType = "Oceanic"
    };
}