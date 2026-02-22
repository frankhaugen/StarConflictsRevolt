namespace StarConflictsRevolt.Server.Domain.Economy;

/// <summary>
/// Minimal faction economy: credits and shipyard capacity.
/// </summary>
public record FactionEconomy(int Credits, int ShipyardSlots);
