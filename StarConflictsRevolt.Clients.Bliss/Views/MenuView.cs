using Bliss.CSharp.Colors;
using Bliss.CSharp.Graphics.Rendering.Batches.Primitives;
using Bliss.CSharp.Graphics.Rendering.Batches.Sprites;
using Bliss.CSharp.Graphics.Rendering.Renderers;
using Bliss.CSharp.Transformations;
using StarConflictsRevolt.Clients.Bliss.Core;
using StarConflictsRevolt.Clients.Bliss.Core.UI;
using System.Numerics;
using Veldrid;
using Color = Bliss.CSharp.Colors.Color;
using RectangleF = Bliss.CSharp.Transformations.RectangleF;

namespace StarConflictsRevolt.Clients.Bliss.Views;

/// <summary>
/// Main menu view with Star Wars Rebellion inspired styling.
/// </summary>
public class MenuView : GameView
{
    private readonly List<MenuButton> _buttons = new();
    private float _time = 0f;
    private int _selectedButtonIndex = 0;
    
    public event Action<string>? ViewRequested;
    
    public MenuView() : base("Main Menu")
    {
        InitializeButtons();
    }
    
    public override void Update(float deltaTime)
    {
        _time += deltaTime;
        
        // Update button animations
        for (int i = 0; i < _buttons.Count; i++)
        {
            var button = _buttons[i];
            button.IsSelected = i == _selectedButtonIndex;
            button.Update(deltaTime);
        }
    }
    
    public override void Render(ImmediateRenderer immediateRenderer, PrimitiveBatch primitiveBatch, 
                               SpriteBatch spriteBatch, CommandList commandList, Framebuffer framebuffer)
    {
        // Draw animated starfield background
        DrawAnimatedStarfield(primitiveBatch, commandList, framebuffer);
        
        // Draw title
        DrawTitle(primitiveBatch, commandList, framebuffer);
        
        // Draw menu buttons
        DrawMenuButtons(primitiveBatch, commandList, framebuffer);
        
        // Draw decorative elements
        DrawDecorativeElements(primitiveBatch, commandList, framebuffer);
    }
    
    private void DrawAnimatedStarfield(PrimitiveBatch primitiveBatch, CommandList commandList, Framebuffer framebuffer)
    {
        primitiveBatch.Begin(commandList, framebuffer.OutputDescription);
        
        // Draw animated stars
        for (int i = 0; i < 300; i++)
        {
            var x = (i * 12345) % 1920;
            var y = (i * 67890) % 1080;
            
            // Animate star brightness
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
        
        // Draw nebula-like effects
        for (int i = 0; i < 5; i++)
        {
            var center = new Vector2(
                200 + i * 300 + (float)Math.Sin(_time * 0.5f + i) * 50,
                200 + i * 150 + (float)Math.Cos(_time * 0.3f + i) * 30
            );
            
            var nebulaColor = new Color(26, 51, 102, 26);
            primitiveBatch.DrawFilledCircle(
                center, 
                100 + (float)Math.Sin(_time + i) * 20, 
                32, 
                0.1f, 
                nebulaColor);
        }
        
        primitiveBatch.End();
    }
    
    private void DrawTitle(PrimitiveBatch primitiveBatch, CommandList commandList, Framebuffer framebuffer)
    {
        primitiveBatch.Begin(commandList, framebuffer.OutputDescription);
        
        // Draw title background panel - centered
        var titlePanel = UILayout.CenteredRect(1120, 200, 1920, 1080);
        titlePanel.Y = 100; // Position from top
        primitiveBatch.DrawFilledRectangle(
            titlePanel, 
            Vector2.Zero, 
            0f, 
            0.5f, 
            new Color(26, 26, 51, 204));
        
        // Draw title border (using filled rectangle with border effect)
        primitiveBatch.DrawFilledRectangle(
            new RectangleF(titlePanel.X - 3, titlePanel.Y - 3, titlePanel.Width + 6, titlePanel.Height + 6), 
            Vector2.Zero, 
            0f, 
            0.5f, 
            StarWarsTheme.Border);
        
        // Draw subtitle line
        primitiveBatch.DrawLine(
            new Vector2(titlePanel.X + 50, titlePanel.Y + 180), 
            new Vector2(titlePanel.X + titlePanel.Width - 50, titlePanel.Y + 180), 
            2f, 
            0.5f, 
            StarWarsTheme.EmpireAccent);
        
        primitiveBatch.End();
    }
    
    private void DrawMenuButtons(PrimitiveBatch primitiveBatch, CommandList commandList, Framebuffer framebuffer)
    {
        primitiveBatch.Begin(commandList, framebuffer.OutputDescription);
        
        for (int i = 0; i < _buttons.Count; i++)
        {
            var button = _buttons[i];
            DrawButton(primitiveBatch, button, i);
        }
        
        primitiveBatch.End();
    }
    
    private void DrawButton(PrimitiveBatch primitiveBatch, MenuButton button, int index)
    {
        // Center the button container
        var buttonContainer = UILayout.CenteredRect(720, 60, 1920, 1080);
        buttonContainer.Y = 400 + index * 80; // Position from top
        
        // Button background
        var bgColor = button.IsSelected ? StarWarsTheme.EmpirePrimary : StarWarsTheme.PanelBackground;
        primitiveBatch.DrawFilledRectangle(buttonContainer, Vector2.Zero, 0f, 0.5f, bgColor);
        
        // Button border (using filled rectangle with border effect)
        var borderColor = button.IsSelected ? StarWarsTheme.EmpireAccent : StarWarsTheme.Border;
        primitiveBatch.DrawFilledRectangle(
            new RectangleF(buttonContainer.X - 2, buttonContainer.Y - 2, buttonContainer.Width + 4, buttonContainer.Height + 4), 
            Vector2.Zero, 
            0f, 
            0.5f, 
            borderColor);
        
        // Selection indicator
        if (button.IsSelected)
        {
            // Draw selection arrow
            var arrowPoints = new Vector2[]
            {
                new Vector2(buttonContainer.X - 20, buttonContainer.Y + 30),
                new Vector2(buttonContainer.X - 10, buttonContainer.Y + 20),
                new Vector2(buttonContainer.X - 10, buttonContainer.Y + 40)
            };
            
            primitiveBatch.DrawFilledTriangle(
                arrowPoints[0], 
                arrowPoints[1], 
                arrowPoints[2], 
                0.5f, 
                StarWarsTheme.EmpireAccent);
            
            // Draw glow effect
            primitiveBatch.DrawFilledRectangle(
                new RectangleF(buttonContainer.X - 15, buttonContainer.Y + 5, 5, 50), 
                Vector2.Zero, 
                0f, 
                0.5f, 
                new Color(255, 51, 51, 77));
        }
    }
    
    private void DrawDecorativeElements(PrimitiveBatch primitiveBatch, CommandList commandList, Framebuffer framebuffer)
    {
        primitiveBatch.Begin(commandList, framebuffer.OutputDescription);
        
        // Draw decorative lines
        for (int i = 0; i < 3; i++)
        {
            var y = 350 + i * 200;
            primitiveBatch.DrawLine(
                new Vector2(0, y), 
                new Vector2(200, y), 
                1f, 
                0.5f, 
                StarWarsTheme.EmpireSecondary);
            
            primitiveBatch.DrawLine(
                new Vector2(1720, y), 
                new Vector2(1920, y), 
                1f, 
                0.5f, 
                StarWarsTheme.RebellionSecondary);
        }
        
        // Draw corner decorations
        DrawCornerDecoration(primitiveBatch, new Vector2(50, 50), true);
        DrawCornerDecoration(primitiveBatch, new Vector2(1870, 50), false);
        DrawCornerDecoration(primitiveBatch, new Vector2(50, 1030), true);
        DrawCornerDecoration(primitiveBatch, new Vector2(1870, 1030), false);
        
        primitiveBatch.End();
    }
    
    private void DrawCornerDecoration(PrimitiveBatch primitiveBatch, Vector2 position, bool isLeft)
    {
        var size = 40f;
        var color = StarWarsTheme.Border;
        
        if (isLeft)
        {
            // Left corner decoration
            primitiveBatch.DrawLine(
                position, 
                position + new Vector2(size, 0), 
                2f, 
                0.5f, 
                color);
            
            primitiveBatch.DrawLine(
                position, 
                position + new Vector2(0, size), 
                2f, 
                0.5f, 
                color);
        }
        else
        {
            // Right corner decoration
            primitiveBatch.DrawLine(
                position, 
                position + new Vector2(-size, 0), 
                2f, 
                0.5f, 
                color);
            
            primitiveBatch.DrawLine(
                position, 
                position + new Vector2(0, size), 
                2f, 
                0.5f, 
                color);
        }
    }
    
    private void InitializeButtons()
    {
        _buttons.AddRange(new[]
        {
            new MenuButton("Galaxy Overview", () => ViewRequested?.Invoke("Galaxy Overview")),
            new MenuButton("Tactical Battle", () => ViewRequested?.Invoke("Tactical Battle")),
            new MenuButton("Fleet Management", () => ViewRequested?.Invoke("Fleet Management")),
            new MenuButton("Planetary Operations", () => ViewRequested?.Invoke("Planetary Operations")),
            new MenuButton("Diplomacy", () => ViewRequested?.Invoke("Diplomacy")),
            new MenuButton("Research & Development", () => ViewRequested?.Invoke("Research")),
            new MenuButton("Game Options", () => ViewRequested?.Invoke("Game Options")),
            new MenuButton("Exit Game", () => Environment.Exit(0))
        });
    }
    
    public void SelectNext()
    {
        _selectedButtonIndex = (_selectedButtonIndex + 1) % _buttons.Count;
    }
    
    public void SelectPrevious()
    {
        _selectedButtonIndex = (_selectedButtonIndex - 1 + _buttons.Count) % _buttons.Count;
    }
    
    public void ActivateSelected()
    {
        if (_selectedButtonIndex >= 0 && _selectedButtonIndex < _buttons.Count)
        {
            _buttons[_selectedButtonIndex].Activate();
        }
    }
}

/// <summary>
/// Represents a menu button.
/// </summary>
public class MenuButton
{
    public string Text { get; }
    public Action OnActivate { get; }
    public bool IsSelected { get; set; }
    public float AnimationTime { get; private set; }
    
    public MenuButton(string text, Action onActivate)
    {
        Text = text;
        OnActivate = onActivate;
        IsSelected = false;
        AnimationTime = 0f;
    }
    
    public void Update(float deltaTime)
    {
        if (IsSelected)
        {
            AnimationTime += deltaTime;
        }
        else
        {
            AnimationTime = 0f;
        }
    }
    
    public void Activate()
    {
        OnActivate?.Invoke();
    }
} 