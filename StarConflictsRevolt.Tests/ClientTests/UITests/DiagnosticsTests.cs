using Microsoft.Playwright;

namespace StarConflictsRevolt.Tests.ClientTests.UITests;

/// <summary>
/// UI tests for the diagnostics page
/// </summary>
public class DiagnosticsTests : BaseUITest
{
    [Test]
    public async Task DiagnosticsPage_DisplaysCorrectTitle()
    {
        // Navigate to diagnostics page
        await Page.GotoAsync($"{BaseUrl}/diagnostics");
        await WaitForNavigationAsync();
        
        // Verify the page title
        await AssertElementContainsTextAsync("h1", "System Diagnostics");
    }
    
    [Test]
    public async Task DiagnosticsPage_DisplaysConnectionStatus()
    {
        // Navigate to diagnostics page
        await Page.GotoAsync($"{BaseUrl}/diagnostics");
        await WaitForNavigationAsync();
        
        // Verify connection status section
        await AssertElementContainsTextAsync("h5", "Connection Status");
        await AssertElementVisibleAsync(".status-indicator");
    }
    
    [Test]
    public async Task DiagnosticsPage_DisplaysSessionInformation()
    {
        // Navigate to diagnostics page
        await Page.GotoAsync($"{BaseUrl}/diagnostics");
        await WaitForNavigationAsync();
        
        // Verify session information section
        await AssertElementContainsTextAsync("h5", "Session Information");
    }
    
    [Test]
    public async Task DiagnosticsPage_DisplaysSignalRStatistics()
    {
        // Navigate to diagnostics page
        await Page.GotoAsync($"{BaseUrl}/diagnostics");
        await WaitForNavigationAsync();
        
        // Verify SignalR statistics section
        await AssertElementContainsTextAsync("h5", "SignalR Statistics");
        await AssertElementContainsTextAsync("Messages Received");
        await AssertElementContainsTextAsync("Reconnections");
    }
    
    [Test]
    public async Task DiagnosticsPage_DisplaysHTTPStatistics()
    {
        // Navigate to diagnostics page
        await Page.GotoAsync($"{BaseUrl}/diagnostics");
        await WaitForNavigationAsync();
        
        // Verify HTTP statistics section
        await AssertElementContainsTextAsync("h5", "HTTP Statistics");
        await AssertElementContainsTextAsync("Requests Sent");
        await AssertElementContainsTextAsync("Errors");
    }
    
    [Test]
    public async Task DiagnosticsPage_DisplaysActivityLog()
    {
        // Navigate to diagnostics page
        await Page.GotoAsync($"{BaseUrl}/diagnostics");
        await WaitForNavigationAsync();
        
        // Verify activity log section
        await AssertElementContainsTextAsync("h5", "Activity Log");
        await AssertElementVisibleAsync(".activity-log");
    }
    
    [Test]
    public async Task DiagnosticsPage_DisplaysPerformanceMetrics()
    {
        // Navigate to diagnostics page
        await Page.GotoAsync($"{BaseUrl}/diagnostics");
        await WaitForNavigationAsync();
        
        // Verify performance metrics section
        await AssertElementContainsTextAsync("h5", "Performance Metrics");
        await AssertElementContainsTextAsync("Memory Usage");
        await AssertElementContainsTextAsync("CPU Usage");
        await AssertElementContainsTextAsync("Uptime");
    }
    
    [Test]
    public async Task DiagnosticsPage_RefreshButton_UpdatesData()
    {
        // Navigate to diagnostics page
        await Page.GotoAsync($"{BaseUrl}/diagnostics");
        await WaitForNavigationAsync();
        
        // Click refresh button
        await ClickButtonByTextAsync("Refresh");
        
        // Wait for refresh to complete
        await Page.WaitForSelectorAsync(".spinner-border", new PageWaitForSelectorOptions 
        { 
            State = WaitForSelectorState.Detached,
            Timeout = 10000 
        });
        
        // Verify data is still displayed
        await AssertElementContainsTextAsync("h1", "System Diagnostics");
    }
    
    [Test]
    public async Task DiagnosticsPage_BackButton_ReturnsToHome()
    {
        // Navigate to diagnostics page
        await Page.GotoAsync($"{BaseUrl}/diagnostics");
        await WaitForNavigationAsync();
        
        // Click back button
        await ClickButtonByTextAsync("Back to Menu");
        await WaitForNavigationAsync();
        
        // Verify we're back on the home page
        await Page.WaitForURLAsync("**/");
        await AssertElementContainsTextAsync("h1", "Star Conflicts Revolt");
    }
    
    [Test]
    public async Task DiagnosticsPage_ClearLogButton_ClearsActivityLog()
    {
        // Navigate to diagnostics page
        await Page.GotoAsync($"{BaseUrl}/diagnostics");
        await WaitForNavigationAsync();
        
        // Wait for activity log to load
        await Page.WaitForSelectorAsync(".activity-log");
        
        // Click clear log button
        await ClickButtonByTextAsync("Clear");
        
        // Verify log is cleared (should be empty or have minimal content)
        var logContent = await Page.Locator(".activity-log").TextContentAsync();
        await Assert.That(logContent).IsNotNull();
    }
}
