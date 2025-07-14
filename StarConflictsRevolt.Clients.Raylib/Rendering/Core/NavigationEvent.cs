namespace StarConflictsRevolt.Clients.Raylib.Rendering.Core;

/// <summary>
/// Represents a navigation event for changing between views.
/// </summary>
public class NavigationEvent
{
    /// <summary>
    /// Target view ID to navigate to.
    /// </summary>
    public string TargetViewId { get; }
    
    /// <summary>
    /// Optional data to pass to the target view.
    /// </summary>
    public object? Data { get; }
    
    /// <summary>
    /// Whether to replace the current view in the navigation stack.
    /// </summary>
    public bool ReplaceCurrent { get; }
    
    /// <summary>
    /// Whether to clear the navigation stack before navigating.
    /// </summary>
    public bool ClearStack { get; }
    
    public NavigationEvent(string targetViewId, object? data = null, bool replaceCurrent = false, bool clearStack = false)
    {
        TargetViewId = targetViewId;
        Data = data;
        ReplaceCurrent = replaceCurrent;
        ClearStack = clearStack;
    }
    
    /// <summary>
    /// Create a navigation event to go back.
    /// </summary>
    /// <returns>Navigation event for going back.</returns>
    public static NavigationEvent GoBack()
    {
        return new NavigationEvent("back");
    }
    
    /// <summary>
    /// Create a navigation event to go to the main menu.
    /// </summary>
    /// <returns>Navigation event for main menu.</returns>
    public static NavigationEvent GoToMainMenu()
    {
        return new NavigationEvent("main_menu");
    }
    
    /// <summary>
    /// Create a navigation event to go to a specific view.
    /// </summary>
    /// <param name="viewId">Target view ID.</param>
    /// <param name="data">Optional data.</param>
    /// <returns>Navigation event.</returns>
    public static NavigationEvent GoToView(string viewId, object? data = null)
    {
        return new NavigationEvent(viewId, data);
    }
} 