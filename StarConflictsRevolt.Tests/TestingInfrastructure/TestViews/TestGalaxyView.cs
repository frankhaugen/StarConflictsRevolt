using StarConflictsRevolt.Clients.Raylib.Renderers;

namespace StarConflictsRevolt.Tests.TestingInfrastructure.TestViews;

public class TestGalaxyView : IView
{
    public GameView ViewType => GameView.Galaxy;
    public void Draw() { }
}