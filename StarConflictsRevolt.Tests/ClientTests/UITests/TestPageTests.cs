using Microsoft.Playwright;
using TUnit.Playwright;
using TUnit;

namespace StarConflictsRevolt.Tests.ClientTests.UITests;

/// <summary>
/// UI tests for the Test.razor page functionality
/// </summary>
public class TestPageTests : BaseUITest
{
    [Test]
    [Timeout(30_000)]
    public async Task TestPage_DisplaysCorrectTitle(CancellationToken cancellationToken)
    {
        // Navigate to the test page
        await Page.GotoAsync($"{BaseUrl}/test");
        await WaitForNavigationAsync();
        
        // Verify the page title
        await AssertElementContainsTextAsync("[data-testid='test-page-title']", "Blazor Functionality Test");
    }
    
    [Test]
    [Timeout(30_000)]
    public async Task TestPage_DisplaysAllTestButtons(CancellationToken cancellationToken)
    {
        await Page.GotoAsync($"{BaseUrl}/test");
        await WaitForNavigationAsync();
        
        // Verify all test buttons are present
        await AssertElementVisibleAsync("[data-testid='test-button-click']");
        await AssertElementVisibleAsync("[data-testid='test-js-console-log']");
        await AssertElementVisibleAsync("[data-testid='test-counter']");
        await AssertElementVisibleAsync("[data-testid='test-pure-js']");
    }
    
    [Test]
    [Timeout(30_000)]
    public async Task TestPage_ButtonClick_UpdatesResults(CancellationToken cancellationToken)
    {
        await Page.GotoAsync($"{BaseUrl}/test");
        await WaitForNavigationAsync();
        
        // Initially should show no results message
        await AssertElementVisibleAsync("[data-testid='no-results-message']");
        
        // Click the test button
        await Page.GetByTestId("test-button-click").ClickAsync();
        
        // Wait for results to appear
        await Page.WaitForSelectorAsync("[data-testid='test-results-list']", new PageWaitForSelectorOptions 
        { 
            State = WaitForSelectorState.Visible,
            Timeout = 5000 
        });
        
        // Verify results are displayed
        await AssertElementVisibleAsync("[data-testid='test-results-list']");
        
        // Verify the no results message is gone
        var noResultsMessage = Page.Locator("[data-testid='no-results-message']");
        await Assert.That(await noResultsMessage.IsVisibleAsync()).IsFalse();
    }
    
    [Test]
    [Timeout(30_000)]
    public async Task TestPage_JavaScriptLogButton_UpdatesResults(CancellationToken cancellationToken)
    {
        await Page.GotoAsync($"{BaseUrl}/test");
        await WaitForNavigationAsync();
        
        // Click the JavaScript log button
        await Page.GetByTestId("test-js-console-log").ClickAsync();
        
        // Wait for results to appear
        await Page.WaitForSelectorAsync("[data-testid='test-results-list']", new PageWaitForSelectorOptions 
        { 
            State = WaitForSelectorState.Visible,
            Timeout = 5000 
        });
        
        // Verify results contain the expected text
        var resultsList = Page.Locator("[data-testid='test-results-list']");
        var resultsText = await resultsList.TextContentAsync();
        await Assert.That(resultsText).Contains("JavaScript console log test completed");
    }
    
    [Test]
    [Timeout(30_000)]
    public async Task TestPage_CounterButton_IncrementsCounter(CancellationToken cancellationToken)
    {
        await Page.GotoAsync($"{BaseUrl}/test");
        await WaitForNavigationAsync();
        
        // Get initial counter value
        var counterButton = Page.GetByTestId("test-counter");
        var initialText = await counterButton.TextContentAsync();
        var initialCount = ExtractCounterValue(initialText!);
        
        // Click the counter button
        await counterButton.ClickAsync();
        
        // Wait for the button text to update
        await Page.WaitForFunctionAsync(
            $"() => document.querySelector('[data-testid=\"test-counter\"]').textContent.includes('{initialCount + 1}')",
            new PageWaitForFunctionOptions { Timeout = 5000 }
        );
        
        // Verify counter incremented
        var updatedText = await counterButton.TextContentAsync();
        var updatedCount = ExtractCounterValue(updatedText!);
        await Assert.That(updatedCount).IsEqualTo(initialCount + 1);
    }
    
    [Test]
    [Timeout(30_000)]
    public async Task TestPage_PureJSButton_ExecutesJavaScript(CancellationToken cancellationToken)
    {
        await Page.GotoAsync($"{BaseUrl}/test");
        await WaitForNavigationAsync();
        
        // Set up console message listener
        var consoleMessages = new List<string>();
        Page.Console += (_, msg) => 
        {
            if (msg.Type == "log")
            {
                consoleMessages.Add(msg.Text);
            }
        };
        
        // Click the pure JS button
        await Page.GetByTestId("test-pure-js").ClickAsync();
        
        // Wait a moment for the console message
        await Task.Delay(1000);
        
        // Verify the console message was logged
        await Assert.That(consoleMessages.Any(msg => msg.Contains("Pure JS button clicked!"))).IsTrue();
    }
    
    [Test]
    [Timeout(30_000)]
    public async Task TestPage_InputField_UpdatesOnInput(CancellationToken cancellationToken)
    {
        await Page.GotoAsync($"{BaseUrl}/test");
        await WaitForNavigationAsync();
        
        const string testInput = "Test input value";
        
        // Type in the input field
        await Page.GetByTestId("test-input").FillAsync(testInput);
        
        // Wait for results to appear
        await Page.WaitForSelectorAsync("[data-testid='test-results-list']", new PageWaitForSelectorOptions 
        { 
            State = WaitForSelectorState.Visible,
            Timeout = 5000 
        });
        
        // Verify the input change was logged
        var resultsList = Page.Locator("[data-testid='test-results-list']");
        var resultsText = await resultsList.TextContentAsync();
        await Assert.That(resultsText).Contains($"Input changed to: '{testInput}'");
    }
    
    [Test]
    [Timeout(30_000)]
    public async Task TestPage_SelectDropdown_UpdatesOnSelection(CancellationToken cancellationToken)
    {
        await Page.GotoAsync($"{BaseUrl}/test");
        await WaitForNavigationAsync();
        
        // Select an option from the dropdown
        await Page.GetByTestId("test-select").SelectOptionAsync("option2");
        
        // Wait for results to appear
        await Page.WaitForSelectorAsync("[data-testid='test-results-list']", new PageWaitForSelectorOptions 
        { 
            State = WaitForSelectorState.Visible,
            Timeout = 5000 
        });
        
        // Verify the selection change was logged
        var resultsList = Page.Locator("[data-testid='test-results-list']");
        var resultsText = await resultsList.TextContentAsync();
        await Assert.That(resultsText).Contains("Select changed to: 'option2'");
    }
    
    [Test]
    [Timeout(30_000)]
    public async Task TestPage_BackToHomeButton_NavigatesToHome(CancellationToken cancellationToken)
    {
        await Page.GotoAsync($"{BaseUrl}/test");
        await WaitForNavigationAsync();
        
        // Click the back to home button
        await Page.GetByTestId("back-to-home").ClickAsync();
        
        // Wait for navigation
        await WaitForNavigationAsync();
        
        // Verify we're on the home page
        await Page.WaitForURLAsync("**/");
        await AssertElementVisibleAsync("[data-testid='home-title']");
    }
    
    [Test]
    [Timeout(30_000)]
    public async Task TestPage_MultipleButtonClicks_AccumulateResults(CancellationToken cancellationToken)
    {
        await Page.GotoAsync($"{BaseUrl}/test");
        await WaitForNavigationAsync();
        
        // Click multiple buttons
        await Page.GetByTestId("test-button-click").ClickAsync();
        await Page.GetByTestId("test-js-console-log").ClickAsync();
        await Page.GetByTestId("test-counter").ClickAsync();
        
        // Wait for results to appear
        await Page.WaitForSelectorAsync("[data-testid='test-results-list']", new PageWaitForSelectorOptions 
        { 
            State = WaitForSelectorState.Visible,
            Timeout = 5000 
        });
        
        // Verify multiple results are displayed
        var resultsList = Page.Locator("[data-testid='test-results-list']");
        var resultsText = await resultsList.TextContentAsync();
        
        await Assert.That(resultsText).Contains("Button clicked");
        await Assert.That(resultsText).Contains("JavaScript console log test completed");
        await Assert.That(resultsText).Contains("Counter incremented");
    }
    
    [Test]
    [Timeout(30_000)]
    public async Task TestPage_ConsoleLogging_WorksCorrectly(CancellationToken cancellationToken)
    {
        await Page.GotoAsync($"{BaseUrl}/test");
        await WaitForNavigationAsync();
        
        // Set up console message listener
        var consoleMessages = new List<string>();
        Page.Console += (_, msg) => 
        {
            consoleMessages.Add($"{msg.Type}: {msg.Text}");
        };
        
        // Click the test button which should trigger console logging
        await Page.GetByTestId("test-button-click").ClickAsync();
        
        // Wait a moment for console messages
        await Task.Delay(2000);
        
        // Verify console messages were logged
        var hasDirectJS = consoleMessages.Any(msg => msg.Contains("DIRECT JS: Test button clicked from Blazor"));
        var hasButtonClick = consoleMessages.Any(msg => msg.Contains("Button clicked: test-button"));
        
        await Assert.That(hasDirectJS || hasButtonClick).IsTrue();
    }
    
    private static int ExtractCounterValue(string buttonText)
    {
        // Extract counter value from text like "Test Counter (5)"
        var match = System.Text.RegularExpressions.Regex.Match(buttonText, @"\((\d+)\)");
        return match.Success ? int.Parse(match.Groups[1].Value) : 0;
    }
}
