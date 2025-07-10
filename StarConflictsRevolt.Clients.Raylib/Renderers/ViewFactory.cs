using Microsoft.Extensions.DependencyInjection;

namespace StarConflictsRevolt.Clients.Raylib.Renderers;

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
            throw new ArgumentException($"No view implementation found for view type: {viewType}");
        }
        
        return view;
    }
} 