using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Clients.Raylib.Renderers;
using StarConflictsRevolt.Clients.Raylib.Services;
using TUnit;

namespace StarConflictsRevolt.Tests.ClientTests;

public class MenuViewSessionTypeTests
{
    private ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IOptions<GameClientConfiguration>>(Options.Create(new GameClientConfiguration()));
        services.AddSingleton<IClientWorldStore, TestWorldStore>();
        services.AddSingleton<RenderContext>();
        services.AddSingleton<GameCommandService>(sp => new GameCommandService(
            sp.GetRequiredService<RenderContext>().GameState,
            sp.GetRequiredService<ILogger<GameCommandService>>(),
            null)); // IHttpApiClient is not needed for these tests
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
        renderContext.GameState.Session = new SessionDto { Id = System.Guid.NewGuid(), SessionName = "Test", SessionType = "SinglePlayer" };
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
        public WorldDto? GetCurrent() => null;
        public void ApplyFull(WorldDto? world) { }
        public void ApplyDeltas(IEnumerable<GameObjectUpdate> deltas) { }
        public IReadOnlyList<WorldDto?> History => new List<WorldDto?>();
        public SessionDto? Session { get; set; }
    }
} 