using System.Diagnostics;
using Raven.Client.Documents;
using Raven.Embedded;

namespace StarConflictsRevolt.Tests.TestingInfrastructure;

public static class RavenTestServer
{
    private static readonly object _lock = new();
    private static IDocumentStore? _documentStore;
    private static bool _started = false;
    private static string? _dataDir;
    public static IDocumentStore DocumentStore
    {
        get
        {
            if (_documentStore != null) return _documentStore;
            lock (_lock)
            {
                if (_documentStore != null) return _documentStore;
                if (!_started)
                {
                    // Use a fixed temp directory for the process
                    _dataDir = Path.Combine(Path.GetTempPath(), "StarConflictsRevoltTest_RavenDbShared");
                    if (Directory.Exists(_dataDir))
                    {
                        try { Directory.Delete(_dataDir, true); } catch { /* ignore cleanup errors */ }
                    }
                    Debug.WriteLine($"[RavenDB] Starting EmbeddedServer at {_dataDir}...");
                    EmbeddedServer.Instance.StartServer(new ServerOptions { DataDirectory = _dataDir });
                    Debug.WriteLine("[RavenDB] EmbeddedServer started.");
                    _started = true;
                }
                _documentStore = EmbeddedServer.Instance.GetDocumentStore("StarConflictsRevoltTestShared");
                Debug.WriteLine("[RavenDB] DocumentStore acquired.");
                return _documentStore;
            }
        }
    }
} 