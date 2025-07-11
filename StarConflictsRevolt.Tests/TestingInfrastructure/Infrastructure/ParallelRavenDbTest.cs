using System.Threading.Tasks;
using Raven.Client.Documents;
using StarConflictsRevolt.Tests.TestingInfrastructure;
using TUnit;
using System.Collections.Concurrent;

namespace StarConflictsRevolt.Tests.TestingInfrastructure.Infrastructure;

public class ParallelRavenDbTest
{
    [Test]
    public async Task MultipleThreads_CanUseSharedEmbeddedRavenDb_InParallel()
    {
        const int parallelCount = 4;
        var results = new ConcurrentBag<string>();
        var tasks = new List<Task>();
        for (int i = 0; i < parallelCount; i++)
        {
            int localI = i;
            tasks.Add(Task.Run(() =>
            {
                // Get the shared DocumentStore
                var store = RavenTestServer.DocumentStore;
                // Open a session and perform a trivial operation
                using var session = store.OpenSession();
                var dbName = store.Database;
                // Optionally, store and save a trivial document
                session.Store(new { Test = "Parallel" + localI }, "tests/" + localI);
                session.SaveChanges();
                // Store the result for assertion
                results.Add(dbName);
            }));
        }
        await Task.WhenAll(tasks);
        foreach (var dbName in results)
        {
            await Assert.That(dbName).IsEqualTo("StarConflictsRevoltTestShared");
        }
    }
} 