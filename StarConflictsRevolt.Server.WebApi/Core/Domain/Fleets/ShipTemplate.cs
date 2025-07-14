namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Fleets;

public class ShipTemplate
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Combat stats
    public int Attack { get; set; }
    public int Health { get; set; }
    public int Speed { get; set; }
    public int Cargo { get; set; }

    // Costs
    public int CreditsCost { get; set; }
    public int MaterialsCost { get; set; }
    public int FuelCost { get; set; }

    // Build time in seconds
    public int BuildTimeSeconds { get; set; }

    // Special abilities
    public bool CanTransportTroops { get; set; }
    public bool CanBombardPlanets { get; set; }
    public bool IsStealth { get; set; }

    public static ShipTemplate Scout => new()
    {
        Name = "Scout",
        Description = "Fast reconnaissance vessel",
        Attack = 1,
        Health = 10,
        Speed = 3,
        Cargo = 5,
        CreditsCost = 50,
        MaterialsCost = 25,
        FuelCost = 10,
        BuildTimeSeconds = 30,
        CanTransportTroops = false,
        CanBombardPlanets = false,
        IsStealth = true
    };

    public static ShipTemplate Fighter => new()
    {
        Name = "Fighter",
        Description = "Balanced combat vessel",
        Attack = 2,
        Health = 20,
        Speed = 2,
        Cargo = 10,
        CreditsCost = 100,
        MaterialsCost = 50,
        FuelCost = 25,
        BuildTimeSeconds = 60,
        CanTransportTroops = false,
        CanBombardPlanets = false,
        IsStealth = false
    };

    public static ShipTemplate Destroyer => new()
    {
        Name = "Destroyer",
        Description = "Heavy combat vessel",
        Attack = 4,
        Health = 40,
        Speed = 1,
        Cargo = 20,
        CreditsCost = 200,
        MaterialsCost = 100,
        FuelCost = 50,
        BuildTimeSeconds = 120,
        CanTransportTroops = false,
        CanBombardPlanets = true,
        IsStealth = false
    };

    public static ShipTemplate Cruiser => new()
    {
        Name = "Cruiser",
        Description = "Capital ship with heavy firepower",
        Attack = 6,
        Health = 60,
        Speed = 1,
        Cargo = 30,
        CreditsCost = 400,
        MaterialsCost = 200,
        FuelCost = 100,
        BuildTimeSeconds = 180,
        CanTransportTroops = true,
        CanBombardPlanets = true,
        IsStealth = false
    };

    public static ShipTemplate Transport => new()
    {
        Name = "Transport",
        Description = "Cargo vessel for resources and troops",
        Attack = 1,
        Health = 30,
        Speed = 1,
        Cargo = 50,
        CreditsCost = 150,
        MaterialsCost = 75,
        FuelCost = 25,
        BuildTimeSeconds = 90,
        CanTransportTroops = true,
        CanBombardPlanets = false,
        IsStealth = false
    };
}