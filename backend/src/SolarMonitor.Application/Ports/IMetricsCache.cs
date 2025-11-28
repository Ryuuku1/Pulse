using SolarMonitor.Domain.Entities;
using SolarMonitor.Domain.ValueObjects;

namespace SolarMonitor.Application.Ports;

/// <summary>
/// Port for caching/storing metrics data retrieved from Huawei API.
/// This allows the application to serve data without constantly hitting the external API.
/// </summary>
public interface IMetricsCache
{
    /// <summary>
    /// Stores or updates realtime metrics for a plant.
    /// </summary>
    Task SetPlantRealtimeMetricsAsync(string plantId, RealtimeMetrics metrics, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves cached realtime metrics for a plant.
    /// </summary>
    Task<RealtimeMetrics?> GetPlantRealtimeMetricsAsync(string plantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores or updates energy summary for a plant.
    /// </summary>
    Task SetPlantEnergySummaryAsync(string plantId, EnergySummary summary, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves cached energy summary for a plant.
    /// </summary>
    Task<EnergySummary?> GetPlantEnergySummaryAsync(string plantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores or updates the plant list.
    /// </summary>
    Task SetPlantsAsync(IReadOnlyList<Plant> plants, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves cached plant list.
    /// </summary>
    Task<IReadOnlyList<Plant>?> GetPlantsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores or updates device list for a plant.
    /// </summary>
    Task SetDevicesAsync(string plantId, IReadOnlyList<Device> devices, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves cached device list for a plant.
    /// </summary>
    Task<IReadOnlyList<Device>?> GetDevicesAsync(string plantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores a batch of historical timeseries points.
    /// </summary>
    Task AddTimeseriesPointsAsync(string plantId, IReadOnlyList<TimeseriesPoint> points, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves historical timeseries points for a given metric and time range.
    /// </summary>
    Task<IReadOnlyList<TimeseriesPoint>> GetTimeseriesPointsAsync(
        string plantId,
        MetricType metricType,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default);
}
