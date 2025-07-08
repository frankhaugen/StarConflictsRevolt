namespace StarConflictsRevolt.Clients.Shared;

public record StarDto(Guid Id, string Name, double Radius, double Mass, double Luminosity) : IGameObject;