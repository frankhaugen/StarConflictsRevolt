using Raylib_CSharp.Colors;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Windowing;
using StarConflictsRevolt.Clients.Shared;
using StarConflictsRevolt.Clients.Models;

namespace StarConflictsRevolt.Clients.Raylib.Renderers;

public class FleetFinderView : IView
{
    private readonly RenderContext _renderContext;
    private int _scrollOffset = 0;
    public FleetFinderView(RenderContext renderContext)
    {
        _renderContext = renderContext;
    }
    public GameView ViewType => (GameView)1001;
    public void Draw()
    {
        var world = _renderContext.World;
        Graphics.ClearBackground(Color.Black);
        Graphics.DrawText("Fleet Finder", 10, 10, 28, Color.RayWhite);
        if (world?.Galaxy?.StarSystems == null)
        {
            Graphics.DrawText("No world data.", 10, 50, 20, Color.LightGray);
            return;
        }
        var fleets = new List<string>();
        foreach (var system in world.Galaxy.StarSystems)
        {
            foreach (var planet in system.Planets)
            {
                // If fleets are present in PlanetDto, list them
                // (Stub: just show planet name as fleet placeholder)
                fleets.Add($"Fleet at {planet.Name}");
            }
        }
        if (fleets.Count == 0)
        {
            Graphics.DrawText("No fleets found.", 10, 50, 20, Color.LightGray);
            return;
        }
        int y = 50;
        int maxVisible = (Window.GetScreenHeight() - 60) / 24;
        for (int i = _scrollOffset; i < Math.Min(fleets.Count, _scrollOffset + maxVisible); i++)
        {
            Graphics.DrawText(fleets[i], 10, y, 22, Color.RayWhite);
            y += 24;
        }
        if (fleets.Count > maxVisible)
        {
            if (Raylib_CSharp.Interact.Input.IsKeyPressed(Raylib_CSharp.Interact.KeyboardKey.Down) && _scrollOffset < fleets.Count - maxVisible)
                _scrollOffset++;
            if (Raylib_CSharp.Interact.Input.IsKeyPressed(Raylib_CSharp.Interact.KeyboardKey.Up) && _scrollOffset > 0)
                _scrollOffset--;
        }
    }
} 