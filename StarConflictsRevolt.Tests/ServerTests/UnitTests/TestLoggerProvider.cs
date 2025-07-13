using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace StarConflictsRevolt.Tests.ServerTests.UnitTests;

public class TestLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentBag<string> _sink;

    public TestLoggerProvider(ConcurrentBag<string> sink)
    {
        _sink = sink;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new TestLogger(_sink, categoryName);
    }

    public void Dispose()
    {
    }

    private class TestLogger : ILogger
    {
        private readonly string _category;
        private readonly ConcurrentBag<string> _sink;

        public TestLogger(ConcurrentBag<string> sink, string category)
        {
            _sink = sink;
            _category = category;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            _sink.Add($"{logLevel}: {_category}: {formatter(state, exception)}{(exception != null ? " " + exception : "")}");
        }
    }
}