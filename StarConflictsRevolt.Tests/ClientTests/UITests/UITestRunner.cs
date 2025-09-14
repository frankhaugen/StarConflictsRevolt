using Microsoft.Playwright;
using TUnit.Playwright;

namespace StarConflictsRevolt.Tests.ClientTests.UITests;

/// <summary>
/// Main test runner for UI tests
/// </summary>
public class UITestRunner
{
    private static BlazorTestHost? _testHost;
    
    [SetUp]
    public static async Task SetupTestEnvironment()
    {
        // Start the test host
        _testHost = new BlazorTestHost();
        await _testHost.StartAsync();
        
        // Wait for the application to be ready
        await Task.Delay(2000);
    }
    
    [TearDown]
    public static async Task CleanupTestEnvironment()
    {
        if (_testHost != null)
        {
            await _testHost.StopAsync();
            _testHost.Dispose();
            _testHost = null;
        }
    }
    
    public static string GetBaseUrl()
    {
        return _testHost?.BaseUrl ?? "https://localhost:7001";
    }
}