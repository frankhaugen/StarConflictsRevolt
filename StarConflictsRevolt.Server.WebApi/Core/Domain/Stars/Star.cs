namespace StarConflictsRevolt.Server.WebApi.Models;

internal record Star(Guid Id, string Name, double Radius, double Mass, double Luminosity) : GameObject;