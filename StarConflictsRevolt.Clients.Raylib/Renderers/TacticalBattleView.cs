using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Windowing;

namespace StarConflictsRevolt.Clients.Raylib.Renderers;

public class TacticalBattleView : IView
{
    private readonly RenderContext _renderContext;

    public TacticalBattleView(RenderContext renderContext)
    {
        _renderContext = renderContext;
    }
    
    /// <inheritdoc />
    public GameView ViewType => GameView.TacticalBattle;
    
    /// <inheritdoc />
    public void Draw()
    {
        var currentWorld = _renderContext.World;
        if (currentWorld == null)
            return;

        // Basic tactical battle view
        Graphics.DrawText("Tactical Battle View", 10, 10, 20, Color.White);
        Graphics.DrawText("Battle in progress...", 10, 40, 16, Color.Yellow);
        
        // Draw battle area
        Graphics.DrawRectangle(50, 80, 700, 400, Color.DarkGray);
        Graphics.DrawRectangleLines(50, 80, 700, 400, Color.White);
        
        // Draw placeholder battle elements
        Graphics.DrawCircle(150, 280, 20, Color.Red);
        Graphics.DrawText("Fleet Alpha", 120, 310, 12, Color.White);
        
        Graphics.DrawCircle(350, 280, 20, Color.Blue);
        Graphics.DrawText("Fleet Beta", 320, 310, 12, Color.White);
        
        Graphics.DrawCircle(550, 280, 20, Color.Green);
        Graphics.DrawText("Fleet Gamma", 520, 310, 12, Color.White);
        
        // Draw battle lines
        Graphics.DrawLine(170, 280, 330, 280, Color.Yellow);
        Graphics.DrawLine(370, 280, 530, 280, Color.Yellow);
        
        // Draw controls
        Graphics.DrawText("Press ESC to return to strategic view", 10, 500, 16, Color.Gray);
        
        // Handle input
        if (Input.IsKeyPressed(KeyboardKey.Escape))
        {
            _renderContext.CurrentView = GameView.Galaxy;
        }
    }
} 