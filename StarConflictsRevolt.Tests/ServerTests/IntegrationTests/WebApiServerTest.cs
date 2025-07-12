using FluentAssertions;
using StarConflictsRevolt.Server.WebApi.Eventing;
using StarConflictsRevolt.Tests.TestingInfrastructure;

namespace StarConflictsRevolt.Tests.ServerTests.IntegrationTests;

[GameServerDataSource]
public partial class WebApiServerTest(GameServerTestHost gameServer)
{
    [Test]
    public void WebApiServer_ShouldStartAndRespond()
    {
        // The application is already built and started by GameServerTestHost
        var app = gameServer.App;
        
        // Assert: Check if the application is running and can respond to requests
        app.Services.GetService(typeof(IEventStore))
            .Should().NotBeNull("because the event store should be registered in the service provider");
        
        var httpClient = new HttpClient { BaseAddress = new Uri($"http://localhost:{gameServer.GetPort()}") };
        var response = httpClient.GetAsync("/").GetAwaiter().GetResult();
        response.IsSuccessStatusCode.Should().BeTrue("because the root endpoint should respond successfully");
        
        Context.Current.OutputWriter.WriteLine(
            $"Web API server started at http://localhost:{gameServer.GetPort()} and responded successfully with status code {response.StatusCode} and content: {response.Content.ReadAsStringAsync().GetAwaiter().GetResult()}"
        );
    }
}