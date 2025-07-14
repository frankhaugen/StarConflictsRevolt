using System.Threading.Channels;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;

public class GameTick
{
    public long TickNumber { get; set; }
    public DateTime Timestamp { get; set; }
}

public class GameTickService : BackgroundService
{
    private readonly ILogger<GameTickService> _logger;
    private readonly ChannelWriter<GameTick> _tickChannelWriter;
    
    // Game timing configuration
    private const int TICKS_PER_SECOND = 10; // 10 ticks per second = 100ms per tick

    public GameTickService(ILogger<GameTickService> logger, ChannelWriter<GameTick> tickChannelWriter)
    {
        _logger = logger;
        _tickChannelWriter = tickChannelWriter;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("GameTickService starting with {TicksPerSecond} ticks per second", TICKS_PER_SECOND);
        
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
                    _logger.LogWarning("Tick {TickNumber} took {TickDuration}ms, exceeding target interval of {TargetInterval}ms", 
                        tickNumber, tickDuration.TotalMilliseconds, tickInterval.TotalMilliseconds);
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("GameTickService cancellation requested after {TickCount} ticks", tickNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GameTickService main loop");
        }
        finally
        {
            _logger.LogInformation("GameTickService exiting");
        }
    }

    private async Task PublishTickAsync(long tickNumber, CancellationToken stoppingToken)
    {
        try
        {
            var tick = new GameTick
            {
                TickNumber = tickNumber,
                Timestamp = DateTime.UtcNow
            };

            await _tickChannelWriter.WriteAsync(tick, stoppingToken);
            
            _logger.LogDebug("Published tick {TickNumber}", tickNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish tick {TickNumber}", tickNumber);
        }
    }

    public ChannelReader<GameTick> GetTickReader()
    {
        // Get the channel reader from the DI container
        // This will be provided by Frank.Channels.DependencyInjection
        throw new NotImplementedException("Use ChannelReader<GameTick> from DI container instead of this method");
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("GameTickService stopping...");
        
        // Complete the channel writer
        _tickChannelWriter.Complete();
        
        await base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        _tickChannelWriter?.Complete();
        base.Dispose();
    }
} 