using FluentAssertions;
using StarConflictsRevolt.Server.WebApi.Eventing;
using StarConflictsRevolt.Tests.TestingInfrastructure;

namespace StarConflictsRevolt.Tests.ServerTests.IntegrationTests;

public class WebApiServerTest
{
    [Test]
    public void WebApiServer_ShouldStartAndRespond()
    {
        // Arrange: Get the web application from the test server
        using var webApiServerBuilder = new FullIntegrationTestWebApplicationBuilder();
        var app = webApiServerBuilder.Build();
        
        // Act: Start the application
        app.StartAsync().GetAwaiter().GetResult();
        
        // Assert: Check if the application is running and can respond to requests
        app.Services.GetService(typeof(IEventStore))
            .Should().NotBeNull("because the event store should be registered in the service provider");
        
        var httpClient = new HttpClient { BaseAddress = new Uri($"http://localhost:{webApiServerBuilder.GetPort()}") };
        var response = httpClient.GetAsync("/").GetAwaiter().GetResult();
        response.IsSuccessStatusCode.Should().BeTrue("because the root endpoint should respond successfully");
        
        Context.Current.OutputWriter.WriteLine(
            $"Web API server started at http://localhost:{webApiServerBuilder.GetPort()} and responded successfully with status code {response.StatusCode} and content: {response.Content.ReadAsStringAsync().GetAwaiter().GetResult()}"
        );
        
        // Stop the application after tests
        app.StopAsync().GetAwaiter().GetResult();
    }
}