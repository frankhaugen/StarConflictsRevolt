using Microsoft.AspNetCore.Builder;

namespace StarConflictsRevolt.Tests.TestingInfrastructure.InfrastructureTests;

public class MinimalHostLifecycleTest
{
    [Test]
    public async Task MinimalHost_Starts_And_Stops_Cleanly()
    {
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();
        await app.StartAsync();
        await app.StopAsync();
        await app.DisposeAsync();
        await Assert.That(true).IsTrue(); // If we get here, the host started and stopped cleanly
    }

    [Test]
    [Timeout(30_000)]
    public async Task MinimalHost_Handles_EmptyRequest(CancellationToken cancellationToken)
    {
        // Create TestHostApplication without client services to avoid SignalR reconnection issues
        var builder = new TestHostApplication(false);
        await builder.App.StartAsync(cancellationToken);

        await Task.Delay(1000, cancellationToken);

        var healthResponse = await builder.Client.RetrieveHealthCheckAsync(cancellationToken);
        var response = await healthResponse.Content.ReadAsStringAsync(cancellationToken);
        await Context.Current.OutputWriter.WriteLineAsync($"Health check response: ({healthResponse.StatusCode} {healthResponse.ReasonPhrase}) {response}");
        await Assert.That(healthResponse.IsSuccessStatusCode).IsTrue();

        await Task.Delay(1000, cancellationToken);

        await builder.App.StopAsync(cancellationToken);
        await builder.App.DisposeAsync();

        // Dispose the TestHostApplication to clean up resources
        builder.Dispose();
    }
}