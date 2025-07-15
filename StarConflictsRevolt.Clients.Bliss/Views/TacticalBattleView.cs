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
/// Tactical battle view showing ship-to-ship combat.
/// </summary>
public class TacticalBattleView : GameView
{
    private readonly List<CombatShip> _imperialShips = new();
    private readonly List<CombatShip> _rebelShips = new();
    private readonly List<Projectile> _projectiles = new();
    private float _time = 0f;
    private Vector2 _cameraPosition = Vector2.Zero;
    
    public TacticalBattleView() : base("Tactical Battle")
    {
        InitializeShips();
    }
    
    public override void Update(float deltaTime)
    {
        _time += deltaTime;
        
        // Update ships
        foreach (var ship in _imperialShips.Concat<CombatShip>(_rebelShips))
        {
            ship.Update(deltaTime);
        }
        
        // Update projectiles
        for (int i = _projectiles.Count - 1; i >= 0; i--)
        {
            var projectile = _projectiles[i];
            projectile.Update(deltaTime);
            
            // Remove projectiles that are out of bounds or have hit something
            if (projectile.Position.X < 0 || projectile.Position.X > 1920 ||
                projectile.Position.Y < 0 || projectile.Position.Y > 1080 ||
                projectile.Lifetime <= 0)
            {
                _projectiles.RemoveAt(i);
            }
        }
        
        // Simple AI: ships fire at nearest enemy
        UpdateCombat(deltaTime);
    }
    
    public override void Render(ImmediateRenderer immediateRenderer, PrimitiveBatch primitiveBatch, 
                               SpriteBatch spriteBatch, CommandList commandList, Framebuffer framebuffer)
    {
        // Draw space background
        DrawSpaceBackground(primitiveBatch, commandList, framebuffer);
        
        // Draw tactical grid
        DrawTacticalGrid(primitiveBatch, commandList, framebuffer);
        
        // Draw projectiles
        DrawProjectiles(primitiveBatch, commandList, framebuffer);
        
        // Draw ships
        DrawShips(primitiveBatch, commandList, framebuffer);
        
        // Draw UI overlay
        DrawBattleUI(primitiveBatch, commandList, framebuffer);
    }
    
    private void DrawSpaceBackground(PrimitiveBatch primitiveBatch, CommandList commandList, Framebuffer framebuffer)
    {
        primitiveBatch.Begin(commandList, framebuffer.OutputDescription);
        
        // Draw distant stars
        for (int i = 0; i < 100; i++)
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
                1, 
                8, 
                0.5f, 
                brightness);
        }
        
        primitiveBatch.End();
    }
    
    private void DrawTacticalGrid(PrimitiveBatch primitiveBatch, CommandList commandList, Framebuffer framebuffer)
    {
        primitiveBatch.Begin(commandList, framebuffer.OutputDescription);
        
        // Draw tactical grid lines
        for (int x = 0; x <= 1920; x += 100)
        {
            primitiveBatch.DrawLine(
                new Vector2(x, 0), 
                new Vector2(x, 1080), 
                0.5f, 
                new Color(51, 51, 77, 128));
        }
        
        for (int y = 0; y <= 1080; y += 100)
        {
            primitiveBatch.DrawLine(
                new Vector2(0, y), 
                new Vector2(1920, y), 
                0.5f, 
                new Color(51, 51, 77, 128));
        }
        
        primitiveBatch.End();
    }
    
    private void DrawShips(PrimitiveBatch primitiveBatch, CommandList commandList, Framebuffer framebuffer)
    {
        primitiveBatch.Begin(commandList, framebuffer.OutputDescription);
        
        // Draw Imperial ships
        foreach (var ship in _imperialShips)
        {
            DrawShip(primitiveBatch, ship, StarWarsTheme.EmpirePrimary, StarWarsTheme.EmpireAccent);
        }
        
        // Draw Rebel ships
        foreach (var ship in _rebelShips)
        {
            DrawShip(primitiveBatch, ship, StarWarsTheme.RebellionPrimary, StarWarsTheme.RebellionAccent);
        }
        
        primitiveBatch.End();
    }
    
    private void DrawShip(PrimitiveBatch primitiveBatch, CombatShip ship, Color primaryColor, Color accentColor)
    {
        // Draw ship hull
        primitiveBatch.DrawFilledRectangle(
            new RectangleF(ship.Position.X - ship.Size.X / 2, ship.Position.Y - ship.Size.Y / 2, 
                          ship.Size.X, ship.Size.Y), 
            new Vector2(ship.Size.X / 2, ship.Size.Y / 2), 
            ship.Rotation, 
            primaryColor);
        
        // Draw ship details
        primitiveBatch.DrawFilledRectangle(
            new RectangleF(ship.Position.X - ship.Size.X / 3, ship.Position.Y - ship.Size.Y / 3, 
                          ship.Size.X * 0.66f, ship.Size.Y * 0.66f), 
            new Vector2(ship.Size.X / 3, ship.Size.Y / 3), 
            ship.Rotation, 
            accentColor);
        
        // Draw engine glow
        var engineGlow = new Color(255, 204, 51, 179);
        primitiveBatch.DrawFilledCircle(
            ship.Position + new Vector2((float)Math.Cos(ship.Rotation + Math.PI) * ship.Size.X / 2, 
                                       (float)Math.Sin(ship.Rotation + Math.PI) * ship.Size.Y / 2), 
            8, 
            12, 
            0.6f, 
            engineGlow);
        
        // Draw shield effect if shields are up
        if (ship.Shields > 0)
        {
            var shieldColor = new Color(51, 153, 255, 77);
            primitiveBatch.DrawFilledCircle(
                ship.Position, 
                Math.Max(ship.Size.X, ship.Size.Y) * 0.8f, 
                16, 
                0.3f, 
                shieldColor);
        }
    }
    
    private void DrawProjectiles(PrimitiveBatch primitiveBatch, CommandList commandList, Framebuffer framebuffer)
    {
        primitiveBatch.Begin(commandList, framebuffer.OutputDescription);
        
        foreach (var projectile in _projectiles)
        {
            var color = projectile.Faction switch
            {
                Faction.Empire => StarWarsTheme.EmpireAccent,
                Faction.Alliance => StarWarsTheme.RebellionAccent,
                _ => StarWarsTheme.Neutral
            };
            
            // Draw projectile trail
            var trailLength = 20f;
            var trailStart = projectile.Position - projectile.Velocity * trailLength;
            
            primitiveBatch.DrawLine(
                trailStart, 
                projectile.Position, 
                2f, 
                color);
            
            // Draw projectile head
            primitiveBatch.DrawFilledCircle(
                projectile.Position, 
                3, 
                8, 
                0.8f, 
                color);
        }
        
        primitiveBatch.End();
    }
    
    private void DrawBattleUI(PrimitiveBatch primitiveBatch, CommandList commandList, Framebuffer framebuffer)
    {
        primitiveBatch.Begin(commandList, framebuffer.OutputDescription);
        
        // Draw top status panel
        primitiveBatch.DrawFilledRectangle(
            new RectangleF(0, 0, 1920, 80), 
            Vector2.Zero, 
            0f, 
            StarWarsTheme.PanelBackground);
        
        // Draw bottom control panel
        primitiveBatch.DrawFilledRectangle(
            new RectangleF(0, 1000, 1920, 80), 
            Vector2.Zero, 
            0f, 
            StarWarsTheme.PanelBackground);
        
        // Draw side tactical panel
        primitiveBatch.DrawFilledRectangle(
            new RectangleF(1720, 80, 200, 920), 
            Vector2.Zero, 
            0f, 
            StarWarsTheme.PanelBackground);
        
        primitiveBatch.End();
    }
    
    private void UpdateCombat(float deltaTime)
    {
        // Simple combat logic: ships fire at nearest enemy
        var allShips = _imperialShips.Concat<CombatShip>(_rebelShips).ToList();
        
        foreach (var ship in allShips)
        {
            if (ship.CanFire())
            {
                var nearestEnemy = FindNearestEnemy(ship, allShips);
                if (nearestEnemy != null && Vector2.Distance(ship.Position, nearestEnemy.Position) < 300)
                {
                    FireProjectile(ship, nearestEnemy.Position);
                    ship.ResetFireCooldown();
                }
            }
        }
    }
    
    private CombatShip? FindNearestEnemy(CombatShip ship, List<CombatShip> allShips)
    {
        CombatShip? nearest = null;
        float nearestDistance = float.MaxValue;
        
        foreach (var other in allShips)
        {
            if (other.Faction != ship.Faction)
            {
                var distance = Vector2.Distance(ship.Position, other.Position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = other;
                }
            }
        }
        
        return nearest;
    }
    
    private void FireProjectile(CombatShip ship, Vector2 targetPosition)
    {
        var direction = Vector2.Normalize(targetPosition - ship.Position);
        var projectile = new Projectile(
            ship.Position + direction * ship.Size.X / 2,
            direction * 200f, // Speed
            ship.Faction
        );
        
        _projectiles.Add(projectile);
    }
    
    private void InitializeShips()
    {
        // Initialize Imperial ships
        _imperialShips.AddRange(new[]
        {
            new CombatShip("Star Destroyer", new Vector2(300, 300), Faction.Empire, new Vector2(60, 40)),
            new CombatShip("TIE Fighter", new Vector2(350, 250), Faction.Empire, new Vector2(20, 15)),
            new CombatShip("TIE Fighter", new Vector2(350, 350), Faction.Empire, new Vector2(20, 15)),
            new CombatShip("Imperial Cruiser", new Vector2(400, 300), Faction.Empire, new Vector2(45, 30)),
        });
        
        // Initialize Rebel ships
        _rebelShips.AddRange(new[]
        {
            new CombatShip("Mon Calamari Cruiser", new Vector2(1600, 300), Faction.Alliance, new Vector2(55, 35)),
            new CombatShip("X-Wing", new Vector2(1550, 250), Faction.Alliance, new Vector2(18, 12)),
            new CombatShip("X-Wing", new Vector2(1550, 350), Faction.Alliance, new Vector2(18, 12)),
            new CombatShip("Y-Wing", new Vector2(1500, 300), Faction.Alliance, new Vector2(25, 20)),
        });
    }
}

/// <summary>
/// Represents a ship in tactical combat.
/// </summary>
public class CombatShip
{
    public string Name { get; }
    public Vector2 Position { get; private set; }
    public Vector2 Velocity { get; private set; }
    public Vector2 Size { get; }
    public Faction Faction { get; }
    public float Rotation { get; private set; }
    public float Health { get; private set; }
    public float Shields { get; private set; }
    public float FireCooldown { get; private set; }
    
    public CombatShip(string name, Vector2 position, Faction faction, Vector2 size)
    {
        Name = name;
        Position = position;
        Faction = faction;
        Size = size;
        Health = 100f;
        Shields = 50f;
        FireCooldown = 0f;
    }
    
    public void Update(float deltaTime)
    {
        // Simple movement: gentle drift
        var time = (float)DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond * 0.001f;
        var drift = new Vector2(
            (float)Math.Sin(time) * 10,
            (float)Math.Cos(time * 0.8f) * 8
        );
        
        Velocity = drift * 0.1f;
        Position += Velocity * deltaTime;
        
        // Gentle rotation
        Rotation += deltaTime * 0.2f;
        
        // Update fire cooldown
        if (FireCooldown > 0)
        {
            FireCooldown -= deltaTime;
        }
    }
    
    public bool CanFire() => FireCooldown <= 0;
    
    public void ResetFireCooldown() => FireCooldown = 2f; // 2 second cooldown
}

/// <summary>
/// Represents a projectile in tactical combat.
/// </summary>
public class Projectile
{
    public Vector2 Position { get; private set; }
    public Vector2 Velocity { get; }
    public Faction Faction { get; }
    public float Lifetime { get; private set; }
    
    public Projectile(Vector2 position, Vector2 velocity, Faction faction)
    {
        Position = position;
        Velocity = velocity;
        Faction = faction;
        Lifetime = 5f; // 5 second lifetime
    }
    
    public void Update(float deltaTime)
    {
        Position += Velocity * deltaTime;
        Lifetime -= deltaTime;
    }
} 