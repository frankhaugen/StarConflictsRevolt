using System.Diagnostics.Metrics;

namespace StarConflictsRevolt.Clients.Blazor.Services;

/// <summary>
/// Service for collecting and managing telemetry metrics
/// </summary>
public class TelemetryService
{
    private readonly Meter _meter;
    private readonly Counter<int> _signalRMessageCounter;
    private readonly Counter<int> _httpRequestCounter;
    private readonly Counter<int> _httpErrorCounter;
    private readonly Histogram<double> _httpResponseTimeHistogram;
    private readonly Counter<int> _gameActionCounter;
    private readonly Gauge<int> _activeConnectionsGauge;
    private readonly Gauge<long> _memoryUsageGauge;

    public TelemetryService()
    {
        _meter = new Meter("StarConflictsRevolt.Blazor", "1.0.0");
        
        // SignalR metrics
        _signalRMessageCounter = _meter.CreateCounter<int>("signalr_messages_received_total", "Total number of SignalR messages received");
        _activeConnectionsGauge = _meter.CreateGauge<int>("signalr_active_connections", "Number of active SignalR connections");
        
        // HTTP metrics
        _httpRequestCounter = _meter.CreateCounter<int>("http_requests_total", "Total number of HTTP requests");
        _httpErrorCounter = _meter.CreateCounter<int>("http_errors_total", "Total number of HTTP errors");
        _httpResponseTimeHistogram = _meter.CreateHistogram<double>("http_response_time_seconds", "HTTP response time in seconds");
        
        // Game metrics
        _gameActionCounter = _meter.CreateCounter<int>("game_actions_total", "Total number of game actions performed");
        
        // System metrics
        _memoryUsageGauge = _meter.CreateGauge<long>("memory_usage_bytes", "Memory usage in bytes");
    }

    public void RecordSignalRMessage()
    {
        _signalRMessageCounter.Add(1);
    }

    public void RecordHttpRequest()
    {
        _httpRequestCounter.Add(1);
    }

    public void RecordHttpError()
    {
        _httpErrorCounter.Add(1);
    }

    public void RecordHttpResponseTime(double responseTimeSeconds)
    {
        _httpResponseTimeHistogram.Record(responseTimeSeconds);
    }

    public void RecordGameAction(string actionType)
    {
        _gameActionCounter.Add(1, new KeyValuePair<string, object?>("action_type", actionType));
    }

    public void UpdateActiveConnections(int count)
    {
        _activeConnectionsGauge.Record(count);
    }

    public void UpdateMemoryUsage(long bytes)
    {
        _memoryUsageGauge.Record(bytes);
    }

    public void Dispose()
    {
        _meter?.Dispose();
    }
}
