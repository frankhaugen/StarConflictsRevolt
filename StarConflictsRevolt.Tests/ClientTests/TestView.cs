using Microsoft.Extensions.Logging;
using NSubstitute;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Clients.Raylib.Core;
using StarConflictsRevolt.Clients.Raylib.Game.World;
using StarConflictsRevolt.Clients.Raylib.Rendering.Core;

namespace StarConflictsRevolt.Tests.ClientTests;

/// <summary>
/// Test view implementation for testing.
/// </summary>
public class TestView : BaseView
{
    public TestView() : base(null!)
    {
        // Create a minimal render context for testing
        var gameState = new GameState();
        var worldStore = Substitute.For<IClientWorldStore>();
        var context = new RenderContext(Substitute.For<ILogger<RenderContext>>(), 
            Microsoft.Extensions.Options.Options.Create(new GameClientConfiguration()), worldStore);
        Initialize(context);
    }
    
    public override GameView ViewType => GameView.Menu;
    
    public override void Draw()
    {
        // Simple test implementation
        // In a real view, this would use UIHelper or a renderer
    }
    
    public void Initialize(RenderContext context)
    {
        // Initialize the view with the context
    }
}