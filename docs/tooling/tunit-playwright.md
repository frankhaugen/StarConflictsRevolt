# TUnit and Playwright Integration Documentation

## Overview

TUnit is a modern .NET testing framework that provides excellent integration with Playwright for UI testing. This document explains how to effectively use TUnit with Playwright for testing Blazor applications.

## Key Concepts

### 1. TUnit Framework
- **Modern .NET Testing**: Built specifically for .NET 6+ with modern C# features
- **Fluent Assertions**: Provides readable and maintainable assertion syntax
- **Async/Await Support**: Full support for asynchronous testing patterns
- **Data-Driven Testing**: Built-in support for parameterized tests
- **Lifecycle Management**: Automatic setup and teardown of test resources

### 2. Playwright Integration
- **Cross-Browser Testing**: Support for Chromium, Firefox, and WebKit
- **Automatic Browser Management**: Handles browser lifecycle automatically
- **Rich API**: Comprehensive set of methods for UI interaction
- **Screenshot and Video**: Built-in capabilities for debugging and reporting

## Installation and Setup

### Required Packages
```xml
<PackageReference Include="TUnit" Version="0.25.21" />
<PackageReference Include="TUnit.Playwright" Version="0.25.21" />
<PackageReference Include="Microsoft.Playwright" Version="1.52.0" />
```

### Basic Test Class Structure
```csharp
using TUnit.Playwright;

public class BlazorAppUITests : PageTest
{
    [Test]
    public async Task HomePage_Should_Display_Correct_Title()
    {
        await Page.GotoAsync("https://localhost:5001");
        var title = await Page.TitleAsync();
        await Assert.That(title).IsEqualTo("Expected Title");
    }
}
```

## Core Components

### 1. PageTest Base Class

The `PageTest` base class provides automatic lifecycle management for Playwright objects:

#### Available Properties
- **`Page`**: Playwright `IPage` object for browser interactions
- **`Context`**: Playwright `IBrowserContext` for browser context management
- **`Browser`**: Playwright `IBrowser` object
- **`Playwright`**: Playwright `IPlaywright` instance

#### Browser Configuration
```csharp
public class BlazorAppUITests : PageTest
{
    protected override string BrowserName => "chromium"; // or "firefox", "webkit"
    
    // Test methods...
}
```

### 2. Test Lifecycle Methods

TUnit provides several attributes for test lifecycle management:

#### Before and After Methods
```csharp
public class BlazorAppUITests : PageTest
{
    [Before]
    public async Task Setup()
    {
        // Runs before each test
        await Page.SetViewportSizeAsync(1920, 1080);
    }

    [After]
    public async Task Cleanup()
    {
        // Runs after each test
        // Cleanup code here
    }

    [Test]
    public async Task MyTest()
    {
        // Test implementation
    }
}
```

#### Class-Level Setup and Teardown
```csharp
public class BlazorAppUITests : PageTest
{
    [BeforeAll]
    public static async Task ClassSetup()
    {
        // Runs once before all tests in the class
    }

    [AfterAll]
    public static async Task ClassCleanup()
    {
        // Runs once after all tests in the class
    }
}
```

### 3. TUnit Assertions

TUnit provides fluent assertions that work seamlessly with async operations:

#### Basic Assertions
```csharp
// String assertions
await Assert.That(actualValue).IsEqualTo("expected");
await Assert.That(actualValue).IsNotEqualTo("unexpected");
await Assert.That(actualValue).Contains("substring");
await Assert.That(actualValue).StartsWith("prefix");
await Assert.That(actualValue).EndsWith("suffix");

// Boolean assertions
await Assert.That(condition).IsTrue();
await Assert.That(condition).IsFalse();

// Null assertions
await Assert.That(value).IsNotNull();
await Assert.That(value).IsNull();

// Collection assertions
await Assert.That(collection).IsEmpty();
await Assert.That(collection).IsNotEmpty();
await Assert.That(collection).HasCount(5);
await Assert.That(collection).Contains("item");
```

#### Advanced Assertions
```csharp
// Numeric assertions
await Assert.That(number).IsGreaterThan(10);
await Assert.That(number).IsLessThan(100);
await Assert.That(number).IsBetween(5, 15);

// Exception assertions
await Assert.That(() => methodThatThrows()).Throws<ArgumentException>();
await Assert.That(() => methodThatThrows()).Throws<ArgumentException>("Expected message");

// Custom assertions
await Assert.That(value).Satisfies(x => x.Length > 5);
```

### 4. Data-Driven Testing

TUnit supports multiple ways to create data-driven tests:

#### Arguments Attribute
```csharp
[Test]
[Arguments("https://localhost:5001", "Home Page")]
[Arguments("https://localhost:5001/about", "About Us")]
[Arguments("https://localhost:5001/contact", "Contact")]
public async Task Page_Should_Display_Correct_Title(string url, string expectedTitle)
{
    await Page.GotoAsync(url);
    var title = await Page.TitleAsync();
    await Assert.That(title).IsEqualTo(expectedTitle);
}
```

#### DataSource Attribute
```csharp
[Test]
[DataSource<TestDataProvider>]
public async Task Login_With_Valid_Credentials(string username, string password, bool shouldSucceed)
{
    await Page.GotoAsync("https://localhost:5001/login");
    await Page.FillAsync("#username", username);
    await Page.FillAsync("#password", password);
    await Page.ClickAsync("#loginButton");
    
    if (shouldSucceed)
    {
        await Assert.That(Page.Url).Contains("/dashboard");
    }
    else
    {
        await Assert.That(Page.Locator("#error-message")).IsVisible();
    }
}
```

## Playwright Integration Patterns

### 1. Page Navigation and Interaction
```csharp
[Test]
public async Task User_Can_Navigate_To_Login_Page()
{
    // Navigate to the application
    await Page.GotoAsync("https://localhost:5001");
    
    // Click on login link
    await Page.ClickAsync("a[href='/login']");
    
    // Wait for navigation
    await Page.WaitForURLAsync("**/login");
    
    // Verify we're on the login page
    await Assert.That(Page.Url).Contains("/login");
}
```

### 2. Form Interaction
```csharp
[Test]
public async Task User_Can_Submit_Login_Form()
{
    await Page.GotoAsync("https://localhost:5001/login");
    
    // Fill form fields
    await Page.FillAsync("#username", "testuser");
    await Page.FillAsync("#password", "password123");
    
    // Submit form
    await Page.ClickAsync("#loginButton");
    
    // Wait for response
    await Page.WaitForSelectorAsync("#dashboard", new PageWaitForSelectorOptions 
    { 
        State = WaitForSelectorState.Visible,
        Timeout = 5000 
    });
    
    // Verify success
    await Assert.That(Page.Locator("#dashboard")).IsVisible();
}
```

### 3. Element Waiting and Verification
```csharp
[Test]
public async Task Dynamic_Content_Loads_Correctly()
{
    await Page.GotoAsync("https://localhost:5001/dashboard");
    
    // Wait for dynamic content to load
    await Page.WaitForSelectorAsync(".loading", new PageWaitForSelectorOptions 
    { 
        State = WaitForSelectorState.Detached,
        Timeout = 10000 
    });
    
    // Verify content is displayed
    var content = await Page.TextContentAsync(".data-content");
    await Assert.That(content).IsNotNull();
    await Assert.That(content).IsNotEmpty();
}
```

### 4. Screenshot and Video Capture
```csharp
[Test]
public async Task Capture_Screenshot_On_Failure()
{
    try
    {
        await Page.GotoAsync("https://localhost:5001");
        await Page.ClickAsync("#non-existent-button");
    }
    catch
    {
        // Capture screenshot for debugging
        await Page.ScreenshotAsync(new PageScreenshotOptions 
        { 
            Path = "screenshot.png",
            FullPage = true 
        });
        throw;
    }
}
```

## Best Practices

### 1. Test Organization
```csharp
// Group related tests in classes
public class AuthenticationTests : PageTest
{
    [Test]
    public async Task Valid_Login_Succeeds() { /* ... */ }
    
    [Test]
    public async Task Invalid_Login_Fails() { /* ... */ }
    
    [Test]
    public async Task Logout_Works_Correctly() { /* ... */ }
}

public class NavigationTests : PageTest
{
    [Test]
    public async Task Home_Page_Loads() { /* ... */ }
    
    [Test]
    public async Task About_Page_Accessible() { /* ... */ }
}
```

### 2. Page Object Pattern
```csharp
public class LoginPage
{
    private readonly IPage _page;
    
    public LoginPage(IPage page)
    {
        _page = page;
    }
    
    public async Task NavigateTo()
    {
        await _page.GotoAsync("https://localhost:5001/login");
    }
    
    public async Task FillCredentials(string username, string password)
    {
        await _page.FillAsync("#username", username);
        await _page.FillAsync("#password", password);
    }
    
    public async Task ClickLogin()
    {
        await _page.ClickAsync("#loginButton");
    }
    
    public async Task<bool> IsLoggedIn()
    {
        return await _page.Locator("#dashboard").IsVisibleAsync();
    }
}

// Usage in tests
[Test]
public async Task Login_With_Valid_Credentials()
{
    var loginPage = new LoginPage(Page);
    await loginPage.NavigateTo();
    await loginPage.FillCredentials("testuser", "password123");
    await loginPage.ClickLogin();
    
    await Assert.That(await loginPage.IsLoggedIn()).IsTrue();
}
```

### 3. Configuration and Environment Setup
```csharp
public class BlazorAppUITests : PageTest
{
    protected override string BrowserName => "chromium";
    
    private string BaseUrl => Environment.GetEnvironmentVariable("TEST_BASE_URL") 
        ?? "https://localhost:5001";
    
    [Before]
    public async Task Setup()
    {
        // Set viewport size
        await Page.SetViewportSizeAsync(1920, 1080);
        
        // Set up console logging
        Page.Console += (_, msg) => Console.WriteLine($"Console: {msg.Type} - {msg.Text}");
        
        // Set up request/response logging
        Page.Request += (_, request) => Console.WriteLine($"Request: {request.Method} {request.Url}");
        Page.Response += (_, response) => Console.WriteLine($"Response: {response.Status} {response.Url}");
    }
}
```

### 4. Error Handling and Debugging
```csharp
[Test]
public async Task Robust_Element_Interaction()
{
    await Page.GotoAsync("https://localhost:5001");
    
    try
    {
        // Wait for element with timeout
        await Page.WaitForSelectorAsync("#dynamic-element", new PageWaitForSelectorOptions 
        { 
            State = WaitForSelectorState.Visible,
            Timeout = 10000 
        });
        
        // Interact with element
        await Page.ClickAsync("#dynamic-element");
        
        // Verify result
        await Assert.That(Page.Locator("#result")).IsVisible();
    }
    catch (TimeoutException)
    {
        // Capture screenshot for debugging
        await Page.ScreenshotAsync(new PageScreenshotOptions 
        { 
            Path = $"timeout-{DateTime.Now:yyyyMMdd-HHmmss}.png" 
        });
        throw;
    }
}
```

## Common Patterns for Blazor Applications

### 1. Testing Blazor Server Components
```csharp
[Test]
public async Task Blazor_Component_Renders_Correctly()
{
    await Page.GotoAsync("https://localhost:5001/component-test");
    
    // Wait for Blazor to load
    await Page.WaitForFunctionAsync("() => window.Blazor !== undefined");
    
    // Wait for component to render
    await Page.WaitForSelectorAsync(".blazor-component");
    
    // Verify component content
    var componentText = await Page.TextContentAsync(".blazor-component");
    await Assert.That(componentText).Contains("Expected Content");
}
```

### 2. Testing SignalR Connections
```csharp
[Test]
public async Task SignalR_Connection_Establishes()
{
    await Page.GotoAsync("https://localhost:5001/chat");
    
    // Wait for SignalR connection
    await Page.WaitForFunctionAsync("() => window.connection && window.connection.state === 'Connected'");
    
    // Verify connection status
    var connectionStatus = await Page.TextContentAsync("#connection-status");
    await Assert.That(connectionStatus).IsEqualTo("Connected");
}
```

### 3. Testing Form Validation
```csharp
[Test]
public async Task Form_Validation_Works_Correctly()
{
    await Page.GotoAsync("https://localhost:5001/contact");
    
    // Submit empty form
    await Page.ClickAsync("#submit-button");
    
    // Verify validation messages
    await Assert.That(Page.Locator(".validation-error")).IsVisible();
    await Assert.That(Page.Locator(".validation-error")).ContainsText("Name is required");
    
    // Fill required fields
    await Page.FillAsync("#name", "John Doe");
    await Page.FillAsync("#email", "john@example.com");
    
    // Submit again
    await Page.ClickAsync("#submit-button");
    
    // Verify success
    await Assert.That(Page.Locator(".success-message")).IsVisible();
}
```

## Running Tests

### Command Line
```bash
# Run all tests
dotnet test

# Run specific test class
dotnet test --filter "ClassName=BlazorAppUITests"

# Run with specific browser
dotnet test --filter "BrowserName=firefox"

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"
```

### Visual Studio / Rider
- Use the built-in test runner
- Set breakpoints for debugging
- View test results and coverage

## Troubleshooting

### Common Issues

1. **Browser Not Found**
   - Ensure Playwright browsers are installed: `playwright install`
   - Check browser name configuration

2. **Timeout Issues**
   - Increase timeout values for slow operations
   - Use proper waiting strategies

3. **Element Not Found**
   - Use proper selectors
   - Wait for elements to be visible/attached
   - Check for dynamic content loading

4. **Async/Await Issues**
   - Always use `await` with Playwright methods
   - Use `Assert.That().IsEqualTo()` for async assertions

### Debugging Tips

1. **Screenshots**: Capture screenshots on failure
2. **Console Logs**: Monitor browser console output
3. **Network Logs**: Track network requests and responses
4. **Slow Motion**: Use `Page.SetDefaultTimeout()` for debugging

## Conclusion

TUnit with Playwright provides a powerful combination for testing Blazor applications. The fluent assertion syntax, automatic lifecycle management, and comprehensive Playwright integration make it an excellent choice for UI testing. By following the patterns and best practices outlined in this documentation, you can create robust, maintainable, and reliable UI tests for your Blazor applications.
