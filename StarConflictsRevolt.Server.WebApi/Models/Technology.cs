namespace StarConflictsRevolt.Server.WebApi.Models;

public class Technology
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Research requirements
    public int ResearchCost { get; set; }
    public int ResearchTimeSeconds { get; set; }
    public List<string> Prerequisites { get; set; } = new();

    // Technology level (1-5)
    public int Level { get; set; }

    // Category
    public TechnologyCategory Category { get; set; }

    // Effects when researched
    public int ShipAttackBonus { get; set; }
    public int ShipHealthBonus { get; set; }
    public int ShipSpeedBonus { get; set; }
    public int StructureEfficiencyBonus { get; set; }
    public int ResourceProductionBonus { get; set; }
    public double CostReductionPercent { get; set; }

    // Special abilities unlocked
    public bool UnlocksStealth { get; set; }
    public bool UnlocksBombardment { get; set; }
    public bool UnlocksAdvancedShips { get; set; }

    // Static technology definitions
    public static Technology BasicWeapons => new()
    {
        Name = "Basic Weapons",
        Description = "Improved ship weaponry",
        ResearchCost = 100,
        ResearchTimeSeconds = 60,
        Level = 1,
        Category = TechnologyCategory.Combat,
        ShipAttackBonus = 1,
        Prerequisites = new List<string>()
    };

    public static Technology AdvancedWeapons => new()
    {
        Name = "Advanced Weapons",
        Description = "Superior ship weaponry",
        ResearchCost = 200,
        ResearchTimeSeconds = 120,
        Level = 2,
        Category = TechnologyCategory.Combat,
        ShipAttackBonus = 2,
        Prerequisites = new List<string> { "Basic Weapons" }
    };

    public static Technology HeavyWeapons => new()
    {
        Name = "Heavy Weapons",
        Description = "Capital ship weaponry",
        ResearchCost = 400,
        ResearchTimeSeconds = 180,
        Level = 3,
        Category = TechnologyCategory.Combat,
        ShipAttackBonus = 3,
        UnlocksBombardment = true,
        Prerequisites = new List<string> { "Advanced Weapons" }
    };

    public static Technology BasicArmor => new()
    {
        Name = "Basic Armor",
        Description = "Improved ship protection",
        ResearchCost = 100,
        ResearchTimeSeconds = 60,
        Level = 1,
        Category = TechnologyCategory.Defense,
        ShipHealthBonus = 10,
        Prerequisites = new List<string>()
    };

    public static Technology AdvancedArmor => new()
    {
        Name = "Advanced Armor",
        Description = "Superior ship protection",
        ResearchCost = 200,
        ResearchTimeSeconds = 120,
        Level = 2,
        Category = TechnologyCategory.Defense,
        ShipHealthBonus = 20,
        Prerequisites = new List<string> { "Basic Armor" }
    };

    public static Technology ReinforcedHull => new()
    {
        Name = "Reinforced Hull",
        Description = "Maximum ship protection",
        ResearchCost = 400,
        ResearchTimeSeconds = 180,
        Level = 3,
        Category = TechnologyCategory.Defense,
        ShipHealthBonus = 30,
        Prerequisites = new List<string> { "Advanced Armor" }
    };

    public static Technology BasicEngines => new()
    {
        Name = "Basic Engines",
        Description = "Improved ship propulsion",
        ResearchCost = 100,
        ResearchTimeSeconds = 60,
        Level = 1,
        Category = TechnologyCategory.Propulsion,
        ShipSpeedBonus = 1,
        Prerequisites = new List<string>()
    };

    public static Technology AdvancedEngines => new()
    {
        Name = "Advanced Engines",
        Description = "Superior ship propulsion",
        ResearchCost = 200,
        ResearchTimeSeconds = 120,
        Level = 2,
        Category = TechnologyCategory.Propulsion,
        ShipSpeedBonus = 2,
        Prerequisites = new List<string> { "Basic Engines" }
    };

    public static Technology Hyperdrive => new()
    {
        Name = "Hyperdrive",
        Description = "Instant travel between systems",
        ResearchCost = 500,
        ResearchTimeSeconds = 240,
        Level = 3,
        Category = TechnologyCategory.Propulsion,
        ShipSpeedBonus = 3,
        UnlocksStealth = true,
        Prerequisites = new List<string> { "Advanced Engines" }
    };

    public static Technology BasicMining => new()
    {
        Name = "Basic Mining",
        Description = "Improved resource extraction",
        ResearchCost = 100,
        ResearchTimeSeconds = 60,
        Level = 1,
        Category = TechnologyCategory.Economy,
        ResourceProductionBonus = 10,
        Prerequisites = new List<string>()
    };

    public static Technology AdvancedMining => new()
    {
        Name = "Advanced Mining",
        Description = "Superior resource extraction",
        ResearchCost = 200,
        ResearchTimeSeconds = 120,
        Level = 2,
        Category = TechnologyCategory.Economy,
        ResourceProductionBonus = 20,
        Prerequisites = new List<string> { "Basic Mining" }
    };

    public static Technology AutomatedMining => new()
    {
        Name = "Automated Mining",
        Description = "Maximum resource extraction",
        ResearchCost = 400,
        ResearchTimeSeconds = 180,
        Level = 3,
        Category = TechnologyCategory.Economy,
        ResourceProductionBonus = 30,
        CostReductionPercent = 10,
        Prerequisites = new List<string> { "Advanced Mining" }
    };

    public static Technology BasicConstruction => new()
    {
        Name = "Basic Construction",
        Description = "Improved building efficiency",
        ResearchCost = 100,
        ResearchTimeSeconds = 60,
        Level = 1,
        Category = TechnologyCategory.Industry,
        StructureEfficiencyBonus = 10,
        Prerequisites = new List<string>()
    };

    public static Technology AdvancedConstruction => new()
    {
        Name = "Advanced Construction",
        Description = "Superior building efficiency",
        ResearchCost = 200,
        ResearchTimeSeconds = 120,
        Level = 2,
        Category = TechnologyCategory.Industry,
        StructureEfficiencyBonus = 20,
        Prerequisites = new List<string> { "Basic Construction" }
    };

    public static Technology MegaConstruction => new()
    {
        Name = "Mega Construction",
        Description = "Maximum building efficiency",
        ResearchCost = 400,
        ResearchTimeSeconds = 180,
        Level = 3,
        Category = TechnologyCategory.Industry,
        StructureEfficiencyBonus = 30,
        CostReductionPercent = 15,
        UnlocksAdvancedShips = true,
        Prerequisites = new List<string> { "Advanced Construction" }
    };
}