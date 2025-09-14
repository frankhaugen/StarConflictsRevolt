using Microsoft.Playwright;

namespace StarConflictsRevolt.Tests.ClientTests.UITests;

/// <summary>
/// UI tests for the home page functionality
/// </summary>
public class HomePageTests : BaseUITest
{
    [Test]
    public async Task HomePage_DisplaysCorrectTitle()
    {
        // Verify the main title is displayed
        await AssertElementContainsTextAsync("h1", "Star Conflicts Revolt");
        await AssertElementContainsTextAsync(".lead", "A modern reimagining of Star Wars: Rebellion");
    }
    
    [Test]
    public async Task HomePage_DisplaysMainMenuButtons()
    {
        // Verify all main menu buttons are present
        await AssertElementVisibleAsync("button:has-text('Single Player')");
        await AssertElementVisibleAsync("button:has-text('Multiplayer')");
        await AssertElementVisibleAsync("button:has-text('Join Session')");
        await AssertElementVisibleAsync("button:has-text('Options')");
    }
    
    [Test]
    public async Task HomePage_SinglePlayerButton_NavigatesToSinglePlayer()
    {
        // Click the Single Player button
        await ClickButtonByTextAsync("Single Player");
        
        // Wait for navigation
        await WaitForNavigationAsync();
        
        // Verify we're on the single player page
        await Page.WaitForURLAsync("**/singleplayer");
        await AssertElementVisibleAsync("h1");
    }
    
    [Test]
    public async Task HomePage_MultiplayerButton_NavigatesToMultiplayer()
    {
        // Click the Multiplayer button
        await ClickButtonByTextAsync("Multiplayer");
        
        // Wait for navigation
        await WaitForNavigationAsync();
        
        // Verify we're on the multiplayer page
        await Page.WaitForURLAsync("**/multiplayer");
        await AssertElementVisibleAsync("h1");
    }
    
    [Test]
    public async Task HomePage_JoinSessionButton_NavigatesToSessions()
    {
        // Click the Join Session button
        await ClickButtonByTextAsync("Join Session");
        
        // Wait for navigation
        await WaitForNavigationAsync();
        
        // Verify we're on the sessions page
        await Page.WaitForURLAsync("**/sessions");
        await AssertElementContainsTextAsync("h3", "Available Sessions");
    }
    
    [Test]
    public async Task HomePage_DisplaysConnectionStatus()
    {
        // Verify connection status is displayed
        var connectionAlert = Page.Locator(".alert");
        await Assert.That(connectionAlert).IsVisibleAsync();
        
        // The status should be either connected or not connected
        var alertText = await connectionAlert.TextContentAsync();
        Assert.That(alertText, Is.Not.Null);
        Assert.That(alertText, Does.Contain("Connected").Or.Contain("Not connected"));
    }
    
    [Test]
    public async Task HomePage_BackButton_ReturnsToHome()
    {
        // Navigate to sessions page first
        await ClickButtonByTextAsync("Join Session");
        await WaitForNavigationAsync();
        
        // Click back button
        await ClickButtonByIconAsync("fa-arrow-left");
        await WaitForNavigationAsync();
        
        // Verify we're back on the home page
        await Page.WaitForURLAsync("**/");
        await AssertElementContainsTextAsync("h1", "Star Conflicts Revolt");
    }
}
