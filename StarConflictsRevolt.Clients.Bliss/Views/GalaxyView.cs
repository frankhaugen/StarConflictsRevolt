using Bliss.CSharp.Colors;
using Bliss.CSharp.Graphics.Rendering.Batches.Primitives;
using Bliss.CSharp.Graphics.Rendering.Batches.Sprites;
using Bliss.CSharp.Graphics.Rendering.Renderers;
using Bliss.CSharp.Transformations;
using StarConflictsRevolt.Clients.Bliss.Core;
using System.Numerics;
using Veldrid;
using Color = Bliss.CSharp.Colors.Color;
using RectangleF = Bliss.CSharp.Transformations.RectangleF;

namespace StarConflictsRevolt.Clients.Bliss.Views;

/// <summary>
/// Main galaxy view showing the star map with sectors, planets, and fleets.
/// </summary>
public class GalaxyView : GameView
{
    private readonly List<StarSystem> _starSystems = new();
    private readonly List<Fleet> _fleets = new();
    private Vector2 _cameraPosition = Vector2.Zero;
    private float _zoom = 1.0f;
    private float _time = 0f;
    
    public GalaxyView() : base("Galaxy Overview")
    {
        InitializeStarSystems();
        InitializeFleets();
    }
    
    public override void Update(float deltaTime)
    {
        _time += deltaTime;
        
        // Update fleet positions (simple orbital movement)
        foreach (var fleet in _fleets)
        {
            fleet.Update(deltaTime);
        }
    }
    
    public override void Render(ImmediateRenderer immediateRenderer, PrimitiveBatch primitiveBatch, 
                               SpriteBatch spriteBatch, CommandList commandList, Framebuffer framebuffer)
    {
        // Draw starfield background
        DrawStarfield(primitiveBatch, commandList, framebuffer);
        
        // Draw sector grid
        DrawSectorGrid(primitiveBatch, commandList, framebuffer);
        
        // Draw star systems
        DrawStarSystems(primitiveBatch, commandList, framebuffer);
        
        // Draw fleets
        DrawFleets(primitiveBatch, commandList, framebuffer);
        
        // Draw UI overlay
        DrawUIOverlay(primitiveBatch, commandList, framebuffer);
    }
    
    private void DrawStarfield(PrimitiveBatch primitiveBatch, CommandList commandList, Framebuffer framebuffer)
    {
        primitiveBatch.Begin(commandList, framebuffer.OutputDescription);
        
        // Draw distant stars
        for (int i = 0; i < 200; i++)
        {
            var x = (i * 12345) % 1920;
            var y = (i * 67890) % 1080;
            var brightness = (i % 3) switch
            {
                0 => StarWarsTheme.StarDistant,
                1 => StarWarsTheme.StarDim,
                _ => StarWarsTheme.StarBright
            };
            
            primitiveBatch.DrawFilledCircle(
                new Vector2(x, y), 
                1 + (i % 2), 
                8, 
                0.5f, 
                brightness);
        }
        
        primitiveBatch.End();
    }
    
    private void DrawSectorGrid(PrimitiveBatch primitiveBatch, CommandList commandList, Framebuffer framebuffer)
    {
        primitiveBatch.Begin(commandList, framebuffer.OutputDescription);
        
        // Draw sector boundaries
        for (int x = 0; x <= 1920; x += 200)
        {
            primitiveBatch.DrawLine(
                new Vector2(x, 0), 
                new Vector2(x, 1080), 
                1f, 
                StarWarsTheme.Border);
        }
        
        for (int y = 0; y <= 1080; y += 200)
        {
            primitiveBatch.DrawLine(
                new Vector2(0, y), 
                new Vector2(1920, y), 
                1f, 
                StarWarsTheme.Border);
        }
        
        primitiveBatch.End();
    }
    
    private void DrawStarSystems(PrimitiveBatch primitiveBatch, CommandList commandList, Framebuffer framebuffer)
    {
        primitiveBatch.Begin(commandList, framebuffer.OutputDescription);
        
        foreach (var system in _starSystems)
        {
            // Draw star
            var starColor = system.Type switch
            {
                StarType.Yellow => new Color(255, 255, 204, 255),
                StarType.Red => new Color(255, 153, 102, 255),
                StarType.Blue => new Color(153, 204, 255, 255),
                _ => new Color(255, 255, 255, 255)
            };
            
            primitiveBatch.DrawFilledCircle(
                system.Position, 
                system.Size, 
                16, 
                0.5f, 
                starColor);
            
            // Draw star glow effect
            primitiveBatch.DrawFilledCircle(
                system.Position, 
                system.Size + 5, 
                16, 
                0.2f, 
                starColor);
            
            // Draw planet if present
            if (system.HasPlanet)
            {
                var planetColor = system.PlanetFaction switch
                {
                    Faction.Empire => StarWarsTheme.EmpirePrimary,
                    Faction.Alliance => StarWarsTheme.RebellionPrimary,
                    _ => StarWarsTheme.Neutral
                };
                
                primitiveBatch.DrawFilledCircle(
                    system.Position + new Vector2(system.Size + 15, 0), 
                    8, 
                    12, 
                    0.8f, 
                    planetColor);
            }
        }
        
        primitiveBatch.End();
    }
    
    private void DrawFleets(PrimitiveBatch primitiveBatch, CommandList commandList, Framebuffer framebuffer)
    {
        primitiveBatch.Begin(commandList, framebuffer.OutputDescription);
        
        foreach (var fleet in _fleets)
        {
            var fleetColor = fleet.Faction switch
            {
                Faction.Empire => StarWarsTheme.EmpireAccent,
                Faction.Alliance => StarWarsTheme.RebellionAccent,
                _ => StarWarsTheme.Neutral
            };
            
            // Draw fleet as a triangle pointing in movement direction
            var points = new Vector2[3];
            var angle = fleet.MovementDirection;
            
            points[0] = fleet.Position + new Vector2(
                (float)Math.Cos(angle) * 15,
                (float)Math.Sin(angle) * 15);
            
            points[1] = fleet.Position + new Vector2(
                (float)Math.Cos(angle + 2.5f) * 8,
                (float)Math.Sin(angle + 2.5f) * 8);
            
            points[2] = fleet.Position + new Vector2(
                (float)Math.Cos(angle - 2.5f) * 8,
                (float)Math.Sin(angle - 2.5f) * 8);
            
            primitiveBatch.DrawFilledTriangle(points[0], points[1], points[2], fleetColor);
            
            // Draw fleet glow
            primitiveBatch.DrawFilledCircle(
                fleet.Position, 
                20, 
                12, 
                0.3f, 
                fleetColor);
        }
        
        primitiveBatch.End();
    }
    
    private void DrawUIOverlay(PrimitiveBatch primitiveBatch, CommandList commandList, Framebuffer framebuffer)
    {
        primitiveBatch.Begin(commandList, framebuffer.OutputDescription);
        
        // Draw top panel
        primitiveBatch.DrawFilledRectangle(
            new RectangleF(0, 0, 1920, 60), 
            Vector2.Zero, 
            0f, 
            StarWarsTheme.PanelBackground);
        
        // Draw bottom panel
        primitiveBatch.DrawFilledRectangle(
            new RectangleF(0, 1020, 1920, 60), 
            Vector2.Zero, 
            0f, 
            StarWarsTheme.PanelBackground);
        
        // Draw side panel
        primitiveBatch.DrawFilledRectangle(
            new RectangleF(1720, 60, 200, 960), 
            Vector2.Zero, 
            0f, 
            StarWarsTheme.PanelBackground);
        
        primitiveBatch.End();
    }
    
    private void InitializeStarSystems()
    {
        // Create some sample star systems
        _starSystems.AddRange(new[]
        {
            new StarSystem("Coruscant", new Vector2(200, 200), StarType.Yellow, true, Faction.Empire),
            new StarSystem("Naboo", new Vector2(400, 300), StarType.Yellow, true, Faction.Alliance),
            new StarSystem("Tatooine", new Vector2(600, 150), StarType.Red, true, Faction.Neutral),
            new StarSystem("Hoth", new Vector2(300, 500), StarType.Blue, true, Faction.Alliance),
            new StarSystem("Endor", new Vector2(700, 400), StarType.Yellow, true, Faction.Empire),
            new StarSystem("Alderaan", new Vector2(500, 600), StarType.Yellow, true, Faction.Alliance),
            new StarSystem("Bespin", new Vector2(800, 250), StarType.Blue, false, Faction.Neutral),
            new StarSystem("Dagobah", new Vector2(150, 400), StarType.Yellow, false, Faction.Neutral),
        });
    }
    
    private void InitializeFleets()
    {
        // Create some sample fleets
        _fleets.AddRange(new[]
        {
            new Fleet("Imperial Fleet Alpha", new Vector2(250, 220), Faction.Empire),
            new Fleet("Rebel Fleet Beta", new Vector2(420, 320), Faction.Alliance),
            new Fleet("Imperial Fleet Gamma", new Vector2(720, 420), Faction.Empire),
            new Fleet("Rebel Fleet Delta", new Vector2(320, 520), Faction.Alliance),
        });
    }
}

/// <summary>
/// Represents a star system in the galaxy.
/// </summary>
public class StarSystem
{
    public string Name { get; }
    public Vector2 Position { get; }
    public StarType Type { get; }
    public bool HasPlanet { get; }
    public Faction PlanetFaction { get; }
    public float Size { get; }
    
    public StarSystem(string name, Vector2 position, StarType type, bool hasPlanet, Faction planetFaction)
    {
        Name = name;
        Position = position;
        Type = type;
        HasPlanet = hasPlanet;
        PlanetFaction = planetFaction;
        Size = 12 + (type switch { StarType.Blue => 4, StarType.Red => 2, _ => 0 });
    }
}

/// <summary>
/// Represents a fleet in the galaxy.
/// </summary>
public class Fleet
{
    public string Name { get; }
    public Vector2 Position { get; private set; }
    public Faction Faction { get; }
    public float MovementDirection { get; private set; }
    public Vector2 TargetPosition { get; private set; }
    
    public Fleet(string name, Vector2 position, Faction faction)
    {
        Name = name;
        Position = position;
        Faction = faction;
        MovementDirection = 0f;
        TargetPosition = position;
    }
    
    public void Update(float deltaTime)
    {
        // Simple orbital movement around the current position
        MovementDirection += deltaTime * 0.5f;
        
        // Add some gentle movement
        var time = (float)DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond * 0.001f;
        var offset = new Vector2(
            (float)Math.Sin(time * 0.3f) * 20,
            (float)Math.Cos(time * 0.2f) * 15);
        
        Position = TargetPosition + offset;
    }
}

/// <summary>
/// Types of stars in the galaxy.
/// </summary>
public enum StarType
{
    Yellow,
    Red,
    Blue
} 