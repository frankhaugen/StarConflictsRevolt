using System.Text.Json.Serialization;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StarConflictsRevolt.Clients.Models;

namespace StarConflictsRevolt.Clients.Shared;

public class GameClient
{
    readonly HubConnection _hub;
    readonly IClientWorldStore _store;
    readonly IGameRenderer _renderer;
    readonly CancellationTokenSource _cts = new();

    public GameClient(IOptions<GameClientConfiguration> options,
        IClientWorldStore store, 
        IGameRenderer renderer)
    {
        _store = store;
        _renderer = renderer;
        
        var config = options.Value;
        var hubUrl = config.GameServerHubUrl;

        _hub = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .AddJsonProtocol(opts =>
                opts.PayloadSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)
            .WithAutomaticReconnect()
            .Build();

        _hub.On<WorldDto>("FullWorld", store.ApplyFull);
        _hub.On<List<GameObjectUpdate>>("ReceiveUpdates", store.ApplyDeltas);
    }

    public async Task RunAsync()
    {
        await _hub.StartAsync(_cts.Token);
        await _hub.SendAsync("JoinWorld", "world-1", _cts.Token);
        _ = Task.Run(RenderLoop, _cts.Token);
    }

    async Task RenderLoop()
    {
        while (!_cts.IsCancellationRequested)
        {
            try
            {
                var world = _store.GetCurrent();
                await _renderer.RenderAsync(world, _cts.Token);
            }
            catch (InvalidOperationException) { /* world not ready */ }

            await Task.Delay(1000 / 30, _cts.Token);
        }
    }

    public async Task StopAsync()
    {
        _cts.Cancel();
        await _hub.StopAsync();
    }
}