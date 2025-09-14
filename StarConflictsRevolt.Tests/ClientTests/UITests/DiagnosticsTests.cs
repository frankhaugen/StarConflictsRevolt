using Microsoft.Playwright;

namespace StarConflictsRevolt.Tests.ClientTests.UITests;

/// <summary>
/// UI tests for the diagnostics page
/// </summary>
public class DiagnosticsTests : BaseUITest
{
    [Test]
    [Timeout(30_000)]
    public async Task DiagnosticsPage_DisplaysCorrectTitle(CancellationToken cancellationToken)
    {
        // Navigate to diagnostics page
        await Page.GotoAsync($"{BaseUrl}/diagnostics");
        await WaitForNavigationAsync();
        
        // Verify the page title
        await AssertElementContainsTextAsync("h1", "System Diagnostics");
    }
    
    [Test]
    [Timeout(30_000)]
    public async Task DiagnosticsPage_DisplaysConnectionStatus(CancellationToken cancellationToken)
    {
        // Navigate to diagnostics page
        await Page.GotoAsync($"{BaseUrl}/diagnostics");
        await WaitForNavigationAsync();
        
        // Verify connection status section
        await AssertElementContainsTextAsync("h5:has-text('Connection Status')", "Connection Status");
        await AssertElementVisibleAsync(".status-indicator.connected");
    }
    
    [Test]
    [Timeout(30_000)]
    public async Task DiagnosticsPage_DisplaysSessionInformation(CancellationToken cancellationToken)
    {
        // Navigate to diagnostics page
        await Page.GotoAsync($"{BaseUrl}/diagnostics");
        await WaitForNavigationAsync();
        
        // Verify session information section
        await AssertElementContainsTextAsync("h5:has-text('Session Information')", "Session Information");
    }
    
    [Test]
    [Timeout(30_000)]
    public async Task DiagnosticsPage_DisplaysSignalRStatistics(CancellationToken cancellationToken)
    {
        // Navigate to diagnostics page
        await Page.GotoAsync($"{BaseUrl}/diagnostics");
        await WaitForNavigationAsync();
        
        // Verify SignalR statistics section
        await AssertElementContainsTextAsync("h5:has-text('SignalR Statistics')", "SignalR Statistics");
        await AssertElementContainsTextAsync("text=Messages Received");
        await AssertElementContainsTextAsync("text=Reconnections");
    }
    
    [Test]
    [Timeout(30_000)]
    public async Task DiagnosticsPage_DisplaysHTTPStatistics(CancellationToken cancellationToken)
    {
        // Navigate to diagnostics page
        await Page.GotoAsync($"{BaseUrl}/diagnostics");
        await WaitForNavigationAsync();
        
        // Verify HTTP statistics section
        await AssertElementContainsTextAsync("h5:has-text('HTTP Statistics')", "HTTP Statistics");
        await AssertElementContainsTextAsync("text=Requests Sent");
        await AssertElementContainsTextAsync("text=Errors");
    }
    
    [Test]
    [Timeout(30_000)]
    public async Task DiagnosticsPage_DisplaysActivityLog(CancellationToken cancellationToken)
    {
        // Navigate to diagnostics page
        await Page.GotoAsync($"{BaseUrl}/diagnostics");
        await WaitForNavigationAsync();
        
        // Verify activity log section
        await AssertElementContainsTextAsync("h5:has-text('Activity Log')", "Activity Log");
        await AssertElementVisibleAsync(".activity-log");
    }
    
    [Test]
    [Timeout(30_000)]
    public async Task DiagnosticsPage_DisplaysPerformanceMetrics(CancellationToken cancellationToken)
    {
        // Navigate to diagnostics page
        await Page.GotoAsync($"{BaseUrl}/diagnostics");
        await WaitForNavigationAsync();
        
        // Verify performance metrics section
        await AssertElementContainsTextAsync("h5:has-text('Performance Metrics')", "Performance Metrics");
        await AssertElementContainsTextAsync("text=Memory Usage");
        await AssertElementContainsTextAsync("text=CPU Usage");
        await AssertElementContainsTextAsync("text=Uptime");
    }
    
    [Test]
    [Timeout(30_000)]
    public async Task DiagnosticsPage_RefreshButton_UpdatesData(CancellationToken cancellationToken)
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
    [Timeout(30_000)]
    public async Task DiagnosticsPage_BackButton_ReturnsToHome(CancellationToken cancellationToken)
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
    [Timeout(30_000)]
    public async Task DiagnosticsPage_ClearLogButton_ClearsActivityLog(CancellationToken cancellationToken)
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
