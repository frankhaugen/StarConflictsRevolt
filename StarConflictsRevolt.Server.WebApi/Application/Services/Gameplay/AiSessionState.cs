using StarConflictsRevolt.Server.WebApi.Core.Domain.Enums;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;

public class AiSessionState
{
    public GameSessionId SessionId { get; set; }
    public AiDifficulty AiDifficulty { get; set; }
    public GameTickNumber LastAiTick { get; set; }
    public GameTimestamp LastAiActionTime { get; set; }
}