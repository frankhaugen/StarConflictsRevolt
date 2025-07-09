using FluentAssertions;
using StarConflictsRevolt.Server.Eventing;
using StarConflictsRevolt.Tests.TestingInfrastructure;

namespace StarConflictsRevolt.Tests.IntegrationTests;

public class WebApiServerTest
{
    private readonly WebApiTestServer _webApiServer = new();
    
    public WebApiServerTest()
    {
        // Initialize the web API test server
        _webApiServer.SetScheme("http");
    }
    
    [Test]
    public void WebApiServer_ShouldStartAndRespond()
    {
        // Arrange: Get the web application from the test server
        var app = _webApiServer.GetWebApplication();
        
        // Act: Start the application
        app.StartAsync().GetAwaiter().GetResult();
        
        // Assert: Check if the application is running and can respond to requests
        app.Services.GetService(typeof(IEventStore))
            .Should().NotBeNull("because the event store should be registered in the service provider");
        
        var httpClient = new HttpClient { BaseAddress = new Uri($"{_webApiServer.GetScheme()}://localhost:{_webApiServer.GetPort()}") };
        var response = httpClient.GetAsync("/").GetAwaiter().GetResult();
        response.IsSuccessStatusCode.Should().BeTrue("because the root endpoint should respond successfully");
        
        Context.Current.OutputWriter.WriteLine(
            $"Web API server started at {_webApiServer.GetScheme()}://localhost:{_webApiServer.GetPort()} and responded successfully with status code {response.StatusCode} and content: {response.Content.ReadAsStringAsync().GetAwaiter().GetResult()}"
        );
        
        // Stop the application after tests
        app.StopAsync().GetAwaiter().GetResult();
    }
}