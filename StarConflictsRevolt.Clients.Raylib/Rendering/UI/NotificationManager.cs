using System.Numerics;
using StarConflictsRevolt.Clients.Raylib.Rendering.Core;

namespace StarConflictsRevolt.Clients.Raylib.Rendering.UI;

public class NotificationManager : IUIElement
{
    public string Id { get; set; } = "NotificationManager";
    public Vector2 Position { get; set; }
    public Vector2 Size { get; set; }
    public Rectangle Bounds { get; set; }
    public bool IsVisible { get; set; } = true;
    public bool IsEnabled { get; set; } = true;
    public bool HasFocus { get; set; }

    private readonly List<Notification> _notifications = new();
    private const float NotificationDuration = 3.5f;
    private const float FadeTime = 0.4f;
    private const int MaxVisible = 4;

    public void Add(string message, NotificationType type = NotificationType.Info)
    {
        _notifications.Add(new Notification
        {
            Message = message,
            Type = type,
            Time = 0f,
            State = NotificationState.FadingIn
        });
        if (_notifications.Count > 10)
            _notifications.RemoveAt(0);
    }

    public void Update(float deltaTime, IInputState inputState)
    {
        for (int i = _notifications.Count - 1; i >= 0; i--)
        {
            var n = _notifications[i];
            n.Time += deltaTime;
            if (n.State == NotificationState.FadingIn && n.Time > FadeTime)
            {
                n.State = NotificationState.Visible;
                n.Time = 0f;
            }
            else if (n.State == NotificationState.Visible && n.Time > NotificationDuration)
            {
                n.State = NotificationState.FadingOut;
                n.Time = 0f;
            }
            else if (n.State == NotificationState.FadingOut && n.Time > FadeTime)
            {
                _notifications.RemoveAt(i);
            }
        }
    }

    public void Render(IUIRenderer renderer)
    {
        int y = 60;
        int shown = 0;
        foreach (var n in _notifications)
        {
            if (shown >= MaxVisible) break;
            float alpha = n.State switch
            {
                NotificationState.FadingIn => n.Time / FadeTime,
                NotificationState.Visible => 1f,
                NotificationState.FadingOut => 1f - (n.Time / FadeTime),
                _ => 1f
            };
            var color = n.Type switch
            {
                NotificationType.Success => Theme.Success,
                NotificationType.Warning => Theme.Warning,
                NotificationType.Error => Theme.Error,
                NotificationType.Info => Theme.Info,
                _ => Theme.Panel
            };
            color.A = (byte)(color.A * alpha);
            var rect = new Rectangle(20, y, 400, 36);
            Raylib_CSharp.Rendering.Graphics.DrawRectangleRec(ToRaylibRect(rect), color);
            Raylib_CSharp.Rendering.Graphics.DrawRectangleLinesEx(ToRaylibRect(rect), 1, Theme.Border);
            Raylib_CSharp.Rendering.Graphics.DrawText(n.Message, 32, y + 8, Theme.BodyFont, Theme.Text);
            y += 44;
            shown++;
        }
    }

    public bool HandleInput(IInputState inputState) => false;
    public bool Contains(Vector2 point) => false;

    private static Raylib_CSharp.Transformations.Rectangle ToRaylibRect(Rectangle r)
    {
        return new Raylib_CSharp.Transformations.Rectangle(r.X, r.Y, r.Width, r.Height);
    }

    private class Notification
    {
        public string Message = string.Empty;
        public NotificationType Type;
        public float Time;
        public NotificationState State;
    }

    public enum NotificationType { Info, Success, Warning, Error }
    private enum NotificationState { FadingIn, Visible, FadingOut }
} 