namespace StarConflictsRevolt.Server.Core;

record Player(Guid Id, string Name, PlayerController Controller) : GameObject;