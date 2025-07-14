using StarConflictsRevolt.Server.WebApi.Core.Domain.Gameplay;

namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Stars;

internal record Star(Guid Id, string Name, double Radius, double Mass, double Luminosity) : GameObject;