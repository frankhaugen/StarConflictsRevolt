using System.Text.Json;

namespace StarConflictsRevolt.Clients.Raylib;

public class FileLoggerProvider : ILoggerProvider
{
    private readonly string _logFilePath;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly object _lock = new();

    public FileLoggerProvider()
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        _logFilePath = Path.Combine(AppContext.BaseDirectory, $"{timestamp}.log");
        
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        // Create the log file and write header
        File.WriteAllText(_logFilePath, $"# StarConflictsRevolt Raylib Client Log - Started at {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}\n");
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new FileLogger(categoryName, _logFilePath, _jsonOptions, _lock);
    }

    public void Dispose()
    {
        // Flush any remaining logs
        GC.SuppressFinalize(this);
    }

    private class FileLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly string _logFilePath;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly object _lock;

        public FileLogger(string categoryName, string logFilePath, JsonSerializerOptions jsonOptions, object lockObj)
        {
            _categoryName = categoryName;
            _logFilePath = logFilePath;
            _jsonOptions = jsonOptions;
            _lock = lockObj;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            var logEntry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Level = logLevel.ToString(),
                Category = _categoryName,
                EventId = eventId.Id,
                Message = formatter(state, exception),
                Exception = exception?.ToString(),
                State = state?.ToString()
            };

            var jsonLine = JsonSerializer.Serialize(logEntry, _jsonOptions);

            lock (_lock)
            {
                try
                {
                    File.AppendAllText(_logFilePath, jsonLine + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    // Fallback to console if file writing fails
                    Console.WriteLine($"Failed to write to log file: {ex.Message}");
                    Console.WriteLine($"Original log: {jsonLine}");
                }
            }
        }

        private class LogEntry
        {
            public DateTime Timestamp { get; set; }
            public string Level { get; set; } = string.Empty;
            public string Category { get; set; } = string.Empty;
            public int EventId { get; set; }
            public string Message { get; set; } = string.Empty;
            public string? Exception { get; set; }
            public string? State { get; set; }
        }
    }
} 