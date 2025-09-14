using Microsoft.Playwright;

namespace StarConflictsRevolt.Tests.ClientTests.UITests;

/// <summary>
/// UI tests for game functionality
/// </summary>
public class GameFunctionalityTests : BaseUITest
{
    [Test]
    public async Task SinglePlayerGame_CreatesSession_DisplaysGameInterface()
    {
        // Navigate to single player page
        await Page.GotoAsync($"{BaseUrl}/singleplayer");
        await WaitForNavigationAsync();
        
        // Wait for game to load
        await Page.WaitForSelectorAsync(".game-container", new PageWaitForSelectorOptions 
        { 
            State = WaitForSelectorState.Visible,
            Timeout = 10000 
        });
        
        // Verify game interface elements are present
        await AssertElementVisibleAsync(".col-md-3.bg-dark"); // Sidebar
        await AssertElementVisibleAsync(".game-container"); // Main game area
        await AssertElementContainsTextAsync("h5", "Game Controls");
    }
    
    [Test]
    public async Task SinglePlayerGame_DisplaysGameControls()
    {
        // Navigate to single player page
        await Page.GotoAsync($"{BaseUrl}/singleplayer");
        await WaitForNavigationAsync();
        
        // Wait for game to load
        await Page.WaitForSelectorAsync(".game-container");
        
        // Verify game control buttons are present
        await AssertElementVisibleAsync("button:has-text('Fleet Manager')");
        await AssertElementVisibleAsync("button:has-text('Planet Manager')");
        await AssertElementVisibleAsync("button:has-text('Diplomacy')");
        await AssertElementVisibleAsync("button:has-text('Research')");
    }
    
    [Test]
    public async Task SinglePlayerGame_DisplaysResources()
    {
        // Navigate to single player page
        await Page.GotoAsync($"{BaseUrl}/singleplayer");
        await WaitForNavigationAsync();
        
        // Wait for game to load
        await Page.WaitForSelectorAsync(".game-container");
        
        // Verify resource display
        await AssertElementContainsTextAsync("h6", "Resources");
        await AssertElementContainsTextAsync("Credits:");
        await AssertElementContainsTextAsync("Materials:");
        await AssertElementContainsTextAsync("Fuel:");
    }
    
    [Test]
    public async Task SinglePlayerGame_DisplaysGameStatus()
    {
        // Navigate to single player page
        await Page.GotoAsync($"{BaseUrl}/singleplayer");
        await WaitForNavigationAsync();
        
        // Wait for game to load
        await Page.WaitForSelectorAsync(".game-container");
        
        // Verify game status display
        await AssertElementContainsTextAsync("h6", "Game Status");
        await AssertElementContainsTextAsync("Turn:");
        await AssertElementContainsTextAsync("Phase:");
    }
    
    [Test]
    public async Task SinglePlayerGame_FleetManagerButton_OpensFleetManager()
    {
        // Navigate to single player page
        await Page.GotoAsync($"{BaseUrl}/singleplayer");
        await WaitForNavigationAsync();
        
        // Wait for game to load
        await Page.WaitForSelectorAsync(".game-container");
        
        // Click fleet manager button
        await ClickButtonByTextAsync("Fleet Manager");
        
        // Verify fleet manager modal is displayed
        await AssertElementVisibleAsync(".fleet-manager");
        await AssertElementContainsTextAsync("h5", "Fleet Manager");
    }
    
    [Test]
    public async Task SinglePlayerGame_PlanetManagerButton_OpensPlanetManager()
    {
        // Navigate to single player page
        await Page.GotoAsync($"{BaseUrl}/singleplayer");
        await WaitForNavigationAsync();
        
        // Wait for game to load
        await Page.WaitForSelectorAsync(".game-container");
        
        // Click planet manager button
        await ClickButtonByTextAsync("Planet Manager");
        
        // Verify planet manager modal is displayed
        await AssertElementVisibleAsync(".planet-manager");
        await AssertElementContainsTextAsync("h5", "Planet Manager");
    }
    
    [Test]
    public async Task SinglePlayerGame_DisplaysGameMessages()
    {
        // Navigate to single player page
        await Page.GotoAsync($"{BaseUrl}/singleplayer");
        await WaitForNavigationAsync();
        
        // Wait for game to load
        await Page.WaitForSelectorAsync(".game-container");
        
        // Wait for welcome message to appear
        await Page.WaitForSelectorAsync(".game-messages", new PageWaitForSelectorOptions 
        { 
            State = WaitForSelectorState.Visible,
            Timeout = 5000 
        });
        
        // Verify game messages are displayed
        await AssertElementContainsTextAsync("Game Messages");
        await AssertElementContainsTextAsync("Welcome to Star Conflicts Revolt!");
    }
    
    [Test]
    public async Task SinglePlayerGame_HomeButton_ReturnsToHome()
    {
        // Navigate to single player page
        await Page.GotoAsync($"{BaseUrl}/singleplayer");
        await WaitForNavigationAsync();
        
        // Wait for game to load
        await Page.WaitForSelectorAsync(".game-container");
        
        // Click home button
        await ClickButtonByIconAsync("fa-home");
        await WaitForNavigationAsync();
        
        // Verify we're back on the home page
        await Page.WaitForURLAsync("**/");
        await AssertElementContainsTextAsync("h1", "Star Conflicts Revolt");
    }
}
