using StarConflictsRevolt.Clients.Raylib.Rendering.Core;

namespace StarConflictsRevolt.Tests.TestingInfrastructure.TestViews;

public class TestViewFactory : IViewFactory
{
    private readonly List<GameView> _viewCreationHistory = new();
    private readonly Dictionary<GameView, IView> _views = new();

    public IReadOnlyList<GameView> ViewCreationHistory => _viewCreationHistory.AsReadOnly();

    public IView CreateView(GameView viewType)
    {
        _viewCreationHistory.Add(viewType);
        return _views.TryGetValue(viewType, out var view) ? view : new TestView(viewType);
    }

    public void RegisterView(GameView viewType, IView view)
    {
        _views[viewType] = view;
    }

    public void ClearHistory()
    {
        _viewCreationHistory.Clear();
    }
}