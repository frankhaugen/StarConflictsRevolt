using StarConflictsRevolt.Server.Simulation.Engine;

namespace StarConflictsRevolt.Server.Application.Services.Gameplay;

/// <summary>
/// Listens to ticks and runs game update (WorldEngine + ProcessAllSessions).
/// ReceiveTick is pushed by Transport; this listener does not broadcast tick.
/// </summary>
public sealed class GameUpdateTickListener : ITickListener
{
    private readonly GameUpdateService _gameUpdateService;
    private readonly ILogger<GameUpdateTickListener> _logger;

    public GameUpdateTickListener(GameUpdateService gameUpdateService, ILogger<GameUpdateTickListener> logger)
    {
        _gameUpdateService = gameUpdateService;
        _logger = logger;
    }

    public async Task OnTickAsync(GameTickMessage tick, CancellationToken cancellationToken)
    {
        _logger.LogDebug("GameUpdateTickListener processing tick {TickNumber}", tick.TickNumber);
        await _gameUpdateService.ProcessTickAsync(tick, cancellationToken);
    }
}
