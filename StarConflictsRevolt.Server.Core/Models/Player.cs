namespace StarConflictsRevolt.Server.Core.Models;

record Player(Guid Id, string Name, PlayerController Controller) : GameObject;