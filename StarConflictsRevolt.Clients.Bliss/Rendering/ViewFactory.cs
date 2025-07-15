using StarConflictsRevolt.Clients.Bliss.Core;

namespace StarConflictsRevolt.Clients.Bliss.Rendering;

/// <summary>
/// Factory for creating game views in the Bliss client.
/// </summary>
public class ViewFactory : IViewFactory
{
    private readonly IEnumerable<IView> _views;

    public ViewFactory(IEnumerable<IView> views)
    {
        _views = views;
    }

    public IView CreateView(GameView viewType)
    {
        var view = _views.FirstOrDefault(v => v.ViewType == viewType);
        if (view == null)
        {
            // Return a default view if the requested view is not found
            return new DefaultView(viewType);
        }

        return view;
    }
}

/// <summary>
/// Default view implementation for unsupported view types.
/// </summary>
public class DefaultView : IView
{
    public GameView ViewType { get; }

    public DefaultView(GameView viewType)
    {
        ViewType = viewType;
    }

    public void Update(float deltaTime)
    {
        // No update logic for default view
    }

    public void Draw()
    {
        // No drawing for default view
    }

    public void OnActivate()
    {
        // No activation logic for default view
    }

    public void OnDeactivate()
    {
        // No deactivation logic for default view
    }
} 