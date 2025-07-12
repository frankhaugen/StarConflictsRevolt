using StarConflictsRevolt.Clients.Raylib.Renderers;

namespace StarConflictsRevolt.Tests.TestingInfrastructure;

public class TestView : IView
{
    public TestView(GameView viewType)
    {
        ViewType = viewType;
    }

    public GameView ViewType { get; }
    public void Draw() { }
}