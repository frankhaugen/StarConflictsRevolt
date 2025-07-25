using Frank.PulseFlow;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;

public class GameTickService(ILogger<GameTickService> logger, IConduit conduit) : BackgroundService
{
    // Game timing configuration
    private const int TICKS_PER_SECOND = 10; // 10 ticks per second = 100ms per tick

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("GameTickService starting with {TicksPerSecond} ticks per second", TICKS_PER_SECOND);
        
        var tickInterval = TimeSpan.FromMilliseconds(1000.0 / TICKS_PER_SECOND);
        var tickNumber = 0L;
        
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var tickStart = DateTime.UtcNow;
                tickNumber++;

                // Publish tick
                await PublishTickAsync(tickNumber, stoppingToken);

                // Calculate time to wait for next tick
                var tickDuration = DateTime.UtcNow - tickStart;
                var waitTime = tickInterval - tickDuration;
                
                if (waitTime > TimeSpan.Zero)
                {
                    await Task.Delay(waitTime, stoppingToken);
                }
                else
                {
                    logger.LogWarning("Tick {TickNumber} took {TickDuration}ms, exceeding target interval of {TargetInterval}ms", 
                        tickNumber, tickDuration.TotalMilliseconds, tickInterval.TotalMilliseconds);
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("GameTickService cancellation requested after {TickCount} ticks", tickNumber);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error in GameTickService main loop");
        }
        finally
        {
            logger.LogInformation("GameTickService exiting");
        }
    }

    private async Task PublishTickAsync(long tickNumber, CancellationToken stoppingToken)
    {
        try
        {
            var tick = new GameTickMessage
            {
                TickNumber = new GameTickNumber(tickNumber),
                Timestamp = new GameTimestamp(DateTime.UtcNow)
            };

            await conduit.SendAsync(tick, stoppingToken);
            
            logger.LogDebug("Published tick {TickNumber}", tickNumber);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to publish tick {TickNumber}", tickNumber);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("GameTickService stopping...");
        
        // Complete the channel writer
        // _pulse.Complete(); // Removed as per edit hint
        
        await base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        // _pulse?.Complete(); // Removed as per edit hint
        base.Dispose();
    }
} 