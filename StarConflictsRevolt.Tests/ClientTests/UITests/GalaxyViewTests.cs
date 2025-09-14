using Microsoft.Playwright;

namespace StarConflictsRevolt.Tests.ClientTests.UITests;

/// <summary>
/// UI tests for the galaxy view functionality
/// </summary>
public class GalaxyViewTests : BaseUITest
{
    [Test]
    public async Task GalaxyPage_DisplaysCorrectTitle()
    {
        // Navigate to galaxy page
        await Page.GotoAsync($"{BaseUrl}/galaxy");
        await WaitForNavigationAsync();
        
        // Verify the page title
        await AssertElementContainsTextAsync("h1", "Galaxy View");
    }
    
    [Test]
    public async Task GalaxyPage_DisplaysSidebar()
    {
        // Navigate to galaxy page
        await Page.GotoAsync($"{BaseUrl}/galaxy");
        await WaitForNavigationAsync();
        
        // Verify sidebar is present
        await AssertElementVisibleAsync(".col-md-3.bg-dark");
        await AssertElementContainsTextAsync("h5", "Game Controls");
    }
    
    [Test]
    public async Task GalaxyPage_DisplaysGameControls()
    {
        // Navigate to galaxy page
        await Page.GotoAsync($"{BaseUrl}/galaxy");
        await WaitForNavigationAsync();
        
        // Verify game control buttons are present
        await AssertElementVisibleAsync("button:has-text('Fleet Manager')");
        await AssertElementVisibleAsync("button:has-text('Planet Manager')");
        await AssertElementVisibleAsync("button:has-text('Diplomacy')");
    }
    
    [Test]
    public async Task GalaxyPage_DisplaysGalaxyMap()
    {
        // Navigate to galaxy page
        await Page.GotoAsync($"{BaseUrl}/galaxy");
        await WaitForNavigationAsync();
        
        // Verify galaxy map container is present
        await AssertElementVisibleAsync(".galaxy-container");
        await AssertElementVisibleAsync(".galaxy-map");
    }
    
    [Test]
    public async Task GalaxyPage_HomeButton_ReturnsToHome()
    {
        // Navigate to galaxy page
        await Page.GotoAsync($"{BaseUrl}/galaxy");
        await WaitForNavigationAsync();
        
        // Click home button
        await ClickButtonByIconAsync("fa-home");
        await WaitForNavigationAsync();
        
        // Verify we're back on the home page
        await Page.WaitForURLAsync("**/");
        await AssertElementContainsTextAsync("h1", "Star Conflicts Revolt");
    }
    
    [Test]
    public async Task GalaxyPage_DisplaysLoadingState()
    {
        // Navigate to galaxy page
        await Page.GotoAsync($"{BaseUrl}/galaxy");
        await WaitForNavigationAsync();
        
        // Check for loading state (either loading spinner or galaxy content)
        var loadingSpinner = Page.Locator(".fa-spinner.fa-spin");
        var galaxyContent = Page.Locator(".planet");
        
        var hasLoading = await loadingSpinner.IsVisibleAsync();
        var hasContent = await galaxyContent.IsVisibleAsync();
        
        // Should have either loading state or content
        await Assert.That(hasLoading || hasContent).IsTrue();
    }
    
    [Test]
    public async Task GalaxyPage_DisplaysFleetManagementSection()
    {
        // Navigate to galaxy page
        await Page.GotoAsync($"{BaseUrl}/galaxy");
        await WaitForNavigationAsync();
        
        // Verify fleet management section is present
        await AssertElementContainsTextAsync("h6", "Fleet Management");
    }
    
    [Test]
    public async Task GalaxyPage_DisplaysQuickActions()
    {
        // Navigate to galaxy page
        await Page.GotoAsync($"{BaseUrl}/galaxy");
        await WaitForNavigationAsync();
        
        // Verify quick actions section is present
        await AssertElementContainsTextAsync("h6", "Quick Actions");
    }
}
