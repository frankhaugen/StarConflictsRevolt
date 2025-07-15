using Microsoft.Extensions.DependencyInjection;
using StarConflictsRevolt.Clients.Bliss.Core.UI.Interfaces;

namespace StarConflictsRevolt.Clients.Bliss.Core.UI;

/// <summary>
/// Factory for creating UI screens with proper dependency injection.
/// Follows the Factory pattern and uses dependency injection for screen creation.
/// </summary>
public class ScreenFactory : IScreenFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, Type> _screenTypes = new();
    
    public ScreenFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        RegisterScreenTypes();
    }
    
    public IScreen? CreateScreen(string screenId)
    {
        if (string.IsNullOrEmpty(screenId))
            return null;
            
        if (!_screenTypes.TryGetValue(screenId, out var screenType))
            return null;
            
        try
        {
            return (IScreen)ActivatorUtilities.CreateInstance(_serviceProvider, screenType);
        }
        catch (Exception ex)
        {
            // Log error but don't throw - screen creation failure shouldn't crash the app
            Console.WriteLine($"Failed to create screen '{screenId}': {ex.Message}");
            return null;
        }
    }
    
    public IEnumerable<string> GetAvailableScreenIds()
    {
        return _screenTypes.Keys;
    }
    
    private void RegisterScreenTypes()
    {
        // Register all available screen types
        // This could be made more dynamic by using reflection to find all IScreen implementations
        _screenTypes["landing"] = typeof(StarConflictsRevolt.Clients.Bliss.Views.LandingScreen);
        _screenTypes["single-player-setup"] = typeof(StarConflictsRevolt.Clients.Bliss.Views.SinglePlayerSetupScreen);
        _screenTypes["galaxy"] = typeof(StarConflictsRevolt.Clients.Bliss.Views.GalaxyScreen);
        
        // Add more screen types as they are implemented
        // _screenTypes["multiplayer-setup"] = typeof(MultiplayerSetupScreen);
        // _screenTypes["join-game"] = typeof(JoinGameScreen);
        // _screenTypes["leaderboards"] = typeof(LeaderboardsScreen);
        // _screenTypes["galaxy"] = typeof(GalaxyScreen);
        // _screenTypes["debug-mode"] = typeof(DebugModeScreen);
    }
} 