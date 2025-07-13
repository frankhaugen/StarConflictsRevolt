using StarConflictsRevolt.Clients.Raylib.Renderers;

namespace StarConflictsRevolt.Tests.TestingInfrastructure;

public class TestFleetFinderView : IView
{
    public GameView ViewType => GameView.FleetFinder;
    public void Draw() { }
}