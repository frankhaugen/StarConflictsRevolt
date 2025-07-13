using FluentAssertions;
using StarConflictsRevolt.Server.WebApi.Eventing;
using StarConflictsRevolt.Tests.TestingInfrastructure;

namespace StarConflictsRevolt.Tests.ServerTests.IntegrationTests;

[TestHostApplication]
public partial class WebApiServerTest(TestHostApplication testHost, CancellationToken cancellationToken)
{
    [Test]
    [Timeout(20_000)]
    public async Task WebApiServer_ShouldStartAndRespond(CancellationToken cancellationToken)
    {
        // The application is already built and started by TestHostApplication
        var app = testHost.Server;
        
        // Assert: Check if the application is running and can respond to requests
        app.Services.GetService(typeof(IEventStore))
            .Should().NotBeNull("because the event store should be registered in the service provider");
        
        var httpClient = new HttpClient { BaseAddress = new Uri($"http://localhost:{testHost.Port}") };
        var response = await httpClient.GetAsync("/", cancellationToken);
        response.IsSuccessStatusCode.Should().BeTrue("because the root endpoint should respond successfully");
        
        await Context.Current.OutputWriter.WriteLineAsync(
            $"Web API server started at http://localhost:{testHost.Port} and responded successfully with status code {response.StatusCode} and content: {await response.Content.ReadAsStringAsync(cancellationToken)}"
        );
    }
}