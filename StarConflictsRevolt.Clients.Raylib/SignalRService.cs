using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Clients.Shared;

namespace StarConflictsRevolt.Clients.Raylib;

public class SignalRService : IAsyncDisposable
{
    private HubConnection? _hubConnection;
    private readonly IOptions<GameClientConfiguration> _gameClientConfiguration;
    private IClientWorldStore _worldStore;
    private IGameRenderer _gameRenderer;
    private CancellationTokenSource _cts = new();

    public SignalRService(IOptions<GameClientConfiguration> gameClientConfiguration, IClientWorldStore worldStore)
    {
        _gameClientConfiguration = gameClientConfiguration;
        _worldStore = worldStore;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(_gameClientConfiguration.Value.GameServerHubUrl)
            .WithAutomaticReconnect()
            .ConfigureLogging(logging =>
            {
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
            })
            .Build();
        
        _hubConnection.On<WorldDto>("FullWorld", worldDto =>
        {
            _worldStore.ApplyFull(worldDto);
        });
        
        _hubConnection.On<List<GameObjectUpdate>>("ReceiveUpdates", updates =>
        {
            _worldStore.ApplyDeltas(updates);
        });

        try
        {
            await _hubConnection.StartAsync(_cts.Token);
            // join the world group
            await _hubConnection.SendAsync("JoinWorld", "world-1", _cts.Token);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error connecting to game hub: {ex.Message}");
        }
    }

    public async Task StopAsync()
    {
        await _cts.CancelAsync();
        if (_hubConnection != null)
        {
            await _hubConnection.StopAsync();
            await _hubConnection.DisposeAsync();
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_hubConnection != null) await _hubConnection.DisposeAsync();
        if (_cts is IAsyncDisposable ctsAsyncDisposable)
            await ctsAsyncDisposable.DisposeAsync();
        else
            _cts.Dispose();
    }
}