using SolarMonitor.Domain.Common;
using SolarMonitor.Domain.ValueObjects;

namespace SolarMonitor.Application.Services;

/// <summary>
/// Service for accessing real-time and historical metrics data.
/// </summary>
public interface IMetricsService
{
    /// <summary>
    /// Gets the latest real-time metrics for a plant.
    /// </summary>
    Task<Result<RealtimeMetrics>> GetPlantRealtimeMetricsAsync(string plantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the latest real-time metrics for a device.
    /// </summary>
    Task<Result<RealtimeMetrics>> GetDeviceRealtimeMetricsAsync(string deviceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets historical time-series data for a plant.
    /// </summary>
    /// <param name="plantId">The plant ID.</param>
    /// <param name="metricType">The type of metric to retrieve.</param>
    /// <param name="from">Start of the time range (UTC).</param>
    /// <param name="to">End of the time range (UTC).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<Result<IReadOnlyList<TimeseriesPoint>>> GetPlantHistoricalDataAsync(
        string plantId,
        MetricType metricType,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default);
}
