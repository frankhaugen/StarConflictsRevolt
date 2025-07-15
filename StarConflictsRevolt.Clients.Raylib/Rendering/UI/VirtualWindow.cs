using System.Numerics;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Interact;
using StarConflictsRevolt.Clients.Raylib.Rendering.Core;

namespace StarConflictsRevolt.Clients.Raylib.Rendering.UI;

public class VirtualWindow : IUIElement
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public Vector2 Position { get; set; }
    public Vector2 Size { get; set; }
    public Rectangle Bounds => new(Position.X, Position.Y, Size.X, Size.Y);
    public bool IsVisible { get; set; } = true;
    public bool IsEnabled { get; set; } = true;
    public bool HasFocus { get; set; }
    public string Title { get; set; } = "Window";
    public bool IsDraggable { get; set; } = true;
    public bool IsResizable { get; set; } = true;
    public bool IsModal { get; set; } = false;
    public Action? OnClose { get; set; }
    public List<IUIElement> Children { get; set; } = new();

    private bool _dragging = false;
    private Vector2 _dragOffset;
    private bool _resizing = false;
    private Vector2 _resizeStart;
    private Vector2 _resizeOrigin;
    private const int TitleBarHeight = 32;
    private const int ResizeHandleSize = 12;

    public VirtualWindow(string title, float x, float y, float width, float height)
    {
        Title = title;
        Position = new Vector2(x, y);
        Size = new Vector2(width, height);
    }

    public void Update(float deltaTime, IInputState inputState)
    {
        if (!IsVisible || !IsEnabled) return;
        var mouse = Input.GetMousePosition();
        var titleBarRect = new Rectangle(Position.X, Position.Y, Size.X, TitleBarHeight);
        var resizeRect = new Rectangle(Position.X + Size.X - ResizeHandleSize, Position.Y + Size.Y - ResizeHandleSize, ResizeHandleSize, ResizeHandleSize);

        // Dragging
        if (IsDraggable && Input.IsMouseButtonPressed(MouseButton.Left) && ContainsPoint(titleBarRect, mouse))
        {
            _dragging = true;
            _dragOffset = new Vector2(mouse.X - Position.X, mouse.Y - Position.Y);
        }
        if (_dragging)
        {
            if (Input.IsMouseButtonDown(MouseButton.Left))
            {
                Position = new Vector2(mouse.X - _dragOffset.X, mouse.Y - _dragOffset.Y);
            }
            else
            {
                _dragging = false;
            }
        }

        // Resizing
        if (IsResizable && Input.IsMouseButtonPressed(MouseButton.Left) && ContainsPoint(resizeRect, mouse))
        {
            _resizing = true;
            _resizeStart = mouse;
            _resizeOrigin = Size;
        }
        if (_resizing)
        {
            if (Input.IsMouseButtonDown(MouseButton.Left))
            {
                var newWidth = Math.Max(200, _resizeOrigin.X + (mouse.X - _resizeStart.X));
                var newHeight = Math.Max(120, _resizeOrigin.Y + (mouse.Y - _resizeStart.Y));
                Size = new Vector2(newWidth, newHeight);
            }
            else
            {
                _resizing = false;
            }
        }

        // Children
        foreach (var child in Children)
            child.Update(deltaTime, inputState);
    }

    public void Render(IUIRenderer renderer)
    {
        if (!IsVisible) return;
        var winRect = new Raylib_CSharp.Transformations.Rectangle(Position.X, Position.Y, Size.X, Size.Y);
        var titleBarRect = new Raylib_CSharp.Transformations.Rectangle(Position.X, Position.Y, Size.X, TitleBarHeight);
        var resizeRect = new Raylib_CSharp.Transformations.Rectangle(Position.X + Size.X - ResizeHandleSize, Position.Y + Size.Y - ResizeHandleSize, ResizeHandleSize, ResizeHandleSize);

        // Modal overlay
        if (IsModal)
        {
            var overlay = new Color(0, 0, 0, 120);
            Graphics.DrawRectangle(0, 0, Raylib_CSharp.Windowing.Window.GetScreenWidth(), Raylib_CSharp.Windowing.Window.GetScreenHeight(), overlay);
        }

        // Window panel
        Graphics.DrawRectangleRec(winRect, UIManager.Colors.Surface);
        Graphics.DrawRectangleLinesEx(winRect, 2, UIManager.Colors.Border);

        // Title bar
        Graphics.DrawRectangleRec(titleBarRect, UIManager.Colors.Accent);
        Graphics.DrawRectangleLinesEx(titleBarRect, 1, UIManager.Colors.Border);
        Graphics.DrawText(Title, (int)Position.X + 12, (int)Position.Y + 8, UIHelper.FontSizes.Large, UIManager.Colors.Text);

        // Close button
        var closeRect = new Raylib_CSharp.Transformations.Rectangle(Position.X + Size.X - 36, Position.Y + 6, 24, 20);
        Graphics.DrawRectangleRec(closeRect, UIManager.Colors.Danger);
        Graphics.DrawText("X", (int)closeRect.X + 7, (int)closeRect.Y + 2, UIHelper.FontSizes.Medium, UIManager.Colors.Text);

        // Resize handle
        if (IsResizable)
        {
            Graphics.DrawRectangleRec(resizeRect, UIManager.Colors.Accent);
        }

        // Children
        foreach (var child in Children)
            child.Render(renderer);
    }

    public bool HandleInput(IInputState inputState)
    {
        if (!IsVisible || !IsEnabled) return false;
        var mouse = Input.GetMousePosition();
        var closeRect = new Rectangle(Position.X + Size.X - 36, Position.Y + 6, 24, 20);
        if (Input.IsMouseButtonPressed(MouseButton.Left) && ContainsPoint(closeRect, mouse))
        {
            OnClose?.Invoke();
            return true;
        }
        foreach (var child in Children)
            if (child.HandleInput(inputState)) return true;
        return false;
    }

    public bool Contains(Vector2 point) => ContainsPoint(Bounds, point);

    private static bool ContainsPoint(Rectangle rect, Vector2 point)
        => point.X >= rect.X && point.X <= rect.X + rect.Width && point.Y >= rect.Y && point.Y <= rect.Y + rect.Height;
} 