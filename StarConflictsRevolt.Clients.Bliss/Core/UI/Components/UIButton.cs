using Bliss.CSharp.Fonts;
using Bliss.CSharp.Graphics.Rendering.Batches.Primitives;
using Bliss.CSharp.Graphics.Rendering.Batches.Sprites;
using Bliss.CSharp.Graphics.Rendering.Renderers;
using StarConflictsRevolt.Clients.Bliss.Core.UI.Interfaces;
using Veldrid;
using Color = Bliss.CSharp.Colors.Color;
using RectangleF = Bliss.CSharp.Transformations.RectangleF;

namespace StarConflictsRevolt.Clients.Bliss.Core.UI.Components;

/// <summary>
/// A reusable UI button component that can be used across different screens.
/// Follows the Component pattern for reusability and the Single Responsibility Principle.
/// </summary>
public class UIButton : UIComponent
{
    private readonly IInputHandler _inputHandler;
    private readonly string _text;
    private readonly Action _onClick;
    private readonly RectangleF _bounds;
    private bool _isHovered;
    private bool _isSelected;
    private float _animationTime;
    
    public string Text => _text;
    public RectangleF Bounds => _bounds;
    public bool IsHovered => _isHovered;
    public bool IsSelected => _isSelected;
    public bool IsEnabled { get; set; } = true;
    
    bool UIComponent.IsSelected => _isSelected;
    
    public event Action<UIButton>? Clicked;
    
    public UIButton(IInputHandler inputHandler, string text, RectangleF bounds, Action onClick)
    {
        _inputHandler = inputHandler ?? throw new ArgumentNullException(nameof(inputHandler));
        _text = text ?? throw new ArgumentNullException(nameof(text));
        _bounds = bounds;
        _onClick = onClick ?? throw new ArgumentNullException(nameof(onClick));
    }
    
    public void Update(float deltaTime)
    {
        if (!IsEnabled)
            return;
            
        _animationTime += deltaTime;
        
        // Check if mouse is hovering over button
        var mousePos = _inputHandler.GetMousePosition();
        _isHovered = _bounds.Contains(mousePos);
        
        // Handle mouse click
        if (_isHovered && _inputHandler.IsLeftMousePressed())
        {
            Activate();
        }
    }
    
    public void Render(ImmediateRenderer immediateRenderer, 
                      PrimitiveBatch primitiveBatch,
                      SpriteBatch spriteBatch,
                      CommandList commandList,
                      Framebuffer framebuffer)
    {
        primitiveBatch.Begin(commandList, framebuffer.OutputDescription);
        
        // Determine button colors based on state
        var bgColor = GetBackgroundColor();
        var borderColor = GetBorderColor();
        var textColor = GetTextColor();
        
        // Draw button background
        primitiveBatch.DrawFilledRectangle(_bounds, Vector2.Zero, 0f, 0.5f, bgColor);
        
        // Draw button border
        var borderRect = new RectangleF(
            _bounds.X - 2, _bounds.Y - 2, 
            _bounds.Width + 4, _bounds.Height + 4);
        primitiveBatch.DrawFilledRectangle(borderRect, Vector2.Zero, 0f, 0.5f, borderColor);
        
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
        
        primitiveBatch.End();
        
        // Draw text using sprite batch
        DrawButtonText(spriteBatch, textColor);
    }
    
    public void SetSelected(bool selected)
    {
        _isSelected = selected;
    }
    
    public void Activate()
    {
        if (!IsEnabled)
            return;
            
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
            0.5f, StarWarsTheme.EmpireAccent);
        
        // Draw glow effect
        primitiveBatch.DrawFilledRectangle(
            new RectangleF(_bounds.X - 15, _bounds.Y + 5, 5, _bounds.Height - 10), 
            Vector2.Zero, 0f, 0.5f, new Color(255, 51, 51, 77));
    }
    
    private void DrawHoverEffect(PrimitiveBatch primitiveBatch)
    {
        // Draw subtle glow around button
        var glowRect = new RectangleF(
            _bounds.X - 5, _bounds.Y - 5, 
            _bounds.Width + 10, _bounds.Height + 10);
        primitiveBatch.DrawFilledRectangle(
            glowRect, Vector2.Zero, 0f, 0.3f, new Color(102, 102, 153, 51));
    }
    
    private void DrawButtonText(SpriteBatch spriteBatch, Color textColor)
    {
        // Calculate text position (centered in button)
        var textPosition = new Vector2(
            _bounds.X + _bounds.Width / 2f,
            _bounds.Y + _bounds.Height / 2f
        );
        
        // Draw text centered in the button
        try
        {
            // Try to use the Galaxy font if available
            var font = new Font("Assets/Fonts/Galaxy.ttf");
            var fontSize = 20f;
            spriteBatch.DrawText(font, _text, textPosition, fontSize, color: textColor);
            font.Dispose();
        }
        catch
        {
            // Fallback: do nothing
        }
    }
} 