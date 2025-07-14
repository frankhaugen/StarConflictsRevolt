namespace StarConflictsRevolt.Clients.Models;

public record WorldDto(Guid Id, GalaxyDto Galaxy, GameStateInfoDto? PlayerState = null);