using System.Numerics;

namespace StarConflictsRevolt.Clients.Models;

public record StarSystemDto(Guid Id, string Name, IEnumerable<PlanetDto> Planets, Vector2 Coordinates) : IGameObject;