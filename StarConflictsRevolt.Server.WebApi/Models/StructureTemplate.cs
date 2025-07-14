namespace StarConflictsRevolt.Server.WebApi.Models;

public class StructureTemplate
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    // Resource generation per turn
    public int CreditsPerTurn { get; set; }
    public int MaterialsPerTurn { get; set; }
    public int FuelPerTurn { get; set; }
    
    // Costs
    public int CreditsCost { get; set; }
    public int MaterialsCost { get; set; }
    public int FuelCost { get; set; }
    
    // Build time in seconds
    public int BuildTimeSeconds { get; set; }
    
    // Effects
    public int DefenseBonus { get; set; }
    public int HealthBonus { get; set; }
    public double CostReductionPercent { get; set; }
    
    // Capabilities
    public bool CanBuildShips { get; set; }
    public bool CanTrainTroops { get; set; }
    public bool CanResearch { get; set; }
    
    public static StructureTemplate Mine => new()
    {
        Name = "Mine",
        Description = "Extracts materials from the planet",
        MaterialsPerTurn = 10,
        CreditsCost = 100,
        MaterialsCost = 50,
        FuelCost = 25,
        BuildTimeSeconds = 60,
        DefenseBonus = 0,
        HealthBonus = 0,
        CostReductionPercent = 0,
        CanBuildShips = false,
        CanTrainTroops = false,
        CanResearch = false
    };
    
    public static StructureTemplate Refinery => new()
    {
        Name = "Refinery",
        Description = "Processes raw materials into fuel",
        FuelPerTurn = 5,
        CreditsCost = 150,
        MaterialsCost = 75,
        FuelCost = 0,
        BuildTimeSeconds = 90,
        DefenseBonus = 0,
        HealthBonus = 0,
        CostReductionPercent = 0,
        CanBuildShips = false,
        CanTrainTroops = false,
        CanResearch = false
    };
    
    public static StructureTemplate Shipyard => new()
    {
        Name = "Shipyard",
        Description = "Constructs starships",
        CreditsPerTurn = 0,
        MaterialsPerTurn = 0,
        FuelPerTurn = 0,
        CreditsCost = 300,
        MaterialsCost = 200,
        FuelCost = 100,
        BuildTimeSeconds = 120,
        DefenseBonus = 10,
        HealthBonus = 0,
        CostReductionPercent = 0,
        CanBuildShips = true,
        CanTrainTroops = false,
        CanResearch = false
    };
    
    public static StructureTemplate TrainingFacility => new()
    {
        Name = "Training Facility",
        Description = "Trains ground troops",
        CreditsPerTurn = 0,
        MaterialsPerTurn = 0,
        FuelPerTurn = 0,
        CreditsCost = 200,
        MaterialsCost = 100,
        FuelCost = 50,
        BuildTimeSeconds = 90,
        DefenseBonus = 15,
        HealthBonus = 0,
        CostReductionPercent = 0,
        CanBuildShips = false,
        CanTrainTroops = true,
        CanResearch = false
    };
    
    public static StructureTemplate ShieldGenerator => new()
    {
        Name = "Shield Generator",
        Description = "Provides planetary defense",
        CreditsPerTurn = 0,
        MaterialsPerTurn = 0,
        FuelPerTurn = 0,
        CreditsCost = 400,
        MaterialsCost = 200,
        FuelCost = 100,
        BuildTimeSeconds = 150,
        DefenseBonus = 50,
        HealthBonus = 20,
        CostReductionPercent = 0,
        CanBuildShips = false,
        CanTrainTroops = false,
        CanResearch = false
    };
    
    public static StructureTemplate ConstructionYard => new()
    {
        Name = "Construction Yard",
        Description = "Reduces building costs",
        CreditsPerTurn = 0,
        MaterialsPerTurn = 0,
        FuelPerTurn = 0,
        CreditsCost = 250,
        MaterialsCost = 150,
        FuelCost = 75,
        BuildTimeSeconds = 120,
        DefenseBonus = 0,
        HealthBonus = 0,
        CostReductionPercent = 25,
        CanBuildShips = false,
        CanTrainTroops = false,
        CanResearch = false
    };
} 