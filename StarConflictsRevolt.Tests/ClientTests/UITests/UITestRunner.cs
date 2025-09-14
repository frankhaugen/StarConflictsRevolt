using Microsoft.Playwright;
using TUnit.Playwright;
using TUnit;
using StarConflictsRevolt.Tests.ClientTests.UITests.TestHost;

namespace StarConflictsRevolt.Tests.ClientTests.UITests;

/// <summary>
/// Main test runner for UI tests
/// </summary>
public class UITestRunner
{
    private static BlazorTestHost? _testHost;
    private static readonly object _lock = new object();
    private static bool _isInitialized = false;
    
    public static async Task SetupTestEnvironment()
    {
        lock (_lock)
        {
            if (_isInitialized)
                return;
        }
        
        // Start the test host
        _testHost = new BlazorTestHost();
        await _testHost.StartAsync();
        
        // Wait for the application to be ready
        await Task.Delay(3000);
        
        lock (_lock)
        {
            _isInitialized = true;
        }
    }
    
    public static async Task CleanupTestEnvironment()
    {
        lock (_lock)
        {
            if (!_isInitialized)
                return;
        }
        
        if (_testHost != null)
        {
            await _testHost.StopAsync();
            _testHost.Dispose();
            _testHost = null;
        }
        
        lock (_lock)
        {
            _isInitialized = false;
        }
    }
    
    public static string GetBaseUrl()
    {
        if (_testHost == null)
        {
            // Try to start the test host if it's not running
            SetupTestEnvironment().GetAwaiter().GetResult();
        }
        
        if (_testHost == null)
        {
            throw new InvalidOperationException("Test host failed to start.");
        }
        
        return _testHost.BaseUrl;
    }
}