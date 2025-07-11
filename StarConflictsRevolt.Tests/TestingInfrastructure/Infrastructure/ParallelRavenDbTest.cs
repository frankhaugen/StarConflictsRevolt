using System.Threading.Tasks;
using Raven.Client.Documents;
using StarConflictsRevolt.Tests.TestingInfrastructure;
using TUnit;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace StarConflictsRevolt.Tests.TestingInfrastructure.Infrastructure;

public class ParallelRavenDbTest
{
    [Test]
    public async Task MultipleThreads_CanResolveDocumentStore_FromServiceCollection_InParallel()
    {
        const int parallelCount = 4;
        var results = new ConcurrentBag<IDocumentStore>();
        var tasks = new List<Task>();
        for (int i = 0; i < parallelCount; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                var services = new ServiceCollection();
                services.AddSingleton<IDocumentStore>(_ => RavenTestServer.DocumentStore);
                var provider = services.BuildServiceProvider();
                var store = provider.GetRequiredService<IDocumentStore>();
                // Open a session and perform a trivial operation
                using var session = store.OpenSession();
                session.Store(new { Test = "Parallel" + i }, "tests/" + i);
                session.SaveChanges();
                results.Add(store);
            }));
        }
        await Task.WhenAll(tasks);
        // Assert all DocumentStore instances are the same (reference equality)
        var first = results.First();
        foreach (var store in results)
        {
            // All resolved IDocumentStore instances should be the same singleton
            await Assert.That(object.ReferenceEquals(store, first)).IsTrue();
        }
        await Assert.That(results.Count).IsEqualTo(parallelCount);
    }
} 