using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Clients.Shared;

namespace StarConflictsRevolt.Clients.Raylib;

public class RenderService
{
    private HubConnection? _hubConnection;
    private readonly IOptions<GameClientConfiguration> _gameClientConfiguration;
    private IClientWorldStore _worldStore;
    private IGameRenderer _gameRenderer;
    private CancellationTokenSource _cts = new();

    public RenderService(IOptions<GameClientConfiguration> gameClientConfiguration, IClientWorldStore worldStore, IGameRenderer gameRenderer)
    {
        _gameClientConfiguration = gameClientConfiguration;
        _worldStore = worldStore;
        _gameRenderer = gameRenderer;
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

        // start continuous rendering
        _ = Task.Run(() => RenderLoop(_cts.Token));
    }

    private async Task RenderLoop(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var world = _worldStore.GetCurrent();
                await _gameRenderer.RenderAsync(world, cancellationToken);
            }
            catch (InvalidOperationException)
            {
                // world not ready yet
            }
            await Task.Delay(1000 / 30, cancellationToken);
        }
    }

    public async Task StopAsync()
    {
        _cts.Cancel();
        if (_hubConnection != null)
        {
            await _hubConnection.StopAsync();
            await _hubConnection.DisposeAsync();
        }
    }

}