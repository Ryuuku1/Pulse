using System.Collections.Concurrent;
using SolarMonitor.Application.Ports;
using SolarMonitor.Domain.Entities;
using SolarMonitor.Domain.ValueObjects;

namespace SolarMonitor.Infrastructure.Caching;

/// <summary>
/// In-memory implementation of metrics cache.
/// For production with persistence requirements, this could be replaced with a database-backed implementation.
/// </summary>
public class InMemoryMetricsCache : IMetricsCache
{
    private readonly ConcurrentDictionary<string, RealtimeMetrics> _plantRealtimeMetrics = new();
    private readonly ConcurrentDictionary<string, EnergySummary> _plantEnergySummaries = new();
    private readonly ConcurrentDictionary<string, List<Device>> _plantDevices = new();
    private readonly ConcurrentDictionary<string, List<TimeseriesPoint>> _timeseriesPoints = new();
    private IReadOnlyList<Plant>? _plants;

    public Task SetPlantRealtimeMetricsAsync(string plantId, RealtimeMetrics metrics, CancellationToken cancellationToken = default)
    {
        _plantRealtimeMetrics[plantId] = metrics;
        return Task.CompletedTask;
    }

    public Task<RealtimeMetrics?> GetPlantRealtimeMetricsAsync(string plantId, CancellationToken cancellationToken = default)
    {
        _plantRealtimeMetrics.TryGetValue(plantId, out var metrics);
        return Task.FromResult(metrics);
    }

    public Task SetPlantEnergySummaryAsync(string plantId, EnergySummary summary, CancellationToken cancellationToken = default)
    {
        _plantEnergySummaries[plantId] = summary;
        return Task.CompletedTask;
    }

    public Task<EnergySummary?> GetPlantEnergySummaryAsync(string plantId, CancellationToken cancellationToken = default)
    {
        _plantEnergySummaries.TryGetValue(plantId, out var summary);
        return Task.FromResult(summary);
    }

    public Task SetPlantsAsync(IReadOnlyList<Plant> plants, CancellationToken cancellationToken = default)
    {
        _plants = plants;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Plant>?> GetPlantsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_plants);
    }

    public Task SetDevicesAsync(string plantId, IReadOnlyList<Device> devices, CancellationToken cancellationToken = default)
    {
        _plantDevices[plantId] = devices.ToList();
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Device>?> GetDevicesAsync(string plantId, CancellationToken cancellationToken = default)
    {
        if (_plantDevices.TryGetValue(plantId, out var devices))
        {
            return Task.FromResult<IReadOnlyList<Device>?>(devices);
        }
        return Task.FromResult<IReadOnlyList<Device>?>(null);
    }

    public Task AddTimeseriesPointsAsync(string plantId, IReadOnlyList<TimeseriesPoint> points, CancellationToken cancellationToken = default)
    {
        var key = plantId;
        if (!_timeseriesPoints.ContainsKey(key))
        {
            _timeseriesPoints[key] = new List<TimeseriesPoint>();
        }

        _timeseriesPoints[key].AddRange(points);

        // Keep only last 30 days of data to prevent unbounded growth
        var cutoffDate = DateTime.UtcNow.AddDays(-30);
        _timeseriesPoints[key] = _timeseriesPoints[key]
            .Where(p => p.TimestampUtc >= cutoffDate)
            .OrderBy(p => p.TimestampUtc)
            .ToList();

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<TimeseriesPoint>> GetTimeseriesPointsAsync(
        string plantId,
        MetricType metricType,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        var key = plantId;

        if (!_timeseriesPoints.TryGetValue(key, out var allPoints))
        {
            return Task.FromResult<IReadOnlyList<TimeseriesPoint>>(Array.Empty<TimeseriesPoint>());
        }

        var filtered = allPoints
            .Where(p => p.MetricType == metricType && p.TimestampUtc >= from && p.TimestampUtc <= to)
            .OrderBy(p => p.TimestampUtc)
            .ToList();

        return Task.FromResult<IReadOnlyList<TimeseriesPoint>>(filtered);
    }
}
