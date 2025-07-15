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

namespace StarConflictsRevolt.Clients.Bliss.Core.UI;

/// <summary>
/// Reusable button component with Star Wars Rebellion styling.
/// </summary>
public class Button
{
    private RectangleF _bounds;
    private string _text;
    private Action? _onClick;
    private bool _isSelected;
    private bool _isEnabled;
    private float _animationTime;
    private bool _isHovered;
    
    /// <summary>
    /// Gets or sets the bounds of the button.
    /// </summary>
    public RectangleF Bounds
    {
        get => _bounds;
        set => _bounds = value;
    }
    
    /// <summary>
    /// Gets or sets the button text.
    /// </summary>
    public string Text
    {
        get => _text;
        set => _text = value;
    }
    
    /// <summary>
    /// Gets or sets whether the button is selected.
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;
        set => _isSelected = value;
    }
    
    /// <summary>
    /// Gets or sets whether the button is enabled.
    /// </summary>
    public bool IsEnabled
    {
        get => _isEnabled;
        set => _isEnabled = value;
    }
    
    /// <summary>
    /// Gets or sets whether the button is hovered.
    /// </summary>
    public bool IsHovered
    {
        get => _isHovered;
        set => _isHovered = value;
    }
    
    public Button(string text, Action? onClick = null)
    {
        _text = text;
        _onClick = onClick;
        _isEnabled = true;
        _bounds = new RectangleF(0, 0, 200, 50);
    }
    
    /// <summary>
    /// Sets the bounds of the button.
    /// </summary>
    public void SetBounds(RectangleF bounds)
    {
        _bounds = bounds;
    }
    
    /// <summary>
    /// Centers the button on screen.
    /// </summary>
    public void CenterOnScreen(float screenWidth, float screenHeight)
    {
        _bounds = UILayout.Center(_bounds, screenWidth, screenHeight);
    }
    
    /// <summary>
    /// Centers the button horizontally.
    /// </summary>
    public void CenterHorizontal(float screenWidth)
    {
        _bounds = UILayout.CenterHorizontal(_bounds, screenWidth);
    }
    
    /// <summary>
    /// Centers the button vertically.
    /// </summary>
    public void CenterVertical(float screenHeight)
    {
        _bounds = UILayout.CenterVertical(_bounds, screenHeight);
    }
    
    /// <summary>
    /// Activates the button.
    /// </summary>
    public void Activate()
    {
        if (_isEnabled)
        {
            _onClick?.Invoke();
        }
    }
    
    /// <summary>
    /// Updates the button.
    /// </summary>
    public void Update(float deltaTime)
    {
        _animationTime += deltaTime;
    }
    
    /// <summary>
    /// Renders the button.
    /// </summary>
    public void Render(PrimitiveBatch primitiveBatch, CommandList commandList, Framebuffer framebuffer)
    {
        primitiveBatch.Begin(commandList, framebuffer.OutputDescription);
        
        // Determine button colors based on state
        Color bgColor, borderColor, textColor;
        
        if (!_isEnabled)
        {
            bgColor = StarWarsTheme.Neutral;
            borderColor = StarWarsTheme.Border;
            textColor = StarWarsTheme.TextSecondary;
        }
        else if (_isSelected)
        {
            bgColor = StarWarsTheme.EmpirePrimary;
            borderColor = StarWarsTheme.EmpireAccent;
            textColor = StarWarsTheme.Text;
        }
        else if (_isHovered)
        {
            bgColor = StarWarsTheme.BackgroundSecondary;
            borderColor = StarWarsTheme.EmpireAccent;
            textColor = StarWarsTheme.Text;
        }
        else
        {
            bgColor = StarWarsTheme.PanelBackground;
            borderColor = StarWarsTheme.Border;
            textColor = StarWarsTheme.Text;
        }
        
        // Draw button background
        primitiveBatch.DrawFilledRectangle(
            _bounds, 
            Vector2.Zero, 
            0f, 
            0.5f, 
            bgColor);
        
        // Draw button border
        primitiveBatch.DrawFilledRectangle(
            new RectangleF(_bounds.X - 2, _bounds.Y - 2, _bounds.Width + 4, _bounds.Height + 4), 
            Vector2.Zero, 
            0f, 
            0.5f, 
            borderColor);
        
        // Draw selection indicator
        if (_isSelected && _isEnabled)
        {
            // Draw selection glow
            var glowRect = new RectangleF(
                _bounds.X - 4, 
                _bounds.Y - 4, 
                _bounds.Width + 8, 
                _bounds.Height + 8
            );
            
            var glowIntensity = (float)(Math.Sin(_animationTime * 4) * 0.3f + 0.7f);
            var glowColor = new Color(
                (byte)(StarWarsTheme.EmpireAccent.R * glowIntensity),
                (byte)(StarWarsTheme.EmpireAccent.G * glowIntensity),
                (byte)(StarWarsTheme.EmpireAccent.B * glowIntensity),
                (byte)(StarWarsTheme.EmpireAccent.A * 0.5f)
            );
            
            primitiveBatch.DrawFilledRectangle(
                glowRect, 
                Vector2.Zero, 
                0f, 
                0.5f, 
                glowColor);
            
            // Draw selection arrow
            var arrowPoints = new Vector2[]
            {
                new Vector2(_bounds.X - 15, _bounds.Y + _bounds.Height / 2),
                new Vector2(_bounds.X - 5, _bounds.Y + _bounds.Height / 2 - 10),
                new Vector2(_bounds.X - 5, _bounds.Y + _bounds.Height / 2 + 10)
            };
            
            primitiveBatch.DrawFilledTriangle(
                arrowPoints[0], 
                arrowPoints[1], 
                arrowPoints[2], 
                0.5f, 
                StarWarsTheme.EmpireAccent);
        }
        
        primitiveBatch.End();
    }
    
    /// <summary>
    /// Checks if a point is within the button bounds.
    /// </summary>
    public bool Contains(Vector2 point)
    {
        return _bounds.Contains(point);
    }
    
    /// <summary>
    /// Checks if a point is within the button bounds.
    /// </summary>
    public bool Contains(float x, float y)
    {
        return _bounds.Contains(new Vector2(x, y));
    }
} 