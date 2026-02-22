using StarConflictsRevolt.Server.WebApi.Core.Domain.Gameplay;

namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Stars;

#pragma warning disable CS8907 // Parameter is used by record's synthesized property
internal record Star(Guid Id, string Name, double Radius, double Mass, double Luminosity) : GameObject;
#pragma warning restore CS8907