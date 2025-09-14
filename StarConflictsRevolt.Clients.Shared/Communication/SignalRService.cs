using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StarConflictsRevolt.Clients.Models;

namespace StarConflictsRevolt.Clients.Shared.Communication;

/// <summary>
/// Shared SignalR service implementation that can be used by both Raylib and Bliss clients.
/// This version uses events instead of direct world store dependencies.
/// </summary>
public class SignalRService : ISignalRService
{
    private readonly IOptions<GameClientConfiguration> _gameClientConfiguration;
    private readonly ILogger<SignalRService> _logger;
    private CancellationTokenSource _cts = new();
    private Guid? _currentSessionId;
    private HubConnection? _hubConnection;

    public SignalRService(IOptions<GameClientConfiguration> gameClientConfiguration,
        ILogger<SignalRService> logger)
    {
        _gameClientConfiguration = gameClientConfiguration;
        _logger = logger;
        _logger.LogInformation("SignalRService initialized");
    }

    /// <summary>
    /// Event that is raised when a full world update is received
    /// </summary>
    public event Action<WorldDto>? FullWorldReceived;

    /// <summary>
    /// Event that is raised when delta updates are received
    /// </summary>
    public event Action<List<GameObjectUpdate>>? UpdatesReceived;

    /// <summary>
    /// Event that is raised when the connection is closed
    /// </summary>
    public event Action<Exception?>? ConnectionClosed;

    /// <summary>
    /// Event that is raised when the connection is reconnecting
    /// </summary>
    public event Action<Exception?>? Reconnecting;

    /// <summary>
    /// Event that is raised when the connection is reconnected
    /// </summary>
    public event Action<string>? Reconnected;

    /// <inheritdoc />
    public virtual async ValueTask DisposeAsync()
    {
        _logger.LogInformation("Disposing SignalR service");

        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
            _logger.LogInformation("SignalR connection disposed");
        }

        if (_cts is IAsyncDisposable ctsAsyncDisposable)
            await ctsAsyncDisposable.DisposeAsync();
        else
            _cts.Dispose();

        _logger.LogInformation("SignalR service disposed");
    }

    public virtual async Task StartAsync(CancellationToken cancellationToken = default)
    {
        // Use service discovery for the GameEngine service (SignalR is in GameEngine)
        var hubUrl = _gameClientConfiguration.Value.GameServerHubUrl;
        _logger.LogInformation("Starting SignalR connection to: {HubUrl}", hubUrl);

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .ConfigureLogging(logging =>
            {
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
            })
            .Build();

        cancellationToken.Register(async () =>
        {
            _logger.LogInformation("Cancellation requested, stopping SignalR service");
            await _cts.CancelAsync();
            await (_hubConnection?.StopAsync(cancellationToken) ?? Task.CompletedTask);
        });

        // Register message handlers
        _hubConnection.On<WorldDto>("FullWorld", worldDto =>
        {
            _logger.LogInformation("Received FullWorld message. WorldId: {WorldId}", worldDto?.Id);
            _logger.LogDebug("FullWorld contains {StarSystemCount} star systems",
                worldDto?.Galaxy?.StarSystems?.Count() ?? 0);
            if (worldDto != null)
                FullWorldReceived?.Invoke(worldDto);
        });

        _hubConnection.On<List<GameObjectUpdate>>("ReceiveUpdates", updates =>
        {
            _logger.LogInformation("Received {UpdateCount} updates via SignalR", updates?.Count ?? 0);
            if (updates != null)
                foreach (var update in updates)
                    _logger.LogDebug("Update: Id={Id}, Type={Type}", update.Id, update.Type);

            UpdatesReceived?.Invoke(updates ?? new List<GameObjectUpdate>());
        });

        // Register connection event handlers
        _hubConnection.Closed += exception =>
        {
            _logger.LogWarning(exception, "SignalR connection closed");
            ConnectionClosed?.Invoke(exception);
            return Task.CompletedTask;
        };

        _hubConnection.Reconnecting += exception =>
        {
            _logger.LogInformation(exception, "SignalR reconnecting");
            Reconnecting?.Invoke(exception);
            return Task.CompletedTask;
        };

        _hubConnection.Reconnected += connectionId =>
        {
            _logger.LogInformation("SignalR reconnected with connection ID: {ConnectionId}", connectionId);
            Reconnected?.Invoke(connectionId);
            return Task.CompletedTask;
        };

        try
        {
            await StartHubConnectionAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start SignalR connection. Retrying...");

            var continueRetry = true;
            while (continueRetry && !_cts.IsCancellationRequested)
                try
                {
                    _logger.LogInformation("Retrying SignalR connection...");
                    await Task.Delay(2000, _cts.Token); // wait before retrying
                    await StartHubConnectionAsync();
                    continueRetry = false; // success, exit loop
                }
                catch (Exception retryEx)
                {
                    _logger.LogError(retryEx, "Retry failed. Will retry again if not cancelled");
                }
        }
    }

    public virtual async Task JoinSessionAsync(Guid sessionId)
    {
        if (_hubConnection?.State != HubConnectionState.Connected)
        {
            _logger.LogWarning("Cannot join session {SessionId}: SignalR connection not established", sessionId);
            return;
        }

        _currentSessionId = sessionId;
        _logger.LogInformation("Joining world group for session: {SessionId}", sessionId);
        await _hubConnection.SendAsync("JoinWorld", sessionId.ToString(), _cts.Token);
        _logger.LogInformation("Successfully joined world group for session {SessionId}", sessionId);
    }

    private async Task StartHubConnectionAsync()
    {
        _logger.LogInformation("Attempting to start SignalR connection");
        await _hubConnection!.StartAsync(_cts.Token);
        _logger.LogInformation("SignalR connection started successfully");

        // Don't join any world group here - wait for explicit session join
        _logger.LogInformation("SignalR connection ready - waiting for session join");
    }

    public virtual async Task StopAsync()
    {
        _logger.LogInformation("Stopping SignalR service");
        await _cts.CancelAsync();

        if (_hubConnection != null)
        {
            _logger.LogInformation("Stopping SignalR connection");
            await _hubConnection.StopAsync();
            await _hubConnection.DisposeAsync();
            _logger.LogInformation("SignalR connection stopped and disposed");
        }
    }
} 