namespace StarConflictsRevolt.Server.Core;

record HumanController(Guid PlayerId, string ConnectionId) : PlayerController;