namespace StarConflictsRevolt.Server.WebApi.Models;

record Star(Guid Id, string Name, double Radius, double Mass, double Luminosity) : GameObject;