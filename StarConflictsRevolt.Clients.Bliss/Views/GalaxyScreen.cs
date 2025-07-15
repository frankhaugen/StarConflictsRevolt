using Bliss.CSharp.Colors;
using Bliss.CSharp.Graphics.Rendering.Batches.Primitives;
using Bliss.CSharp.Graphics.Rendering.Batches.Sprites;
using Bliss.CSharp.Graphics.Rendering.Renderers;
using Bliss.CSharp.Interact.Keyboards;
using Bliss.CSharp.Transformations;
using StarConflictsRevolt.Clients.Bliss.Core;
using StarConflictsRevolt.Clients.Bliss.Core.UI;
using StarConflictsRevolt.Clients.Bliss.Core.UI.Components;
using StarConflictsRevolt.Clients.Bliss.Core.UI.Interfaces;
using System.Numerics;
using Veldrid;
using Color = Bliss.CSharp.Colors.Color;
using RectangleF = Bliss.CSharp.Transformations.RectangleF;

namespace StarConflictsRevolt.Clients.Bliss.Views;

/// <summary>
/// Placeholder galaxy screen that will be replaced with the full galaxy view implementation.
/// </summary>
public class GalaxyScreen : BaseScreen
{
    private readonly IInputHandler _inputHandler;
    private readonly List<UIButton> _buttons = new();
    private float _time = 0f;
    
    public GalaxyScreen(IInputHandler inputHandler) 
        : base("galaxy", "GALAXY VIEW")
    {
        _inputHandler = inputHandler ?? throw new ArgumentNullException(nameof(inputHandler));
        
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
        
        // Draw placeholder content
        DrawPlaceholderContent(primitiveBatch, commandList, framebuffer);
        
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
            
            var button = new UIButton(_inputHandler, text, bounds, action);
            _buttons.Add(button);
        }
    }
    
    private void DrawBackground(PrimitiveBatch primitiveBatch, CommandList commandList, Framebuffer framebuffer)
    {
        primitiveBatch.Begin(commandList, framebuffer.OutputDescription);
        
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
        
        primitiveBatch.End();
    }
    
    private void DrawTitle(PrimitiveBatch primitiveBatch, CommandList commandList, Framebuffer framebuffer)
    {
        primitiveBatch.Begin(commandList, framebuffer.OutputDescription);
        
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
        
        primitiveBatch.End();
    }
    
    private void DrawPlaceholderContent(PrimitiveBatch primitiveBatch, CommandList commandList, Framebuffer framebuffer)
    {
        primitiveBatch.Begin(commandList, framebuffer.OutputDescription);
        
        // Draw placeholder galaxy content
        var contentPanel = new RectangleF(200f, 200f, 1520f, 600f);
        primitiveBatch.DrawFilledRectangle(
            contentPanel, 
            Vector2.Zero, 
            0f, 
            0.5f, 
            new Color(26, 26, 51, 153));
        
        // Draw border
        primitiveBatch.DrawFilledRectangle(
            new RectangleF(contentPanel.X - 3, contentPanel.Y - 3, contentPanel.Width + 6, contentPanel.Height + 6), 
            Vector2.Zero, 
            0f, 
            0.5f, 
            StarWarsTheme.Border);
        
        // Draw some placeholder star systems
        for (int i = 0; i < 5; i++)
        {
            var center = new Vector2(
                400f + i * 200f + (float)Math.Sin(_time * 0.5f + i) * 20,
                400f + (float)Math.Cos(_time * 0.3f + i) * 20
            );
            
            var starColor = new Color(255, 255, 200, 255);
            primitiveBatch.DrawFilledCircle(
                center, 
                20f, 
                16, 
                0.5f, 
                starColor);
        }
        
        primitiveBatch.End();
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