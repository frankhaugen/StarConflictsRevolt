using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Clients.Raylib.Renderers;

namespace StarConflictsRevolt.Clients.Raylib.Services;

public class GameState
{
    // Session and player information
    public SessionDto? Session { get; set; }
    public string? PlayerId { get; set; }
    public string? PlayerName { get; set; }
    public string? AccessToken { get; set; }

    // Game control
    public GameSpeed Speed { get; set; } = GameSpeed.Normal;
    public bool IsPaused => Speed == GameSpeed.Paused;
    public GameView CurrentView { get; set; } = GameView.Menu;
    public GameView? PreviousView { get; set; }

    // UI state
    public IGameObject? SelectedObject { get; set; }
    public string? FeedbackMessage { get; set; }
    public DateTime? FeedbackExpiry { get; set; }
    public bool ShowConfirmationDialog { get; set; }
    public string? ConfirmationMessage { get; set; }
    public Action? ConfirmationAction { get; set; }

    // Navigation history
    public Stack<GameView> ViewHistory { get; } = new();

    // Game data
    public WorldDto? World { get; set; }

    public bool HasExpiredFeedback => FeedbackExpiry.HasValue && DateTime.UtcNow > FeedbackExpiry.Value;

    public void SetFeedback(string message, TimeSpan? duration = null)
    {
        FeedbackMessage = message;
        FeedbackExpiry = duration.HasValue ? DateTime.UtcNow.Add(duration.Value) : DateTime.UtcNow.AddSeconds(3);
    }

    public void ClearFeedback()
    {
        FeedbackMessage = null;
        FeedbackExpiry = null;
    }

    public void NavigateTo(GameView view)
    {
        if (CurrentView != view)
        {
            ViewHistory.Push(CurrentView);
            PreviousView = CurrentView;
            CurrentView = view;
        }
    }

    public void NavigateBack()
    {
        if (ViewHistory.Count > 0)
        {
            PreviousView = CurrentView;
            CurrentView = ViewHistory.Pop();
        }
    }

    public void ShowConfirmation(string message, Action action)
    {
        ConfirmationMessage = message;
        ConfirmationAction = action;
        ShowConfirmationDialog = true;
    }

    public void HideConfirmation()
    {
        ShowConfirmationDialog = false;
        ConfirmationMessage = null;
        ConfirmationAction = null;
    }
}