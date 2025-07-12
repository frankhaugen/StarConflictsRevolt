using System.Diagnostics;
using System.Threading;
using Raven.Client.Documents;
using Raven.Embedded;

namespace StarConflictsRevolt.Tests.TestingInfrastructure;

public class RavenTestServerProvider : IDocumentStoreProvider
{
    private readonly object _lock = new();
    private IDocumentStore? _documentStore;
    private bool _started = false;
    private string? _dataDir;
    private static readonly string MutexName = "StarConflictsRevolt_RavenDb_Mutex";
    private Mutex? _mutex;

    public IDocumentStore GetStore(string? dbName = null)
    {
        var pid = Process.GetCurrentProcess().Id;
        var tid = Thread.CurrentThread.ManagedThreadId;
        var stack = Environment.StackTrace;
        Console.WriteLine($"[RavenTestServerProvider] GetStore called. PID={pid}, TID={tid}\nStack:\n{stack}");
        if (_documentStore != null) return _documentStore;
        lock (_lock)
        {
            Console.WriteLine($"[RavenTestServerProvider] Entered lock. PID={pid}, TID={tid}");
            if (_documentStore != null) return _documentStore;
            if (!_started)
            {
                _mutex = new Mutex(false, MutexName);
                Console.WriteLine($"[RavenTestServerProvider] Waiting for Mutex '{MutexName}'. PID={pid}, TID={tid}");
                _mutex.WaitOne(); // Wait for exclusive access
                Console.WriteLine($"[RavenTestServerProvider] Acquired Mutex '{MutexName}'. PID={pid}, TID={tid}");
                try
                {
                    var uniqueDir = Path.Combine(
                        Path.GetTempPath(),
                        $"StarConflictsRevoltTest_RavenDb_{pid}_{Guid.NewGuid()}"
                    );
                    Directory.CreateDirectory(uniqueDir);
                    _dataDir = uniqueDir;
                    Console.WriteLine($"[RavenTestServerProvider] Starting EmbeddedServer at {_dataDir}. PID={pid}, TID={tid}");
                    EmbeddedServer.Instance.StartServer(new ServerOptions { DataDirectory = _dataDir });
                    Console.WriteLine($"[RavenTestServerProvider] EmbeddedServer started. PID={pid}, TID={tid}");
                    _started = true;
                }
                finally
                {
                    Console.WriteLine($"[RavenTestServerProvider] Releasing Mutex '{MutexName}'. PID={pid}, TID={tid}");
                    _mutex.ReleaseMutex();
                }
            }
            var name = dbName ?? $"TestDb_{Guid.NewGuid()}";
            Console.WriteLine($"[RavenTestServerProvider] Getting DocumentStore '{name}'. PID={pid}, TID={tid}");
            _documentStore = EmbeddedServer.Instance.GetDocumentStore(name);
            Console.WriteLine($"[RavenTestServerProvider] DocumentStore '{name}' acquired. PID={pid}, TID={tid}");
            return _documentStore;
        }
    }
} 