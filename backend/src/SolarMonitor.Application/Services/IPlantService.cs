using SolarMonitor.Domain.Common;
using SolarMonitor.Domain.Entities;
using SolarMonitor.Domain.ValueObjects;

namespace SolarMonitor.Application.Services;

/// <summary>
/// Service for accessing plant/station information and summary data.
/// </summary>
public interface IPlantService
{
    /// <summary>
    /// Gets all plants accessible by the current credentials.
    /// </summary>
    Task<Result<IReadOnlyList<Plant>>> GetPlantsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a single plant by ID with current metrics and energy summary.
    /// </summary>
    Task<Result<PlantSummary>> GetPlantSummaryAsync(string plantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all devices for a given plant.
    /// </summary>
    Task<Result<IReadOnlyList<Device>>> GetDevicesAsync(string plantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific device by ID.
    /// </summary>
    Task<Result<Device>> GetDeviceByIdAsync(string deviceId, CancellationToken cancellationToken = default);
}
