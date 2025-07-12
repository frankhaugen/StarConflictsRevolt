using StarConflictsRevolt.Clients.Raylib.Renderers;

namespace StarConflictsRevolt.Tests.TestingInfrastructure;

public class TestGalaxyView : IView
{
    public GameView ViewType => GameView.Galaxy;
    public void Draw() { }
}