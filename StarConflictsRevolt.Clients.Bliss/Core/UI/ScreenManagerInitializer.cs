using StarConflictsRevolt.Clients.Bliss.Core.UI.Interfaces;

namespace StarConflictsRevolt.Clients.Bliss.Core.UI;

/// <summary>
/// Initializes the screen manager with all available screens.
/// Follows the Single Responsibility Principle by focusing only on initialization.
/// </summary>
public class ScreenManagerInitializer : IScreenManagerInitializer
{
    private readonly IScreenManager _screenManager;
    private readonly IScreenFactory _screenFactory;
    
    public ScreenManagerInitializer(IScreenManager screenManager, IScreenFactory screenFactory)
    {
        _screenManager = screenManager ?? throw new ArgumentNullException(nameof(screenManager));
        _screenFactory = screenFactory ?? throw new ArgumentNullException(nameof(screenFactory));
    }
    
    public void Initialize()
    {
        var availableScreenIds = _screenFactory.GetAvailableScreenIds();
        
        foreach (var screenId in availableScreenIds)
        {
            var screen = _screenFactory.CreateScreen(screenId);
            if (screen != null)
            {
                _screenManager.RegisterScreen(screen);
            }
        }
    }
} 