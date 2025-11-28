using SolarMonitor.Domain.Common;
using SolarMonitor.Domain.Entities;
using SolarMonitor.Domain.ValueObjects;

namespace SolarMonitor.Application.Ports;

/// <summary>
/// Port (interface) for interacting with the Huawei FusionSolar API.
/// This abstraction isolates the application layer from HTTP/infrastructure concerns.
/// </summary>
public interface IHuaweiClient
{
    /// <summary>
    /// Authenticates with the Huawei API and obtains an access token.
    /// </summary>
    Task<Result> LoginAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs out and invalidates the current session.
    /// </summary>
    Task<Result> LogoutAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the list of plants/stations accessible by the current credentials.
    /// </summary>
    Task<Result<IReadOnlyList<Plant>>> GetPlantsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves detailed information about a specific plant.
    /// </summary>
    Task<Result<Plant>> GetPlantByIdAsync(string plantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the list of devices for a given plant.
    /// </summary>
    Task<Result<IReadOnlyList<Device>>> GetDevicesAsync(string plantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves real-time KPIs (power, energy, etc.) for a plant.
    /// </summary>
    Task<Result<RealtimeMetrics>> GetPlantRealtimeMetricsAsync(string plantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves real-time KPIs for a specific device.
    /// </summary>
    Task<Result<RealtimeMetrics>> GetDeviceRealtimeMetricsAsync(string deviceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves energy summary (today, month, year, total) for a plant.
    /// </summary>
    Task<Result<EnergySummary>> GetPlantEnergySummaryAsync(string plantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves historical time-series data for a plant.
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
