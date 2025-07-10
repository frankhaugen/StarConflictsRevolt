namespace StarConflictsRevolt.Clients.Raylib.Renderers;

public interface IViewFactory
{
    IView CreateView(GameView viewType);
}