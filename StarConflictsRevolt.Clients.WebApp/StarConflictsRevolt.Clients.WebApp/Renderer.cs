using BlazorCanvas2d;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Clients.Shared;

namespace StarConflictsRevolt.Clients.WebApp;

public class Renderer : IGameRenderer
{
    private ICanvas? _canvas;
    
    public void SetCanvas(ICanvas canvas) => _canvas = canvas;
    
    /// <inheritdoc />
    public async Task<bool> RenderAsync(WorldDto? world, CancellationToken cancellationToken)
    {
        if (_canvas == null) return true;

        var context = _canvas.RenderContext;
        
        // Clear the canvas
        context.ClearRect(0, 0, _canvas.Width, _canvas.Height);
        // Draw the galaxy background
        if (world.Galaxy != null)
        {
            foreach (var star in world.Galaxy.StarSystems)
            {
                context.FillStyle = "yellow";
                context.BeginPath();
                context.Arc(star.Coordinates.X, star.Coordinates.Y, 5, 0, MathF.PI * 2);
                context.Fill();
            }
        }
        
        // Draw planets
        if (world.Galaxy?.StarSystems != null)
        {
        }
        
        
        
        
        
        return true;
    }
}