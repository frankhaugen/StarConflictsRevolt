using System.Collections;

namespace StarConflictsRevolt.Clients.Shared;

public record GalaxyDto(Guid Id, IEnumerable<StarSystemDto> StarSystems) : IGameObject;