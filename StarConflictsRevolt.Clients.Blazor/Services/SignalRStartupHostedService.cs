using StarConflictsRevolt.Clients.Shared.Communication;

namespace StarConflictsRevolt.Clients.Blazor.Services;

/// <summary>
/// Starts the SignalR connection when the Blazor app starts so it is ready when the user creates or joins a session.
/// </summary>
public sealed class SignalRStartupHostedService : IHostedService
{
    private readonly ISignalRService _signalRService;
    private readonly ILogger<SignalRStartupHostedService> _logger;

    public SignalRStartupHostedService(ISignalRService signalRService, ILogger<SignalRStartupHostedService> logger)
    {
        _signalRService = signalRService;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting SignalR connection at app startup");
        try
        {
            await _signalRService.StartAsync(cancellationToken);
            _logger.LogInformation("SignalR connection started successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start SignalR connection at startup; join session may not receive live updates");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
