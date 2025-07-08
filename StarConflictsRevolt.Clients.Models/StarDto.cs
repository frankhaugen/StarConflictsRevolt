namespace StarConflictsRevolt.Clients.Models;

public record StarDto(Guid Id, string Name, double Radius, double Mass, double Luminosity) : IGameObject;