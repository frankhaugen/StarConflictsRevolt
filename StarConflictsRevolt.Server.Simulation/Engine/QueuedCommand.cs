using StarConflictsRevolt.Server.EventStorage.Abstractions;

namespace StarConflictsRevolt.Server.Simulation.Engine;

public sealed record QueuedCommand(GameSessionId SessionId, IGameCommand Command);
