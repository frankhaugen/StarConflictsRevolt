using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Clients.Shared;

namespace StarConflictsRevolt.Clients.Raylib;

public class SignalRService : IAsyncDisposable
{
    private HubConnection? _hubConnection;
    private readonly IOptions<GameClientConfiguration> _gameClientConfiguration;
    private readonly IClientWorldStore _worldStore;
    private readonly IGameRenderer _gameRenderer;
    private readonly ILogger<SignalRService> _logger;
    private CancellationTokenSource _cts = new();

    public SignalRService(IOptions<GameClientConfiguration> gameClientConfiguration, 
        IClientWorldStore worldStore, 
        IGameRenderer gameRenderer,
        ILogger<SignalRService> logger)
    {
        _gameClientConfiguration = gameClientConfiguration;
        _worldStore = worldStore;
        _gameRenderer = gameRenderer;
        _logger = logger;
        _logger.LogInformation("SignalRService initialized");
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
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
        
        // Register message handlers
        _hubConnection.On<WorldDto>("FullWorld", worldDto =>
        {
            _logger.LogInformation("Received FullWorld message. WorldId: {WorldId}", worldDto?.Id);
            _logger.LogDebug("FullWorld contains {StarSystemCount} star systems", 
                worldDto?.Galaxy?.StarSystems?.Count() ?? 0);
            _worldStore.ApplyFull(worldDto);
        });
        
        _hubConnection.On<List<GameObjectUpdate>>("ReceiveUpdates", updates =>
        {
            _logger.LogInformation("Received {UpdateCount} updates via SignalR", updates?.Count ?? 0);
            if (updates != null)
            {
                foreach (var update in updates)
                {
                    _logger.LogDebug("Update: Id={Id}, Type={Type}", update.Id, update.Type);
                }
            }
            _worldStore.ApplyDeltas(updates ?? new List<GameObjectUpdate>());
        });

        // Register connection event handlers
        _hubConnection.Closed += (exception) =>
        {
            _logger.LogWarning(exception, "SignalR connection closed");
            return Task.CompletedTask;
        };

        _hubConnection.Reconnecting += (exception) =>
        {
            _logger.LogInformation(exception, "SignalR reconnecting");
            return Task.CompletedTask;
        };

        _hubConnection.Reconnected += (connectionId) =>
        {
            _logger.LogInformation("SignalR reconnected with connection ID: {ConnectionId}", connectionId);
            return Task.CompletedTask;
        };

        try
        {
            _logger.LogInformation("Attempting to start SignalR connection");
            await _hubConnection.StartAsync(_cts.Token);
            _logger.LogInformation("SignalR connection started successfully");
            
            // join the world group
            _logger.LogInformation("Joining world group: world-1");
            await _hubConnection.SendAsync("JoinWorld", "world-1", _cts.Token);
            _logger.LogInformation("Successfully joined world group");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting to game hub");
            Console.WriteLine($"Error connecting to game hub: {ex.Message}");
        }
    }

    public async Task StopAsync()
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

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        _logger.LogInformation("Disposing SignalR service");
        
        if (_hubConnection != null) 
        {
            await _hubConnection.DisposeAsync();
            _logger.LogInformation("SignalR connection disposed");
        }
        
        if (_cts is IAsyncDisposable ctsAsyncDisposable)
        {
            await ctsAsyncDisposable.DisposeAsync();
        }
        else
        {
            _cts.Dispose();
        }
        
        _logger.LogInformation("SignalR service disposed");
    }
}