using Bliss.CSharp.Graphics.Rendering.Batches.Primitives;
using Bliss.CSharp.Graphics.Rendering.Batches.Sprites;
using Bliss.CSharp.Graphics.Rendering.Renderers;
using StarConflictsRevolt.Clients.Bliss.Core.UI.Interfaces;
using Veldrid;
using Color = Bliss.CSharp.Colors.Color;
using RectangleF = Bliss.CSharp.Transformations.RectangleF;

namespace StarConflictsRevolt.Clients.Bliss.Core.UI.Components;

/// <summary>
/// A text input component for UI forms.
/// Follows the Component pattern and provides text input functionality.
/// </summary>
public class UITextInput : UIComponent
{
    private readonly IInputHandler _inputHandler;
    private readonly string _placeholder;
    private readonly Func<string> _getValue;
    private readonly Action<string> _setValue;
    private readonly RectangleF _bounds;
    private bool _isSelected;
    private bool _isEnabled = true;
    private float _cursorBlinkTime = 0f;
    private bool _showCursor = true;
    private readonly SimpleTextRenderer _textRenderer;
    
    public string Placeholder => _placeholder;
    public RectangleF Bounds => _bounds;
    public bool IsSelected => _isSelected;
    public bool IsEnabled 
    { 
        get => _isEnabled; 
        set => _isEnabled = value; 
    }
    
    public UITextInput(IInputHandler inputHandler, string placeholder, RectangleF bounds, 
                       Func<string> getValue, Action<string> setValue, SimpleTextRenderer textRenderer)
    {
        _inputHandler = inputHandler ?? throw new ArgumentNullException(nameof(inputHandler));
        _placeholder = placeholder ?? throw new ArgumentNullException(nameof(placeholder));
        _bounds = bounds;
        _getValue = getValue ?? throw new ArgumentNullException(nameof(getValue));
        _setValue = setValue ?? throw new ArgumentNullException(nameof(setValue));
        _textRenderer = textRenderer ?? throw new ArgumentNullException(nameof(textRenderer));
    }
    
    public void Update(float deltaTime)
    {
        if (!IsEnabled)
            return;
            
        _cursorBlinkTime += deltaTime;
        if (_cursorBlinkTime >= 0.5f)
        {
            _showCursor = !_showCursor;
            _cursorBlinkTime = 0f;
        }
    }
    
    public void Render(ImmediateRenderer immediateRenderer,
                      PrimitiveBatch primitiveBatch,
                      SpriteBatch spriteBatch,
                      CommandList commandList,
                      Framebuffer framebuffer)
    {
        // Determine colors based on state
        var bgColor = GetBackgroundColor();
        var borderColor = GetBorderColor();
        var textColor = GetTextColor();
        
        // Draw input field background
        primitiveBatch.DrawFilledRectangle(_bounds, Vector2.Zero, 0f, 0.5f, bgColor);
        
        // Draw input field border
        var borderRect = new RectangleF(
            _bounds.X - 2, _bounds.Y - 2, 
            _bounds.Width + 4, _bounds.Height + 4);
        primitiveBatch.DrawFilledRectangle(borderRect, Vector2.Zero, 0f, 0.5f, borderColor);
        
        // Draw text content
        DrawTextContent(textColor, spriteBatch, primitiveBatch);
        
        // Draw cursor if selected
        if (_isSelected && _showCursor)
        {
            DrawCursor(primitiveBatch);
        }
    }
    
    public void SetSelected(bool selected)
    {
        _isSelected = selected;
        if (selected)
        {
            _showCursor = true;
            _cursorBlinkTime = 0f;
        }
    }
    
    private Color GetBackgroundColor()
    {
        if (!IsEnabled)
            return new Color(64, 64, 64, 255);
            
        if (_isSelected)
            return new Color(77, 77, 102, 255);
            
        return new Color(51, 51, 77, 255);
    }
    
    private Color GetBorderColor()
    {
        if (!IsEnabled)
            return new Color(32, 32, 32, 255);
            
        if (_isSelected)
            return StarWarsTheme.EmpireAccent;
            
        return StarWarsTheme.Border;
    }
    
    private Color GetTextColor()
    {
        if (!IsEnabled)
            return new Color(128, 128, 128, 255);
            
        return Color.White;
    }
    
    private void DrawTextContent(Color textColor, SpriteBatch spriteBatch, PrimitiveBatch primitiveBatch)
    {
        var currentValue = _getValue();
        var displayText = string.IsNullOrEmpty(currentValue) ? _placeholder : currentValue;
        var isPlaceholder = string.IsNullOrEmpty(currentValue);
        var fontColor = isPlaceholder ? new Color(180, 180, 180, 255) : textColor;
        var fontSize = 20f;
        var textBounds = new RectangleF(
            _bounds.X + 10, _bounds.Y + 5,
            _bounds.Width - 20, _bounds.Height - 10);
        // Draw the text using SimpleTextRenderer
        _textRenderer.DrawTextCentered(displayText, textBounds, spriteBatch, "Default", fontSize, fontColor, primitiveBatch);
    }
    
    private void DrawCursor(PrimitiveBatch primitiveBatch)
    {
        var currentValue = _getValue();
        var cursorX = _bounds.X + 10 + (currentValue?.Length * 8 ?? 0); // Approximate character width
        var cursorY = _bounds.Y + 10;
        var cursorHeight = _bounds.Height - 20;
        
        primitiveBatch.DrawLine(
            new Vector2(cursorX, cursorY),
            new Vector2(cursorX, cursorY + cursorHeight),
            2f, 0.5f, StarWarsTheme.EmpireAccent);
    }
} 