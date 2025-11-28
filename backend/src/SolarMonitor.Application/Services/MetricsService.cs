using SolarMonitor.Application.Ports;
using SolarMonitor.Domain.Common;
using SolarMonitor.Domain.ValueObjects;

namespace SolarMonitor.Application.Services;

public class MetricsService : IMetricsService
{
    private readonly IMetricsCache _cache;

    public MetricsService(IMetricsCache cache)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public async Task<Result<RealtimeMetrics>> GetPlantRealtimeMetricsAsync(
        string plantId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(plantId))
        {
            return Result<RealtimeMetrics>.Failure("Plant ID is required.");
        }

        var metrics = await _cache.GetPlantRealtimeMetricsAsync(plantId, cancellationToken);

        if (metrics == null)
        {
            return Result<RealtimeMetrics>.Failure($"No real-time metrics available for plant '{plantId}'.");
        }

        return Result<RealtimeMetrics>.Success(metrics);
    }

    public async Task<Result<RealtimeMetrics>> GetDeviceRealtimeMetricsAsync(
        string deviceId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            return Result<RealtimeMetrics>.Failure("Device ID is required.");
        }

        // In a full implementation, we would cache device-level metrics separately
        // For now, return a failure indicating this needs implementation
        return Result<RealtimeMetrics>.Failure("Device-level real-time metrics not yet implemented.");
    }

    public async Task<Result<IReadOnlyList<TimeseriesPoint>>> GetPlantHistoricalDataAsync(
        string plantId,
        MetricType metricType,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(plantId))
        {
            return Result<IReadOnlyList<TimeseriesPoint>>.Failure("Plant ID is required.");
        }

        if (from >= to)
        {
            return Result<IReadOnlyList<TimeseriesPoint>>.Failure("'From' date must be before 'To' date.");
        }

        // Limit time range to avoid excessive data
        var maxDays = 90;
        if ((to - from).TotalDays > maxDays)
        {
            return Result<IReadOnlyList<TimeseriesPoint>>.Failure($"Time range cannot exceed {maxDays} days.");
        }

        var points = await _cache.GetTimeseriesPointsAsync(plantId, metricType, from, to, cancellationToken);

        return Result<IReadOnlyList<TimeseriesPoint>>.Success(points);
    }
}
