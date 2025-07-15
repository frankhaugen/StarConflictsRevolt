using System.Collections.Generic;

namespace StarConflictsRevolt.Clients.Bliss.Core;

/// <summary>
/// Manages the different game views and handles view transitions.
/// </summary>
public class ViewManager
{
    private readonly Dictionary<string, IGameView> _views = new();
    private IGameView? _currentView;
    private IGameView? _previousView;
    
    /// <summary>
    /// The currently active view.
    /// </summary>
    public IGameView? CurrentView => _currentView;
    
    /// <summary>
    /// Register a view with the manager.
    /// </summary>
    public void RegisterView(IGameView view)
    {
        _views[view.Title] = view;
        
        // Set the first registered view as current if none is set
        if (_currentView == null)
        {
            _currentView = view;
        }
    }
    
    /// <summary>
    /// Switch to a different view by title.
    /// </summary>
    public bool SwitchToView(string viewTitle)
    {
        if (_views.TryGetValue(viewTitle, out var view))
        {
            _previousView = _currentView;
            _currentView = view;
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Go back to the previous view.
    /// </summary>
    public bool GoBack()
    {
        if (_previousView != null)
        {
            var temp = _currentView;
            _currentView = _previousView;
            _previousView = temp;
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Get a view by title.
    /// </summary>
    public IGameView? GetView(string viewTitle)
    {
        return _views.TryGetValue(viewTitle, out var view) ? view : null;
    }
    
    /// <summary>
    /// Get all registered views.
    /// </summary>
    public IEnumerable<IGameView> GetAllViews()
    {
        return _views.Values;
    }
} 