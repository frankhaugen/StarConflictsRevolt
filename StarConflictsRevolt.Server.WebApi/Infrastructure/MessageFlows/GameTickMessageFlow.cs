using Frank.PulseFlow;
using StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;

namespace StarConflictsRevolt.Server.WebApi.Infrastructure.MessageFlows;

public class GameTickMessageFlow : IFlow
{
    private readonly ILogger<GameTickMessageFlow> _logger;
    private readonly AiTurnService _aiTurnService;
    private readonly GameUpdateService _gameUpdateService;

    public GameTickMessageFlow(
        ILogger<GameTickMessageFlow> logger,
        AiTurnService aiTurnService,
        GameUpdateService gameUpdateService)
    {
        _logger = logger;
        _aiTurnService = aiTurnService;
        _gameUpdateService = gameUpdateService;
    }

    public async Task HandleAsync(IPulse pulse, CancellationToken cancellationToken)
    {
        if (pulse is not GameTickMessage tickMessage)
        {
            _logger.LogWarning("Received unexpected pulse type: {PulseType}", pulse.GetType().Name);
            return;
        }

        try
        {
            _logger.LogDebug("Processing game tick {TickNumber} at {Timestamp}", 
                tickMessage.TickNumber, tickMessage.Timestamp);

            // Process AI turns for this tick
            await _aiTurnService.ProcessTickAsync(tickMessage, cancellationToken);

            // Process game updates for this tick
            await _gameUpdateService.ProcessTickAsync(tickMessage, cancellationToken);

            _logger.LogDebug("Completed processing game tick {TickNumber}", tickMessage.TickNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing game tick {TickNumber}", tickMessage.TickNumber);
            throw;
        }
    }

    public bool CanHandle(Type pulseType) => pulseType == typeof(GameTickMessage);
}