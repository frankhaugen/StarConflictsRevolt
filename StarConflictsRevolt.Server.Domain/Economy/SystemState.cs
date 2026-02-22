namespace StarConflictsRevolt.Server.Domain.Economy;

/// <summary>
/// Minimal system state for economy: industry, production, control.
/// </summary>
public record SystemState(int Industry, int ProductionRate, bool Controlled);
