using StarConflictsRevolt.Server.Domain.Enums;
using StarConflictsRevolt.Server.Simulation.Engine;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;

public class AiSessionState
{
    public GameSessionId SessionId { get; set; } = default!;
    public AiDifficulty AiDifficulty { get; set; }
    public GameTickNumber LastAiTick { get; set; } = default!;
    public GameTimestamp LastAiActionTime { get; set; } = default!;
}