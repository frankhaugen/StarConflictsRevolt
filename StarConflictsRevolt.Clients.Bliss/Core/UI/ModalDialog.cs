using Bliss.CSharp.Graphics.Rendering.Batches.Primitives;
using Bliss.CSharp.Graphics.Rendering.Batches.Sprites;
using Bliss.CSharp.Graphics.Rendering.Renderers;
using Bliss.CSharp.Interact;
using Bliss.CSharp.Interact.Keyboards;
using Veldrid;
using Color = Bliss.CSharp.Colors.Color;
using RectangleF = Bliss.CSharp.Transformations.RectangleF;

namespace StarConflictsRevolt.Clients.Bliss.Core.UI;

/// <summary>
/// Modal dialog button.
/// </summary>
public class ModalButton
{
    public string Text { get; }
    public Action? OnClick { get; }
    public bool IsDefault { get; }
    public bool IsCancel { get; }
    
    public ModalButton(string text, Action? onClick = null, bool isDefault = false, bool isCancel = false)
    {
        Text = text;
        OnClick = onClick;
        IsDefault = isDefault;
        IsCancel = isCancel;
    }
}

/// <summary>
/// Modal dialog with Star Wars Rebellion styling.
/// </summary>
public class ModalDialog
{
    private RectangleF _bounds;
    private string _title;
    private string _message;
    private readonly List<ModalButton> _buttons = new();
    private int _selectedButtonIndex = 0;
    private float _animationTime = 0f;
    private bool _isVisible = false;
    private float _fadeAlpha = 0f;
    private float _scale = 0.8f;
    
    public event Action? OnClose;
    
    /// <summary>
    /// Gets or sets the visibility of the modal.
    /// </summary>
    public bool IsVisible
    {
        get => _isVisible;
        set
        {
            _isVisible = value;
            if (value)
            {
                _fadeAlpha = 0f;
                _scale = 0.8f;
                _selectedButtonIndex = 0;
            }
        }
    }
    
    /// <summary>
    /// Gets the bounds of the modal.
    /// </summary>
    public RectangleF Bounds => _bounds;
    
    public ModalDialog()
    {
        _title = "Dialog";
        _message = "Message";
        _bounds = new RectangleF(0, 0, 400, 300);
    }
    
    /// <summary>
    /// Shows a simple message dialog.
    /// </summary>
    public void ShowMessage(string title, string message, string buttonText = "OK")
    {
        _title = title;
        _message = message;
        _buttons.Clear();
        _buttons.Add(new ModalButton(buttonText, () => Close(), true));
        IsVisible = true;
    }
    
    /// <summary>
    /// Shows a confirmation dialog.
    /// </summary>
    public void ShowConfirmation(string title, string message, Action onConfirm, Action? onCancel = null)
    {
        _title = title;
        _message = message;
        _buttons.Clear();
        _buttons.Add(new ModalButton("Cancel", onCancel ?? (() => Close()), false, true));
        _buttons.Add(new ModalButton("Confirm", () => { onConfirm(); Close(); }, true));
        IsVisible = true;
    }
    
    /// <summary>
    /// Shows a custom dialog with multiple buttons.
    /// </summary>
    public void ShowCustom(string title, string message, params ModalButton[] buttons)
    {
        _title = title;
        _message = message;
        _buttons.Clear();
        _buttons.AddRange(buttons);
        IsVisible = true;
    }
    
    /// <summary>
    /// Closes the modal.
    /// </summary>
    public void Close()
    {
        IsVisible = false;
        OnClose?.Invoke();
    }
    
    /// <summary>
    /// Sets the bounds of the modal.
    /// </summary>
    public void SetBounds(RectangleF bounds)
    {
        _bounds = bounds;
    }
    
    /// <summary>
    /// Centers the modal on screen.
    /// </summary>
    public void CenterOnScreen(float screenWidth, float screenHeight)
    {
        _bounds = UILayout.Center(_bounds, screenWidth, screenHeight);
    }
    
    /// <summary>
    /// Selects the next button.
    /// </summary>
    public void SelectNext()
    {
        if (_buttons.Count == 0) return;
        
        _selectedButtonIndex = (_selectedButtonIndex + 1) % _buttons.Count;
    }
    
    /// <summary>
    /// Selects the previous button.
    /// </summary>
    public void SelectPrevious()
    {
        if (_buttons.Count == 0) return;
        
        _selectedButtonIndex = (_selectedButtonIndex - 1 + _buttons.Count) % _buttons.Count;
    }
    
    /// <summary>
    /// Activates the currently selected button.
    /// </summary>
    public void ActivateSelected()
    {
        if (_selectedButtonIndex >= 0 && _selectedButtonIndex < _buttons.Count)
        {
            _buttons[_selectedButtonIndex].OnClick?.Invoke();
        }
    }
    
    /// <summary>
    /// Updates the modal.
    /// </summary>
    public void Update(float deltaTime)
    {
        if (!_isVisible) return;
        
        _animationTime += deltaTime;
        
        // Animate fade in
        if (_fadeAlpha < 1f)
        {
            _fadeAlpha = Math.Min(1f, _fadeAlpha + deltaTime * 4f);
        }
        
        // Animate scale in
        if (_scale < 1f)
        {
            _scale = Math.Min(1f, _scale + deltaTime * 3f);
        }
    }
    
    /// <summary>
    /// Renders the modal.
    /// </summary>
    public void Render(ImmediateRenderer immediateRenderer, PrimitiveBatch primitiveBatch, 
                      SpriteBatch spriteBatch, CommandList commandList, Framebuffer framebuffer)
    {
        if (!_isVisible) return;
        
        // Draw backdrop
        DrawBackdrop(primitiveBatch, commandList, framebuffer);
        
        // Draw modal content
        DrawModal(primitiveBatch, commandList, framebuffer);
    }
    
    private void DrawBackdrop(PrimitiveBatch primitiveBatch, CommandList commandList, Framebuffer framebuffer)
    {
        primitiveBatch.Begin(commandList, framebuffer.OutputDescription);
        
        // Semi-transparent backdrop
        var backdropColor = new Color(0, 0, 0, (byte)(_fadeAlpha * 180));
        primitiveBatch.DrawFilledRectangle(
            new RectangleF(0, 0, 1920, 1080), // Full screen
            Vector2.Zero, 
            0f, 
            0.5f, 
            backdropColor);
        
        primitiveBatch.End();
    }
    
    private void DrawModal(PrimitiveBatch primitiveBatch, CommandList commandList, Framebuffer framebuffer)
    {
        primitiveBatch.Begin(commandList, framebuffer.OutputDescription);
        
        // Calculate scaled bounds for animation
        var scaledBounds = new RectangleF(
            _bounds.X + (_bounds.Width * (1f - _scale)) / 2f,
            _bounds.Y + (_bounds.Height * (1f - _scale)) / 2f,
            _bounds.Width * _scale,
            _bounds.Height * _scale
        );
        
        // Main background
        var bgColor = new Color(
            StarWarsTheme.PanelBackground.R,
            StarWarsTheme.PanelBackground.G,
            StarWarsTheme.PanelBackground.B,
            (byte)(StarWarsTheme.PanelBackground.A * _fadeAlpha)
        );
        
        primitiveBatch.DrawFilledRectangle(
            scaledBounds, 
            Vector2.Zero, 
            0f, 
            0.5f, 
            bgColor);
        
        // Border
        var borderColor = new Color(
            StarWarsTheme.Border.R,
            StarWarsTheme.Border.G,
            StarWarsTheme.Border.B,
            (byte)(StarWarsTheme.Border.A * _fadeAlpha)
        );
        
        primitiveBatch.DrawFilledRectangle(
            new RectangleF(scaledBounds.X - 3, scaledBounds.Y - 3, scaledBounds.Width + 6, scaledBounds.Height + 6), 
            Vector2.Zero, 
            0f, 
            0.5f, 
            borderColor);
        
        // Title bar
        var titleBarHeight = 40f;
        var titleBarRect = new RectangleF(scaledBounds.X, scaledBounds.Y, scaledBounds.Width, titleBarHeight);
        
        var titleBarColor = new Color(
            StarWarsTheme.EmpirePrimary.R,
            StarWarsTheme.EmpirePrimary.G,
            StarWarsTheme.EmpirePrimary.B,
            (byte)(StarWarsTheme.EmpirePrimary.A * _fadeAlpha)
        );
        
        primitiveBatch.DrawFilledRectangle(
            titleBarRect, 
            Vector2.Zero, 
            0f, 
            0.5f, 
            titleBarColor);
        
        // Title bar border
        primitiveBatch.DrawLine(
            new Vector2(scaledBounds.X, scaledBounds.Y + titleBarHeight), 
            new Vector2(scaledBounds.X + scaledBounds.Width, scaledBounds.Y + titleBarHeight), 
            2f, 
            0.5f, 
            StarWarsTheme.EmpireAccent);
        
        // Corner decorations
        DrawCornerDecorations(primitiveBatch, scaledBounds);
        
        primitiveBatch.End();
    }
    
    private void DrawCornerDecorations(PrimitiveBatch primitiveBatch, RectangleF bounds)
    {
        var cornerSize = 25f;
        var lineLength = 18f;
        var lineThickness = 3f;
        
        // Top-left corner
        DrawCornerDecoration(primitiveBatch, new Vector2(bounds.X, bounds.Y), true, true, cornerSize, lineLength, lineThickness);
        
        // Top-right corner
        DrawCornerDecoration(primitiveBatch, new Vector2(bounds.X + bounds.Width - cornerSize, bounds.Y), false, true, cornerSize, lineLength, lineThickness);
        
        // Bottom-left corner
        DrawCornerDecoration(primitiveBatch, new Vector2(bounds.X, bounds.Y + bounds.Height - cornerSize), true, false, cornerSize, lineLength, lineThickness);
        
        // Bottom-right corner
        DrawCornerDecoration(primitiveBatch, new Vector2(bounds.X + bounds.Width - cornerSize, bounds.Y + bounds.Height - cornerSize), false, false, cornerSize, lineLength, lineThickness);
    }
    
    private void DrawCornerDecoration(PrimitiveBatch primitiveBatch, Vector2 position, bool isLeft, bool isTop, 
                                    float cornerSize, float lineLength, float lineThickness)
    {
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
    
    /// <summary>
    /// Handles keyboard input for the modal.
    /// </summary>
    public void HandleInput()
    {
        if (!_isVisible) return;
        
        // Handle button navigation
        if (Input.IsKeyPressed(KeyboardKey.Left))
        {
            SelectPrevious();
        }
        else if (Input.IsKeyPressed(KeyboardKey.Right))
        {
            SelectNext();
        }
        else if (Input.IsKeyPressed(KeyboardKey.Enter))
        {
            ActivateSelected();
        }
        else if (Input.IsKeyPressed(KeyboardKey.Escape))
        {
            // Find cancel button or close
            var cancelButton = _buttons.FirstOrDefault(b => b.IsCancel);
            if (cancelButton != null)
            {
                cancelButton.OnClick?.Invoke();
            }
            else
            {
                Close();
            }
        }
    }
} 