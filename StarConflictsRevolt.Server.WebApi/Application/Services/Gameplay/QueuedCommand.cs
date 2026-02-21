using StarConflictsRevolt.Server.WebApi.Core.Domain.Commands;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;

public sealed record QueuedCommand(GameSessionId SessionId, IGameCommand Command);
