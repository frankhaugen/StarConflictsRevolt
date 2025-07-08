using System.Numerics;

namespace StarConflictsRevolt.Clients.Shared;

public record StarSystemDto(Guid Id, string Name, IEnumerable<PlanetDto> Planets, Vector2 Coordinates) : IGameObject;