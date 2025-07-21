using Bliss.CSharp.Graphics.Rendering.Batches.Primitives;
using Bliss.CSharp.Graphics.Rendering.Batches.Sprites;
using Bliss.CSharp.Graphics.Rendering.Renderers;
using Bliss.CSharp.Interact.Keyboards;
using StarConflictsRevolt.Clients.Bliss.Core;
using StarConflictsRevolt.Clients.Bliss.Core.UI;
using StarConflictsRevolt.Clients.Bliss.Core.UI.Components;
using StarConflictsRevolt.Clients.Bliss.Core.UI.Interfaces;
using Veldrid;
using Color = Bliss.CSharp.Colors.Color;
using RectangleF = Bliss.CSharp.Transformations.RectangleF;
using StarConflictsRevolt.Clients.Models;

namespace StarConflictsRevolt.Clients.Bliss.Views;

/// <summary>
/// Placeholder galaxy screen that will be replaced with the full galaxy view implementation.
/// </summary>
public class GalaxyScreen : BaseScreen
{
    public static Guid CurrentSessionId { get; set; }
    public static WorldDto? CurrentWorld { get; set; }
    private readonly IInputHandler _inputHandler;
    private readonly SimpleTextRenderer _textRenderer;
    private readonly List<UIButton> _buttons = new();
    private float _time = 0f;
    
    public GalaxyScreen(IInputHandler inputHandler, SimpleTextRenderer textRenderer) 
        : base("galaxy", "GALAXY VIEW")
    {
        _inputHandler = inputHandler ?? throw new ArgumentNullException(nameof(inputHandler));
        _textRenderer = textRenderer ?? throw new ArgumentNullException(nameof(textRenderer));
        
        InitializeButtons();
    }
    
    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);
        _time += deltaTime;
        
        // Update all buttons
        foreach (var button in _buttons)
        {
            button.Update(deltaTime);
        }
    }
    
    public override void Render(ImmediateRenderer immediateRenderer,
                               PrimitiveBatch primitiveBatch,
                               SpriteBatch spriteBatch,
                               CommandList commandList,
                               Framebuffer framebuffer)
    {
        // Draw background
        DrawBackground(primitiveBatch, commandList, framebuffer);
        
        // Draw title
        DrawTitle(primitiveBatch, commandList, framebuffer);
        
        // Draw real galaxy if loaded
        if (CurrentWorld != null)
        {
            DrawGalaxyContent(primitiveBatch, commandList, framebuffer, CurrentWorld, spriteBatch);
        }
        else
        {
            // Draw loading message
            DrawLoadingMessage(primitiveBatch, commandList, framebuffer, spriteBatch);
        }
        
        // Draw buttons
        DrawButtons(immediateRenderer, primitiveBatch, spriteBatch, commandList, framebuffer);
    }
    
    public override void HandleInput()
    {
        base.HandleInput();
        
        // Handle Escape key to go back
        if (_inputHandler.IsKeyPressed(KeyboardKey.Escape))
        {
            RequestNavigation("landing");
        }
    }
    
    private void InitializeButtons()
    {
        var buttonWidth = 200f;
        var buttonHeight = 50f;
        var startY = 900f;
        var spacing = 70f;
        
        // Create navigation buttons
        var buttonConfigs = new (string text, Action action)[]
        {
            ("Back to Menu", () => RequestNavigation("landing")),
            ("Exit Game", () => RequestExit())
        };
        
        for (int i = 0; i < buttonConfigs.Length; i++)
        {
            var (text, action) = buttonConfigs[i];
            var bounds = new RectangleF(
                50f + i * (buttonWidth + 20f),
                startY,
                buttonWidth,
                buttonHeight
            );
            
            var button = new UIButton(_inputHandler, text, bounds, action, _textRenderer);
            _buttons.Add(button);
        }
    }
    
    private void DrawBackground(PrimitiveBatch primitiveBatch, CommandList commandList, Framebuffer framebuffer)
    {
        // Draw animated starfield background
        for (int i = 0; i < 200; i++)
        {
            var x = (i * 12345) % 1920;
            var y = (i * 67890) % 1080;
            
            var brightness = (float)(Math.Sin(_time * 2 + i * 0.1f) * 0.5f + 0.5f);
            var color = new Color((byte)(brightness * 255), (byte)(brightness * 0.8f * 255), (byte)(brightness * 255), 255);
            
            var size = 1 + (i % 3);
            primitiveBatch.DrawFilledCircle(
                new Vector2(x, y), 
                size, 
                8, 
                0.5f, 
                color);
        }
    }
    
    private void DrawTitle(PrimitiveBatch primitiveBatch, CommandList commandList, Framebuffer framebuffer)
    {
        // Draw title panel
        var titlePanel = new RectangleF(50f, 50f, 400f, 80f);
        primitiveBatch.DrawFilledRectangle(
            titlePanel, 
            Vector2.Zero, 
            0f, 
            0.5f, 
            new Color(26, 26, 51, 204));
        
        // Draw border
        primitiveBatch.DrawFilledRectangle(
            new RectangleF(titlePanel.X - 3, titlePanel.Y - 3, titlePanel.Width + 6, titlePanel.Height + 6), 
            Vector2.Zero, 
            0f, 
            0.5f, 
            StarWarsTheme.Border);
    }
    
    private void DrawGalaxyContent(PrimitiveBatch primitiveBatch, CommandList commandList, Framebuffer framebuffer, WorldDto world, SpriteBatch spriteBatch)
    {
        // Example: Draw star systems and planets as circles and text
        if (world.Galaxy?.StarSystems != null)
        {
            int i = 0;
            foreach (var system in world.Galaxy.StarSystems)
            {
                var center = new Vector2(400f + i * 200f, 400f);
                var starColor = new Color(255, 255, 200, 255);
                primitiveBatch.DrawFilledCircle(center, 20f, 16, 0.5f, starColor);
                // Draw system name
                // (Assume _textRenderer is available)
                _textRenderer.DrawText(system.Name ?? $"System {i+1}", center + new Vector2(-30, 30), spriteBatch, "Default", 18f, Color.White, primitiveBatch);
                // Draw planets
                if (system.Planets != null)
                {
                    int j = 0;
                    foreach (var planet in system.Planets)
                    {
                        var planetPos = center + new Vector2(-60 + j * 30, 60);
                        primitiveBatch.DrawFilledCircle(planetPos, 8f, 12, 0.5f, new Color(100, 200, 255, 255));
                        _textRenderer.DrawText(planet.Name ?? $"P{j+1}", planetPos + new Vector2(-10, 10), spriteBatch, "Default", 12f, Color.LightGray, primitiveBatch);
                        j++;
                    }
                }
                i++;
            }
        }
    }
    private void DrawLoadingMessage(PrimitiveBatch primitiveBatch, CommandList commandList, Framebuffer framebuffer, SpriteBatch spriteBatch)
    {
        var loadingPanel = new RectangleF(600f, 500f, 600f, 80f);
        primitiveBatch.DrawFilledRectangle(
            loadingPanel,
            Vector2.Zero,
            0f,
            0.5f,
            new Color(26, 26, 51, 204));
        // (Assume _textRenderer is available)
        _textRenderer.DrawTextCentered("Loading galaxy...", loadingPanel, spriteBatch,"Default", 32f, Color.White, primitiveBatch);
    }
    
    private void DrawButtons(ImmediateRenderer immediateRenderer, PrimitiveBatch primitiveBatch, SpriteBatch spriteBatch, CommandList commandList, Framebuffer framebuffer)
    {
        // Render button components
        foreach (var button in _buttons)
        {
            button.Render(immediateRenderer, primitiveBatch, spriteBatch, commandList, framebuffer);
        }
    }
} 