using StarConflictsRevolt.Clients.Raylib.Renderers;

namespace StarConflictsRevolt.Tests.TestingInfrastructure;

public class TestTacticalBattleView : IView
{
    public GameView ViewType => GameView.TacticalBattle;
    public void Draw() { }
}