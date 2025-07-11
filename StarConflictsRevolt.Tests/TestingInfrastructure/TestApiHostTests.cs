using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using StarConflictsRevolt.Tests.TestingInfrastructure;
using TUnit;

namespace StarConflictsRevolt.Tests.TestingInfrastructure;

public class TestApiHostTests
{
    [Test]
    public async Task Middleware_Logs_And_Asserts_All_Calls()
    {
        var calls = new List<string>();

        await using var apiHost = TestApiHost.Create()
            .WithMiddleware(next => async ctx =>
            {
                calls.Add($"{ctx.Request.Method} {ctx.Request.Path}");
                await next(ctx);
            })
            .With(HttpMethod.Get, "/foo", async ctx => await ctx.Response.WriteAsync("bar"))
            .Build(new Uri("http://127.0.0.1:0"));

        await apiHost.ExecuteAsync(async client =>
        {
            var resp = await client.GetAsync("/foo");
            var content = await resp.Content.ReadAsStringAsync();
            await Assert.That(content).IsEqualTo("bar");
            await Assert.That(calls).Contains("GET /foo");
        });
        
        // Ensure the middleware was called
        await Assert.That(calls).Contains("GET /foo");
    }
} 