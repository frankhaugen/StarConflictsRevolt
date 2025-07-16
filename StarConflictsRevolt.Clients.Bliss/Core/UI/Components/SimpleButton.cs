using Bliss.CSharp.Colors;
using Bliss.CSharp.Graphics.Rendering.Batches.Primitives;
using Bliss.CSharp.Graphics.Rendering.Batches.Sprites;
using Bliss.CSharp.Graphics.Rendering.Renderers;
using Bliss.CSharp.Transformations;
using StarConflictsRevolt.Clients.Bliss.Core.UI.Interfaces;
using Veldrid;
using Color = Bliss.CSharp.Colors.Color;
using RectangleF = Bliss.CSharp.Transformations.RectangleF;

namespace StarConflictsRevolt.Clients.Bliss.Core.UI.Components;

/// <summary>
/// Simplified button component that uses the SimpleTextRenderer.
/// Follows the Component pattern for reusability.
/// </summary>
public class SimpleButton : UIComponent
{
    private readonly IInputHandler _inputHandler;
    private readonly string _text;
    private readonly Action _onClick;
    private readonly RectangleF _bounds;
    private readonly SimpleTextRenderer _textRenderer;
    private bool _isHovered;
    private bool _isSelected;
    
    public string Text => _text;
    public RectangleF Bounds => _bounds;
    public bool IsHovered => _isHovered;
    public bool IsSelected => _isSelected;
    public bool IsEnabled { get; set; } = true;
    
    bool UIComponent.IsSelected => _isSelected;
    
    public event Action<SimpleButton>? Clicked;
    
    public SimpleButton(IInputHandler inputHandler, string text, RectangleF bounds, Action onClick, SimpleTextRenderer textRenderer)
    {
        _inputHandler = inputHandler ?? throw new ArgumentNullException(nameof(inputHandler));
        _text = text ?? throw new ArgumentNullException(nameof(text));
        _bounds = bounds;
        _onClick = onClick ?? throw new ArgumentNullException(nameof(onClick));
        _textRenderer = textRenderer ?? throw new ArgumentNullException(nameof(textRenderer));
    }
    
    public void Update(float deltaTime)
    {
        if (!IsEnabled) return;
        
        // Check if mouse is hovering over button
        var mousePos = _inputHandler.GetMousePosition();
        _isHovered = _bounds.Contains(mousePos);
        
        // Handle mouse click
        if (_isHovered && _inputHandler.IsLeftMousePressed())
        {
            Activate();
        }
    }
    
    public void Render(ImmediateRenderer immediateRenderer, PrimitiveBatch primitiveBatch, SpriteBatch spriteBatch, CommandList commandList, Framebuffer framebuffer)
    {
        // Determine button colors based on state
        var bgColor = GetBackgroundColor();
        var borderColor = GetBorderColor();
        var textColor = GetTextColor();
        
        // Draw button background
        primitiveBatch.DrawFilledRectangle(_bounds, Vector2.Zero, 0f, 0.7f, bgColor);
        
        // Draw button border
        var borderRect = new RectangleF(
            _bounds.X - 2, _bounds.Y - 2, 
            _bounds.Width + 4, _bounds.Height + 4);
        primitiveBatch.DrawFilledRectangle(borderRect, Vector2.Zero, 0f, 0.75f, borderColor);
        
        // Draw selection indicator if selected
        if (_isSelected)
        {
            DrawSelectionIndicator(primitiveBatch);
        }
        
        // Draw hover effect if hovered
        if (_isHovered)
        {
            DrawHoverEffect(primitiveBatch);
        }
        
        // Draw text centered in button using the provided SpriteBatch
        _textRenderer.DrawTextCentered(_text, _bounds, spriteBatch, "Galaxy", 20f, textColor);
    }
    
    public void SetSelected(bool selected)
    {
        _isSelected = selected;
    }
    
    public void Activate()
    {
        if (!IsEnabled) return;
        
        _onClick();
        Clicked?.Invoke(this);
    }
    
    private Color GetBackgroundColor()
    {
        if (!IsEnabled)
            return new Color(64, 64, 64, 255);
            
        if (_isSelected)
            return StarWarsTheme.EmpirePrimary;
            
        if (_isHovered)
            return new Color(77, 77, 102, 255);
            
        return StarWarsTheme.PanelBackground;
    }
    
    private Color GetBorderColor()
    {
        if (!IsEnabled)
            return new Color(32, 32, 32, 255);
            
        if (_isSelected)
            return StarWarsTheme.EmpireAccent;
            
        if (_isHovered)
            return new Color(102, 102, 153, 255);
            
        return StarWarsTheme.Border;
    }
    
    private Color GetTextColor()
    {
        if (!IsEnabled)
            return new Color(128, 128, 128, 255);
            
        if (_isSelected || _isHovered)
            return Color.White;
            
        return new Color(200, 200, 200, 255);
    }
    
    private void DrawSelectionIndicator(PrimitiveBatch primitiveBatch)
    {
        // Draw selection arrow
        var arrowPoints = new Vector2[]
        {
            new Vector2(_bounds.X - 20, _bounds.Y + _bounds.Height / 2),
            new Vector2(_bounds.X - 10, _bounds.Y + _bounds.Height / 2 - 10),
            new Vector2(_bounds.X - 10, _bounds.Y + _bounds.Height / 2 + 10)
        };
        
        primitiveBatch.DrawFilledTriangle(
            arrowPoints[0], arrowPoints[1], arrowPoints[2], 
            0.8f, StarWarsTheme.EmpireAccent);
        
        // Draw glow effect
        primitiveBatch.DrawFilledRectangle(
            new RectangleF(_bounds.X - 15, _bounds.Y + 5, 5, _bounds.Height - 10), 
            Vector2.Zero, 0f, 0.8f, new Color(255, 51, 51, 77));
    }
    
    private void DrawHoverEffect(PrimitiveBatch primitiveBatch)
    {
        // Draw subtle glow around button
        var glowRect = new RectangleF(
            _bounds.X - 5, _bounds.Y - 5, 
            _bounds.Width + 10, _bounds.Height + 10);
        primitiveBatch.DrawFilledRectangle(
            glowRect, Vector2.Zero, 0f, 0.65f, new Color(102, 102, 153, 51));
    }
} 