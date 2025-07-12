using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using StarConflictsRevolt.Tests.TestingInfrastructure;

namespace StarConflictsRevolt.Tests.ServerTests;

public class MinimalApiHelperTests
{
    [Test]
    public async Task HealthGameEndpoint_ReturnsGameOn()
    {
        await using var host = TestApiHost.Create()
            .With(HttpMethod.Get, "/health/game", ctx => ctx.Response.WriteAsync("Game on!"))
            .Build(new Uri("http://127.0.0.1:0"));

        await host.ExecuteAsync(async (client, services) =>
        {
            // Log all registered endpoints
            var endpointDataSource = services.GetRequiredService<Microsoft.AspNetCore.Routing.EndpointDataSource>();
            foreach (var endpoint in endpointDataSource.Endpoints)
            {
                Console.WriteLine($"Registered endpoint: {endpoint.DisplayName}");
                await TestContext.Current?.OutputWriter.WriteLineAsync($"Registered endpoint: {endpoint.DisplayName}")!;
            }

            var response = await client.GetAsync("/health/game");
            var content = await response.Content.ReadAsStringAsync();
            await TestContext.Current?.OutputWriter.WriteLineAsync($"Base address: {client.BaseAddress}")!;
            client.BaseAddress!.Port.Should().NotBe(0);
            content.Should().Be("Game on!");
        });
    }
} 