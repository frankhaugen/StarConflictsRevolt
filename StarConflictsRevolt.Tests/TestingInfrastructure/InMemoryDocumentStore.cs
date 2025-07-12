using System.Collections.Concurrent;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Raven.Client.Documents.Changes;
using Raven.Client.Documents.Operations;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Conventions;
using System.Security.Cryptography.X509Certificates;
using Raven.Client.Documents.Identity;
using Raven.Client.Documents.TimeSeries;
using Raven.Client.Documents.Subscriptions;
using Raven.Client.Documents.Smuggler;
using Raven.Client.Http;
using Raven.Client.Documents.BulkInsert;

namespace StarConflictsRevolt.Tests.TestingInfrastructure;

public sealed class InMemoryDocumentStore : Raven.Client.Documents.IDocumentStore, System.IAsyncDisposable
{
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, object> _docs = new();
    public event System.EventHandler? OnSessionCreated;
    public event System.EventHandler? BeforeDispose;
    public event EventHandler? AfterDispose;
    public event EventHandler<Raven.Client.Documents.Session.BeforeStoreEventArgs> OnBeforeStore;
    public event EventHandler<Raven.Client.Documents.Session.AfterSaveChangesEventArgs> OnAfterSaveChanges;
    public event EventHandler<Raven.Client.Documents.Session.BeforeDeleteEventArgs> OnBeforeDelete;
    public event EventHandler<Raven.Client.Documents.Session.BeforeQueryEventArgs> OnBeforeQuery;
    public event EventHandler<BeforeConversionToDocumentEventArgs> OnBeforeConversionToDocument;
    public event EventHandler<AfterConversionToDocumentEventArgs> OnAfterConversionToDocument;
    public event EventHandler<BeforeConversionToEntityEventArgs> OnBeforeConversionToEntity;
    public event EventHandler<AfterConversionToEntityEventArgs> OnAfterConversionToEntity;
    public event EventHandler<FailedRequestEventArgs> OnFailedRequest;
    public event EventHandler<BeforeRequestEventArgs> OnBeforeRequest;
    public event EventHandler<SucceedRequestEventArgs> OnSucceedRequest;
    public event EventHandler<TopologyUpdatedEventArgs> OnTopologyUpdated;
    public event EventHandler<SessionDisposingEventArgs> OnSessionDisposing;

    public string Identifier { get; set; } = "InMemoryDocumentStore";
    public string Database => "InMemory";
    public bool WasDisposed { get; private set; }
    public bool Initialized => true;
    public X509Certificate2 Certificate => null!;
    public DocumentConventions Conventions => new();

    event EventHandler<SessionCreatedEventArgs> IDocumentStore.OnSessionCreated
    {
        add
        {
            throw new NotImplementedException();
        }

        remove
        {
            throw new NotImplementedException();
        }
    }

    public RequestExecutor GetRequestExecutor(string database = null) => throw new NotSupportedException();
    public IHiLoIdGenerator HiLoIdGenerator => throw new NotSupportedException();

    public TimeSeriesOperations TimeSeries => throw new NotImplementedException();

    public string[] Urls => throw new NotImplementedException();

    public DocumentSubscriptions Subscriptions => throw new NotImplementedException();

    string IDocumentStore.Database { get => Database; set => throw new NotImplementedException(); }

    public MaintenanceOperationExecutor Maintenance => throw new NotImplementedException();

    public OperationExecutor Operations => throw new NotImplementedException();

    public DatabaseSmuggler Smuggler => throw new NotImplementedException();

    public IDisposable SetRequestsTimeout(TimeSpan timeout, string database = null) => throw new NotSupportedException();
    public IDisposable SetSessionTracking(bool enabled) => throw new NotSupportedException();
    public IDisposable DisableTopologyUpdates() => throw new NotSupportedException();
    public IDisposable AggressivelyCacheFor(TimeSpan cacheDuration, string database = null) => new DummyDisposable();
    public IDisposable AggressivelyCacheFor(TimeSpan cacheDuration, AggressiveCacheMode mode, string database = null) => new DummyDisposable();
    public ValueTask<IDisposable> AggressivelyCacheForAsync(TimeSpan cacheDuration, string database = null) => ValueTask.FromResult<IDisposable>(new DummyDisposable());
    public ValueTask<IDisposable> AggressivelyCacheForAsync(TimeSpan cacheDuration, AggressiveCacheMode mode, string database = null) => ValueTask.FromResult<IDisposable>(new DummyDisposable());
    public IDisposable AggressivelyCache(string database = null) => new DummyDisposable();
    public ValueTask<IDisposable> AggressivelyCacheAsync(string database = null) => ValueTask.FromResult<IDisposable>(new DummyDisposable());
    public IDisposable DisableAggressiveCaching(string database = null) => new DummyDisposable();
    public IDatabaseChanges Changes(string database = null) => new DummyDatabaseChanges();
    public ISingleNodeDatabaseChanges Changes(string database, string nodeTag) => new DummySingleNodeDatabaseChanges();
    public void ExecuteIndex(IAbstractIndexCreationTask task, string database = null) { }
    public void ExecuteIndexes(IEnumerable<IAbstractIndexCreationTask> tasks, string database = null) { }
    public Task ExecuteIndexAsync(IAbstractIndexCreationTask task, string database = null, CancellationToken token = default) => Task.CompletedTask;
    public Task ExecuteIndexesAsync(IEnumerable<IAbstractIndexCreationTask> tasks, string database = null, CancellationToken token = default) => Task.CompletedTask;
    public Task ExecuteIndexesAsync(IEnumerable<IAbstractIndexCreationTask> tasks, DocumentConventions conventions, string database = null, CancellationToken token = default) => Task.CompletedTask;
    public void Dispose() { WasDisposed = true; BeforeDispose?.Invoke(this, EventArgs.Empty); AfterDispose?.Invoke(this, EventArgs.Empty); }
    public ValueTask DisposeAsync() { Dispose(); return ValueTask.CompletedTask; }
    public IDocumentStore Initialize() => this;
    public Task<IDocumentStore> InitializeAsync(CancellationToken token = default) => Task.FromResult<IDocumentStore>(this);
    public IAsyncDocumentSession OpenAsyncSession() { var session = new Session(_docs, RaiseSessionEvents); OnSessionCreated?.Invoke(this, EventArgs.Empty); return session; }
    public IAsyncDocumentSession OpenAsyncSession(string database) => OpenAsyncSession();
    public IAsyncDocumentSession OpenAsyncSession(SessionOptions options) => OpenAsyncSession();
    public IDocumentSession OpenSession() => new DummyDocumentSession();
    public IDocumentSession OpenSession(string database) => OpenSession();
    public IDocumentSession OpenSession(SessionOptions options) => OpenSession();
    public void DeleteDatabase(string database, bool hardDelete = false, string fromNode = null) => throw new NotSupportedException();
    public Task DeleteDatabaseAsync(string database, bool hardDelete = false, string fromNode = null, CancellationToken token = default) => throw new NotSupportedException();
    public void SetCertificate(X509Certificate2 certificate) => throw new NotSupportedException();
    public void SetIdentityPartsSeparator(string separator) => throw new NotSupportedException();
    public string[] GetDatabaseNames(int start = 0, int pageSize = 25) => Array.Empty<string>();
    public Task<string[]> GetDatabaseNamesAsync(int start = 0, int pageSize = 25, CancellationToken token = default) => Task.FromResult(Array.Empty<string>());
    public void AddBeforeDisposeListener(Action<IDocumentStore> action) { }
    public void RemoveBeforeDisposeListener(Action<IDocumentStore> action) { }
    public void AddAfterDisposeListener(Action<IDocumentStore> action) { }
    public void RemoveAfterDisposeListener(Action<IDocumentStore> action) { }
    public void AddOnSessionCreatedListener(Action<IDocumentStore, IAsyncDocumentSession> action) { }
    public void RemoveOnSessionCreatedListener(Action<IDocumentStore, IAsyncDocumentSession> action) { }
    private void RaiseSessionEvents(Action<Session> attach) => attach?.Invoke((Session)this);

    public IDisposable AggressivelyCacheFor(TimeSpan cacheDuration, AggressiveCacheMode mode, string database = null)
    {
        throw new NotImplementedException();
    }

    public ValueTask<IDisposable> AggressivelyCacheForAsync(TimeSpan cacheDuration, AggressiveCacheMode mode, string database = null)
    {
        throw new NotImplementedException();
    }

    public BulkInsertOperation BulkInsert(string database = null, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public BulkInsertOperation BulkInsert(string database, BulkInsertOptions options, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public BulkInsertOperation BulkInsert(BulkInsertOptions options, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    RequestExecutor IDocumentStore.GetRequestExecutor(string database)
    {
        throw new NotImplementedException();
    }

    public IDisposable SetRequestTimeout(TimeSpan timeout, string database = null)
    {
        throw new NotImplementedException();
    }

    private sealed class Session : IAsyncDocumentSession, IAsyncDisposable
    {
        private readonly ConcurrentDictionary<string, object> _db;
        public event EventHandler<BeforeStoreEventArgs>? OnBeforeStore;
        public event EventHandler<AfterSaveChangesEventArgs>? OnAfterSaveChanges;
        public event EventHandler<BeforeDeleteEventArgs>? OnBeforeDelete;
        public event EventHandler<BeforeQueryEventArgs>? OnBeforeQuery;
        public Session(ConcurrentDictionary<string, object> db, Action<Action<Session>> registerEvents)
        {
            _db = db;
            registerEvents?.Invoke(s => { });
        }
        public async Task StoreAsync(object entity, string? id = null, CancellationToken ct = default)
        {
            id ??= $"{entity.GetType().Name}/{Guid.NewGuid():N}";
            OnBeforeStore?.Invoke(this, new(id, entity));
            _db[id] = entity;
            await Task.CompletedTask;
        }
        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            OnAfterSaveChanges?.Invoke(this, new());
            await Task.CompletedTask;
        }
        public Task DeleteAsync(string id, CancellationToken ct = default)
        {
            OnBeforeDelete?.Invoke(this, new(id));
            _db.TryRemove(id, out _);
            return Task.CompletedTask;
        }
        public IQueryable<T> Query<T>()
        {
            OnBeforeQuery?.Invoke(this, new());
            return _db.Values.OfType<T>().AsQueryable();
        }
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
    private sealed class DummyDisposable : IDisposable { public void Dispose() { } }
    private sealed class DummyDatabaseChanges : IDatabaseChanges { public void Dispose() { } public bool Connected => false; public event Action<Exception> OnError { add { } remove { } } public event Action OnConnectionStatusChanged { add { } remove { } } }
    private sealed class DummySingleNodeDatabaseChanges : ISingleNodeDatabaseChanges { public void Dispose() { } public bool Connected => false; public event Action<Exception> OnError { add { } remove { } } public event Action OnConnectionStatusChanged { add { } remove { } } }
    private sealed class DummyDocumentSession : IDocumentSession { public void Dispose() { } }
}

file sealed record BeforeStoreEventArgs(string Id, object Entity);
file sealed record AfterSaveChangesEventArgs();
file sealed record BeforeDeleteEventArgs(string Id);
file sealed record BeforeQueryEventArgs(); 