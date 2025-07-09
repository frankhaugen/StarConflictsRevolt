using Raylib_CSharp.Colors;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Windowing;

namespace StarConflictsRevolt.Clients.Raylib.Renderers;

public class PlanetaryFinderView : IView
{
    private readonly RenderContext _renderContext;
    private int _scrollOffset = 0;
    public PlanetaryFinderView(RenderContext renderContext)
    {
        _renderContext = renderContext;
    }
    public GameView ViewType => (GameView)1003;
    public void Draw()
    {
        var world = _renderContext.World;
        Graphics.ClearBackground(Color.Black);
        Graphics.DrawText("Planetary Finder", 10, 10, 28, Color.RayWhite);
        if (world?.Galaxy?.StarSystems == null)
        {
            Graphics.DrawText("No world data.", 10, 50, 20, Color.LightGray);
            return;
        }
        var planets = new List<string>();
        foreach (var system in world.Galaxy.StarSystems)
        {
            foreach (var planet in system.Planets)
            {
                planets.Add($"{planet.Name} (System: {system.Name})");
            }
        }
        if (planets.Count == 0)
        {
            Graphics.DrawText("No planets found.", 10, 50, 20, Color.LightGray);
            return;
        }
        int y = 50;
        int maxVisible = (Window.GetScreenHeight() - 60) / 24;
        for (int i = _scrollOffset; i < Math.Min(planets.Count, _scrollOffset + maxVisible); i++)
        {
            Graphics.DrawText(planets[i], 10, y, 22, Color.RayWhite);
            y += 24;
        }
        if (planets.Count > maxVisible)
        {
            if (Raylib_CSharp.Interact.Input.IsKeyPressed(Raylib_CSharp.Interact.KeyboardKey.Down) && _scrollOffset < planets.Count - maxVisible)
                _scrollOffset++;
            if (Raylib_CSharp.Interact.Input.IsKeyPressed(Raylib_CSharp.Interact.KeyboardKey.Up) && _scrollOffset > 0)
                _scrollOffset--;
        }
    }
} 