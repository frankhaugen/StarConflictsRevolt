using StarConflictsRevolt.Server.Simulation.Engine;
using StarConflictsRevolt.Server.WebApi.Application.Services.AI;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;

/// <summary>
/// Listens to ticks and processes AI turns for this tick.
/// </summary>
public sealed class AiTurnTickListener : ITickListener
{
    private readonly AiTurnService _aiTurnService;
    private readonly ILogger<AiTurnTickListener> _logger;

    public AiTurnTickListener(AiTurnService aiTurnService, ILogger<AiTurnTickListener> logger)
    {
        _aiTurnService = aiTurnService;
        _logger = logger;
    }

    public async Task OnTickAsync(GameTickMessage tick, CancellationToken cancellationToken)
    {
        _logger.LogDebug("AiTurnTickListener processing tick {TickNumber}", tick.TickNumber);
        await _aiTurnService.ProcessTickAsync(tick, cancellationToken);
    }
}
