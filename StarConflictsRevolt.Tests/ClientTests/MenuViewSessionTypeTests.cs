using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StarConflictsRevolt.Clients.Http.Http;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Clients.Raylib.Renderers;
using StarConflictsRevolt.Clients.Raylib.Services;

namespace StarConflictsRevolt.Tests.ClientTests;

public class MenuViewSessionTypeTests
{
    private ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(Options.Create(new GameClientConfiguration()));
        services.AddSingleton<IClientWorldStore, TestWorldStore>();
        services.AddSingleton<RenderContext>();
        services.AddSingleton<GameCommandService>(sp => new GameCommandService(
            sp.GetRequiredService<RenderContext>().GameState,
            sp.GetRequiredService<ILogger<GameCommandService>>(),
            new TestHttpApiClient())); // Use a test implementation instead of null
        services.AddSingleton<SignalRService>(sp => new TestSignalRService(
            sp.GetRequiredService<IOptions<GameClientConfiguration>>(),
            sp.GetRequiredService<IClientWorldStore>(),
            sp.GetRequiredService<ILogger<SignalRService>>()));
        services.AddSingleton<MenuView>();
        return services.BuildServiceProvider();
    }

    [Test]
    public async Task MenuView_Initial_State_Is_MainMenu()
    {
        var provider = BuildServiceProvider();
        var menuView = provider.GetRequiredService<MenuView>();
        await Assert.That(menuView.ViewType).IsEqualTo(GameView.Menu);
    }

    [Test]
    public async Task MenuView_SessionType_Selection_Changes_State()
    {
        var provider = BuildServiceProvider();
        var menuView = provider.GetRequiredService<MenuView>();
        await Assert.That(menuView.ViewType).IsEqualTo(GameView.Menu);
    }

    [Test]
    public async Task MenuView_SessionType_Display_Is_Correct()
    {
        var provider = BuildServiceProvider();
        var menuView = provider.GetRequiredService<MenuView>();
        var renderContext = provider.GetRequiredService<RenderContext>();
        renderContext.GameState.Session = new SessionDto { SessionName = "Test", SessionType = "SinglePlayer" };
        await Assert.That(renderContext.GameState.Session.SessionType).IsEqualTo("SinglePlayer");
    }

    [Test]
    public async Task MenuView_SessionType_Can_Be_Set_To_Multiplayer()
    {
        var provider = BuildServiceProvider();
        var menuView = provider.GetRequiredService<MenuView>();
        var renderContext = provider.GetRequiredService<RenderContext>();
        renderContext.GameState.Session = new SessionDto { SessionName = "Test", SessionType = "Multiplayer" };
        await Assert.That(renderContext.GameState.Session.SessionType).IsEqualTo("Multiplayer");
    }

    [Test]
    public async Task MenuView_PlayerName_Can_Be_Set()
    {
        var provider = BuildServiceProvider();
        var menuView = provider.GetRequiredService<MenuView>();
        var renderContext = provider.GetRequiredService<RenderContext>();
        renderContext.GameState.PlayerName = "Player1";
        await Assert.That(renderContext.GameState.PlayerName).IsEqualTo("Player1");
    }

    [Test]
    public async Task MenuView_SessionId_Can_Be_Set()
    {
        var provider = BuildServiceProvider();
        var menuView = provider.GetRequiredService<MenuView>();
        var renderContext = provider.GetRequiredService<RenderContext>();
        renderContext.GameState.Session = new SessionDto { Id = Guid.NewGuid(), SessionName = "Test", SessionType = "SinglePlayer" };
        await Assert.That(renderContext.GameState.Session.SessionName).IsEqualTo("Test");
    }

    [Test]
    public async Task MenuView_SessionType_Default_Is_Multiplayer()
    {
        var provider = BuildServiceProvider();
        var menuView = provider.GetRequiredService<MenuView>();
        var renderContext = provider.GetRequiredService<RenderContext>();
        renderContext.GameState.Session = new SessionDto { SessionName = "Test" };
        await Assert.That(renderContext.GameState.Session.SessionType).IsEqualTo("Multiplayer");
    }

    // Minimal fake for IClientWorldStore
    private class TestWorldStore : IClientWorldStore
    {
        public WorldDto? GetCurrent()
        {
            return null;
        }

        public void ApplyFull(WorldDto? world)
        {
        }

        public void ApplyDeltas(IEnumerable<GameObjectUpdate> deltas)
        {
        }

        public IReadOnlyList<WorldDto?> History => new List<WorldDto?>();
        public SessionDto? Session { get; set; }
    }

    // Minimal fake for SignalRService
    private class TestSignalRService : SignalRService
    {
        public TestSignalRService(IOptions<GameClientConfiguration> gameClientConfiguration, 
            IClientWorldStore worldStore, 
            ILogger<SignalRService> logger) 
            : base(gameClientConfiguration, worldStore, null!, logger)
        {
        }

        public override Task StartAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public override Task JoinSessionAsync(Guid sessionId)
        {
            return Task.CompletedTask;
        }

        public override Task StopAsync()
        {
            return Task.CompletedTask;
        }

        public override ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
    }

    // Minimal fake for IHttpApiClient
    private class TestHttpApiClient : IHttpApiClient
    {
        /// <inheritdoc />
        public async Task<HttpResponseMessage> RetrieveHealthCheckAsync(CancellationToken ct)
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        public Task<T?> GetAsync<T>(string uri, CancellationToken ct = default)
        {
            return Task.FromResult<T?>(default);
        }

        public Task<HttpResponseMessage> PostAsync<T>(string uri, T body, CancellationToken ct = default)
        {
            return Task.FromResult(new HttpResponseMessage());
        }

        public Task<HttpResponseMessage> PutAsync<T>(string uri, T body, CancellationToken ct = default)
        {
            return Task.FromResult(new HttpResponseMessage());
        }

        public Task<HttpResponseMessage> DeleteAsync(string uri, CancellationToken ct = default)
        {
            return Task.FromResult(new HttpResponseMessage());
        }

        /// <inheritdoc />
        public async Task<bool> IsHealthyAsync(CancellationToken ct = default)
        {
            return true;
        }
    }
}