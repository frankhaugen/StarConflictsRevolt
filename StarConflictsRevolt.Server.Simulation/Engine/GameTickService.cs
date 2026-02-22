using Microsoft.Extensions.Hosting;

namespace StarConflictsRevolt.Server.Simulation.Engine;

public class GameTickService(
    ILogger<GameTickService> logger,
    ITickPublisher publisher,
    ISimulationManager simulationManager,
    ITickerLiveness? tickerLiveness = null) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("GameTickService starting; speed controlled by ISimulationManager (initial {TicksPerSecond} t/s)",
            simulationManager.TicksPerSecond);

        var tickNumber = 0L;

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                tickerLiveness?.SetLastTick(DateTime.UtcNow);
                var tickInterval = simulationManager.GetTickInterval();
                var tickStart = DateTime.UtcNow;

                if (!simulationManager.IsPaused)
                {
                    tickNumber++;
                    await PublishTickAsync(tickNumber, stoppingToken);
                }

                var tickDuration = DateTime.UtcNow - tickStart;
                var waitTime = tickInterval - tickDuration;

                if (waitTime > TimeSpan.Zero)
                {
                    await Task.Delay(waitTime, stoppingToken);
                }
                else if (!simulationManager.IsPaused)
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

            await publisher.PublishTickAsync(tick, stoppingToken);

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
        await base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        base.Dispose();
    }
}
