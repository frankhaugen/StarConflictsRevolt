using StarConflictsRevolt.Clients.Raylib.Renderers;

namespace StarConflictsRevolt.Tests.TestingInfrastructure;

public class TestViewFactory : IViewFactory
{
    private readonly Dictionary<GameView, IView> _views = new();
    private readonly List<GameView> _viewCreationHistory = new();

    public void RegisterView(GameView viewType, IView view)
    {
        _views[viewType] = view;
    }

    public IView CreateView(GameView viewType)
    {
        _viewCreationHistory.Add(viewType);
        return _views.TryGetValue(viewType, out var view) ? view : new TestView(viewType);
    }

    public IReadOnlyList<GameView> ViewCreationHistory => _viewCreationHistory.AsReadOnly();
    public void ClearHistory() => _viewCreationHistory.Clear();
}