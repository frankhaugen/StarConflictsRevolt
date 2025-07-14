using StarConflictsRevolt.Clients.Raylib.Rendering.Core;

namespace StarConflictsRevolt.Tests.TestingInfrastructure.TestViews;

public class TestView : IView
{
    public TestView(GameView viewType)
    {
        ViewType = viewType;
    }

    public GameView ViewType { get; }

    public void Draw()
    {
    }
}