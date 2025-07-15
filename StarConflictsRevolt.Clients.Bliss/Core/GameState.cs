using StarConflictsRevolt.Clients.Models;
using System.Numerics;

namespace StarConflictsRevolt.Clients.Bliss.Core;

/// <summary>
/// Central state management for the Bliss game client.
/// </summary>
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
    public GameStateInfoDto? PlayerState { get; set; }

    // Rendering state
    public bool FrameInvalid { get; set; } = true;
    public Matrix3x2 CameraMatrix { get; set; } = Matrix3x2.Identity;

    public bool HasExpiredFeedback => FeedbackExpiry.HasValue && DateTime.UtcNow > FeedbackExpiry.Value;

    public void SetFeedback(string message, TimeSpan? duration = null)
    {
        FeedbackMessage = message;
        FeedbackExpiry = duration.HasValue ? DateTime.UtcNow.Add(duration.Value) : DateTime.UtcNow.AddSeconds(3);
        FrameInvalid = true;
    }

    public void ClearFeedback()
    {
        FeedbackMessage = null;
        FeedbackExpiry = null;
        FrameInvalid = true;
    }

    public void NavigateTo(GameView view)
    {
        if (CurrentView != view)
        {
            ViewHistory.Push(CurrentView);
            PreviousView = CurrentView;
            CurrentView = view;
            FrameInvalid = true;
        }
    }

    public void NavigateBack()
    {
        if (ViewHistory.Count > 0)
        {
            PreviousView = CurrentView;
            CurrentView = ViewHistory.Pop();
            FrameInvalid = true;
        }
    }

    public void ShowConfirmation(string message, Action action)
    {
        ConfirmationMessage = message;
        ConfirmationAction = action;
        ShowConfirmationDialog = true;
        FrameInvalid = true;
    }

    public void HideConfirmation()
    {
        ShowConfirmationDialog = false;
        ConfirmationMessage = null;
        ConfirmationAction = null;
        FrameInvalid = true;
    }

    public void InvalidateFrame()
    {
        FrameInvalid = true;
    }
}

/// <summary>
/// Game speed enumeration.
/// </summary>
public enum GameSpeed
{
    Paused,
    Slow,
    Normal,
    Fast
}

/// <summary>
/// Game view enumeration matching the Raylib client.
/// </summary>
public enum GameView
{
    // Main navigation views
    Menu,
    Galaxy,
    System,
    Planet,

    // Strategic & Finder Views (F1-F7)
    GameOptions,
    PlanetaryFinder,
    FleetFinder,
    TroopFinder,
    PersonnelFinder,
    MessageWindow,
    Encyclopedia,

    // Agent menu / GID Views (ALT-A to ALT-V)
    BuildShips,
    BuildTroops,
    BuildFacilities,
    GalaxyOverview,
    GameObjectives,
    ManageGarrisons,
    ManageProduction,
    TranslateCounterpart,
    AgentAdvice,
    PopularSupport,
    Uprising,
    IdleFleets,
    EnrouteFleets,
    IdlePersonnel,
    ActivePersonnel,
    IdleShipyard,
    IdleTrainingFacilities,
    IdleConstruction,

    // Tactical / Battle Views
    TacticalBattle,
    DeathStarControl,

    // System & Miscellaneous Windows
    SaveLoadScreen,
    MultiplayerOptions,
    PauseDialog
} 