namespace StarConflictsRevolt.Server.WebApi.Models;

public record Ship(
    Guid Id,
    string Model,
    bool IsUnderConstruction,
    int Health = 100,
    int MaxHealth = 100,
    int AttackPower = 10,
    int DefensePower = 5,
    double Speed = 1.0
);