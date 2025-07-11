using System.Diagnostics;
using System.Threading;
using Raven.Client.Documents;
using Raven.Embedded;

namespace StarConflictsRevolt.Tests.TestingInfrastructure;

public static class RavenTestServer
{
    private static readonly object _lock = new();
    private static IDocumentStore? _documentStore;
    private static bool _started = false;
    private static string? _dataDir;
    private static readonly string MutexName = "StarConflictsRevolt_RavenDb_Mutex";
    private static Mutex? _mutex; 
    public static IDocumentStore DocumentStore
    {
        get
        {
            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            var stack = Environment.StackTrace;
            Debug.WriteLine($"[RavenTestServer] DocumentStore getter called. PID={pid}, TID={tid}\nStack:\n{stack}");
            if (_documentStore != null) return _documentStore;
            lock (_lock)
            {
                Debug.WriteLine($"[RavenTestServer] Entered lock. PID={pid}, TID={tid}");
                if (_documentStore != null) return _documentStore;
                if (!_started)
                {
                    _mutex = new Mutex(false, MutexName);
                    Debug.WriteLine($"[RavenTestServer] Waiting for Mutex '{MutexName}'. PID={pid}, TID={tid}");
                    _mutex.WaitOne(); // Wait for exclusive access
                    Debug.WriteLine($"[RavenTestServer] Acquired Mutex '{MutexName}'. PID={pid}, TID={tid}");
                    try
                    {
                        var uniqueDir = Path.Combine(
                            Path.GetTempPath(),
                            $"StarConflictsRevoltTest_RavenDb_{pid}_{Guid.NewGuid()}"
                        );
                        Directory.CreateDirectory(uniqueDir);
                        _dataDir = uniqueDir;
                        Debug.WriteLine($"[RavenTestServer] Starting EmbeddedServer at {_dataDir}. PID={pid}, TID={tid}");
                        EmbeddedServer.Instance.StartServer(new ServerOptions { DataDirectory = _dataDir });
                        Debug.WriteLine($"[RavenTestServer] EmbeddedServer started. PID={pid}, TID={tid}");
                        _started = true;
                    }
                    finally
                    {
                        Debug.WriteLine($"[RavenTestServer] Releasing Mutex '{MutexName}'. PID={pid}, TID={tid}");
                        _mutex.ReleaseMutex();
                    }
                }
                var dbName = $"TestDb_{Guid.NewGuid()}";
                Debug.WriteLine($"[RavenTestServer] Getting DocumentStore '{dbName}'. PID={pid}, TID={tid}");
                _documentStore = EmbeddedServer.Instance.GetDocumentStore(dbName);
                Debug.WriteLine($"[RavenTestServer] DocumentStore '{dbName}' acquired. PID={pid}, TID={tid}");
                return _documentStore;
            }
        }
    }
} 