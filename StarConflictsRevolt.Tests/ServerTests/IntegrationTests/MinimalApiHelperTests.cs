using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using StarConflictsRevolt.Tests.TestingInfrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using TUnit;

namespace StarConflictsRevolt.Tests.ServerTests.IntegrationTests;

public class MinimalApiHelperTests
{
    [Test]
    public async Task HealthGameEndpoint_ReturnsGameOn()
    {
        await using var host = TestApiHost.Create()
            .With(HttpMethod.Get, "/health/game", ctx => {
                Console.WriteLine("Handler invoked!");
                return Task.FromResult((IResult)TypedResults.Text("Game on!"));
            })
            .Build(new Uri("http://127.0.0.1:0"));

        await host.ExecuteAsync(async client =>
        {
            var response = await client.GetAsync("/health/game");
            var content = await response.Content.ReadAsStringAsync();
            await TestContext.Current?.OutputWriter.WriteLineAsync($"Base address: {client.BaseAddress}")!;
            client.BaseAddress!.Port.Should().NotBe(0);
            content.Should().Be("Game on!");
        });
    }
} 