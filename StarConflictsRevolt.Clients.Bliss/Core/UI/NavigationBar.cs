using Bliss.CSharp.Graphics.Rendering.Batches.Primitives;
using Bliss.CSharp.Graphics.Rendering.Batches.Sprites;
using Bliss.CSharp.Graphics.Rendering.Renderers;
using Veldrid;
using Color = Bliss.CSharp.Colors.Color;
using RectangleF = Bliss.CSharp.Transformations.RectangleF;

namespace StarConflictsRevolt.Clients.Bliss.Core.UI;

/// <summary>
/// Navigation bar item for the navigation bar.
/// </summary>
public class NavigationItem
{
    public string Text { get; }
    public string ViewName { get; }
    public bool IsSelected { get; set; }
    public bool IsEnabled { get; set; } = true;
    public Action? OnClick { get; set; }
    
    public NavigationItem(string text, string viewName, Action? onClick = null)
    {
        Text = text;
        ViewName = viewName;
        OnClick = onClick;
    }
}

/// <summary>
/// Horizontal navigation bar component with Star Wars Rebellion styling.
/// </summary>
public class NavigationBar
{
    private readonly List<NavigationItem> _items = new();
    private RectangleF _bounds;
    private int _selectedIndex = 0;
    private float _animationTime = 0f;
    private bool _isVisible = true;
    
    public event Action<string>? ViewRequested;
    
    /// <summary>
    /// Gets or sets the visibility of the navigation bar.
    /// </summary>
    public bool IsVisible
    {
        get => _isVisible;
        set => _isVisible = value;
    }
    
    /// <summary>
    /// Gets the bounds of the navigation bar.
    /// </summary>
    public RectangleF Bounds => _bounds;
    
    /// <summary>
    /// Gets the currently selected item index.
    /// </summary>
    public int SelectedIndex => _selectedIndex;
    
    public NavigationBar()
    {
        InitializeDefaultItems();
    }
    
    /// <summary>
    /// Sets the bounds of the navigation bar.
    /// </summary>
    public void SetBounds(RectangleF bounds)
    {
        _bounds = bounds;
    }
    
    /// <summary>
    /// Adds a navigation item to the bar.
    /// </summary>
    public void AddItem(NavigationItem item)
    {
        _items.Add(item);
    }
    
    /// <summary>
    /// Removes a navigation item from the bar.
    /// </summary>
    public void RemoveItem(string viewName)
    {
        var item = _items.FirstOrDefault(x => x.ViewName == viewName);
        if (item != null)
        {
            _items.Remove(item);
        }
    }
    
    /// <summary>
    /// Selects the next item in the navigation bar.
    /// </summary>
    public void SelectNext()
    {
        if (_items.Count == 0) return;
        
        _selectedIndex = (_selectedIndex + 1) % _items.Count;
        UpdateSelection();
    }
    
    /// <summary>
    /// Selects the previous item in the navigation bar.
    /// </summary>
    public void SelectPrevious()
    {
        if (_items.Count == 0) return;
        
        _selectedIndex = (_selectedIndex - 1 + _items.Count) % _items.Count;
        UpdateSelection();
    }
    
    /// <summary>
    /// Activates the currently selected item.
    /// </summary>
    public void ActivateSelected()
    {
        if (_selectedIndex >= 0 && _selectedIndex < _items.Count)
        {
            var item = _items[_selectedIndex];
            if (item.IsEnabled)
            {
                item.OnClick?.Invoke();
                ViewRequested?.Invoke(item.ViewName);
            }
        }
    }
    
    /// <summary>
    /// Selects an item by view name.
    /// </summary>
    public void SelectItem(string viewName)
    {
        for (int i = 0; i < _items.Count; i++)
        {
            if (_items[i].ViewName == viewName)
            {
                _selectedIndex = i;
                UpdateSelection();
                break;
            }
        }
    }
    
    /// <summary>
    /// Updates the navigation bar.
    /// </summary>
    public void Update(float deltaTime)
    {
        _animationTime += deltaTime;
        
        // Update item animations
        for (int i = 0; i < _items.Count; i++)
        {
            var item = _items[i];
            item.IsSelected = i == _selectedIndex;
        }
    }
    
    /// <summary>
    /// Renders the navigation bar.
    /// </summary>
    public void Render(ImmediateRenderer immediateRenderer, PrimitiveBatch primitiveBatch, 
                      SpriteBatch spriteBatch, CommandList commandList, Framebuffer framebuffer)
    {
        if (!_isVisible) return;
        
        // Draw background
        DrawBackground(primitiveBatch, commandList, framebuffer);
        
        // Draw items
        DrawItems(primitiveBatch, commandList, framebuffer);
        
        // Draw decorative elements
        DrawDecorations(primitiveBatch, commandList, framebuffer);
    }
    
    private void DrawBackground(PrimitiveBatch primitiveBatch, CommandList commandList, Framebuffer framebuffer)
    {
        primitiveBatch.Begin(commandList, framebuffer.OutputDescription);
        
        // Main background
        primitiveBatch.DrawFilledRectangle(
            _bounds, 
            Vector2.Zero, 
            0f, 
            0.5f, 
            StarWarsTheme.PanelBackground);
        
        // Border
        primitiveBatch.DrawFilledRectangle(
            new RectangleF(_bounds.X - 2, _bounds.Y - 2, _bounds.Width + 4, _bounds.Height + 4), 
            Vector2.Zero, 
            0f, 
            0.5f, 
            StarWarsTheme.Border);
        
        // Bottom accent line
        primitiveBatch.DrawLine(
            new Vector2(_bounds.X, _bounds.Y + _bounds.Height), 
            new Vector2(_bounds.X + _bounds.Width, _bounds.Y + _bounds.Height), 
            3f, 
            0.5f, 
            StarWarsTheme.EmpireAccent);
        
        primitiveBatch.End();
    }
    
    private void DrawItems(PrimitiveBatch primitiveBatch, CommandList commandList, Framebuffer framebuffer)
    {
        if (_items.Count == 0) return;
        
        primitiveBatch.Begin(commandList, framebuffer.OutputDescription);
        
        var itemWidth = _bounds.Width / _items.Count;
        var itemHeight = _bounds.Height - 20f; // Leave space for border
        
        for (int i = 0; i < _items.Count; i++)
        {
            var item = _items[i];
            var itemRect = new RectangleF(
                _bounds.X + i * itemWidth + 5f,
                _bounds.Y + 10f,
                itemWidth - 10f,
                itemHeight
            );
            
            // Item background
            var bgColor = item.IsSelected ? StarWarsTheme.EmpirePrimary : 
                         item.IsEnabled ? StarWarsTheme.BackgroundSecondary : StarWarsTheme.Neutral;
            
            primitiveBatch.DrawFilledRectangle(
                itemRect, 
                Vector2.Zero, 
                0f, 
                0.5f, 
                bgColor);
            
            // Item border
            var borderColor = item.IsSelected ? StarWarsTheme.EmpireAccent : StarWarsTheme.Border;
            primitiveBatch.DrawFilledRectangle(
                new RectangleF(itemRect.X - 1, itemRect.Y - 1, itemRect.Width + 2, itemRect.Height + 2), 
                Vector2.Zero, 
                0f, 
                0.5f, 
                borderColor);
            
            // Selection indicator
            if (item.IsSelected)
            {
                // Draw selection glow
                var glowRect = new RectangleF(
                    itemRect.X - 3, 
                    itemRect.Y - 3, 
                    itemRect.Width + 6, 
                    itemRect.Height + 6
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
            }
        }
        
        primitiveBatch.End();
    }
    
    private void DrawDecorations(PrimitiveBatch primitiveBatch, CommandList commandList, Framebuffer framebuffer)
    {
        primitiveBatch.Begin(commandList, framebuffer.OutputDescription);
        
        // Draw corner decorations
        var cornerSize = 20f;
        
        // Top-left corner
        DrawCornerDecoration(primitiveBatch, new Vector2(_bounds.X, _bounds.Y), true, true);
        
        // Top-right corner
        DrawCornerDecoration(primitiveBatch, new Vector2(_bounds.X + _bounds.Width - cornerSize, _bounds.Y), false, true);
        
        // Bottom-left corner
        DrawCornerDecoration(primitiveBatch, new Vector2(_bounds.X, _bounds.Y + _bounds.Height - cornerSize), true, false);
        
        // Bottom-right corner
        DrawCornerDecoration(primitiveBatch, new Vector2(_bounds.X + _bounds.Width - cornerSize, _bounds.Y + _bounds.Height - cornerSize), false, false);
        
        primitiveBatch.End();
    }
    
    private void DrawCornerDecoration(PrimitiveBatch primitiveBatch, Vector2 position, bool isLeft, bool isTop)
    {
        var cornerSize = 20f;
        var lineLength = 15f;
        var lineThickness = 2f;
        
        // Horizontal line
        var hStart = isLeft ? position.X : position.X + cornerSize - lineLength;
        var hEnd = isLeft ? position.X + lineLength : position.X + cornerSize;
        var hY = isTop ? position.Y : position.Y + cornerSize;
        
        primitiveBatch.DrawLine(
            new Vector2(hStart, hY), 
            new Vector2(hEnd, hY), 
            lineThickness, 
            0.5f, 
            StarWarsTheme.EmpireAccent);
        
        // Vertical line
        var vStart = isTop ? position.Y : position.Y + cornerSize - lineLength;
        var vEnd = isTop ? position.Y + lineLength : position.Y + cornerSize;
        var vX = isLeft ? position.X : position.X + cornerSize;
        
        primitiveBatch.DrawLine(
            new Vector2(vX, vStart), 
            new Vector2(vX, vEnd), 
            lineThickness, 
            0.5f, 
            StarWarsTheme.EmpireAccent);
    }
    
    private void UpdateSelection()
    {
        for (int i = 0; i < _items.Count; i++)
        {
            _items[i].IsSelected = i == _selectedIndex;
        }
    }
    
    private void InitializeDefaultItems()
    {
        AddItem(new NavigationItem("Main Menu", "Main Menu"));
        AddItem(new NavigationItem("Galaxy Overview", "Galaxy Overview"));
        AddItem(new NavigationItem("Tactical Battle", "Tactical Battle"));
    }
} 