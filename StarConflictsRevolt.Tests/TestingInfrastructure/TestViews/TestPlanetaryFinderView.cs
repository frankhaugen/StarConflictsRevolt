using StarConflictsRevolt.Clients.Raylib.Renderers;

namespace StarConflictsRevolt.Tests.TestingInfrastructure;

public class TestPlanetaryFinderView : IView
{
    public GameView ViewType => GameView.PlanetaryFinder;
    public void Draw() { }
}