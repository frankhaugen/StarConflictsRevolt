namespace StarConflictsRevolt.Clients.Models;

public record GalaxyDto(Guid Id, IEnumerable<StarSystemDto> StarSystems) : IGameObject;