using SolarMonitor.Application.Ports;
using SolarMonitor.Domain.Common;
using SolarMonitor.Domain.Entities;
using SolarMonitor.Domain.ValueObjects;

namespace SolarMonitor.Application.Services;

public class PlantService : IPlantService
{
    private readonly IMetricsCache _cache;

    public PlantService(IMetricsCache cache)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public async Task<Result<IReadOnlyList<Plant>>> GetPlantsAsync(CancellationToken cancellationToken = default)
    {
        var plants = await _cache.GetPlantsAsync(cancellationToken);

        if (plants == null || plants.Count == 0)
        {
            return Result<IReadOnlyList<Plant>>.Failure("No plants available. Data may not have been synced yet.");
        }

        return Result<IReadOnlyList<Plant>>.Success(plants);
    }

    public async Task<Result<PlantSummary>> GetPlantSummaryAsync(string plantId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(plantId))
        {
            return Result<PlantSummary>.Failure("Plant ID is required.");
        }

        var plants = await _cache.GetPlantsAsync(cancellationToken);
        var plant = plants?.FirstOrDefault(p => p.Id == plantId);

        if (plant == null)
        {
            return Result<PlantSummary>.Failure($"Plant with ID '{plantId}' not found.");
        }

        var realtimeMetrics = await _cache.GetPlantRealtimeMetricsAsync(plantId, cancellationToken);
        var energySummary = await _cache.GetPlantEnergySummaryAsync(plantId, cancellationToken);
        var devices = await _cache.GetDevicesAsync(plantId, cancellationToken);

        var summary = new PlantSummary
        {
            Plant = plant,
            CurrentMetrics = realtimeMetrics,
            EnergySummary = energySummary,
            TotalDevices = devices?.Count ?? 0,
            ActiveDevices = devices?.Count(d => d.Status == DeviceStatus.Normal) ?? 0
        };

        return Result<PlantSummary>.Success(summary);
    }

    public async Task<Result<IReadOnlyList<Device>>> GetDevicesAsync(string plantId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(plantId))
        {
            return Result<IReadOnlyList<Device>>.Failure("Plant ID is required.");
        }

        var devices = await _cache.GetDevicesAsync(plantId, cancellationToken);

        if (devices == null || devices.Count == 0)
        {
            return Result<IReadOnlyList<Device>>.Failure($"No devices found for plant '{plantId}'.");
        }

        return Result<IReadOnlyList<Device>>.Success(devices);
    }

    public async Task<Result<Device>> GetDeviceByIdAsync(string deviceId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            return Result<Device>.Failure("Device ID is required.");
        }

        // Since we store devices by plantId, we need to search across all plants
        // In a real implementation, you might want to add a GetDeviceById method to the cache
        var plants = await _cache.GetPlantsAsync(cancellationToken);

        if (plants == null)
        {
            return Result<Device>.Failure("No data available.");
        }

        foreach (var plant in plants)
        {
            var devices = await _cache.GetDevicesAsync(plant.Id, cancellationToken);
            var device = devices?.FirstOrDefault(d => d.Id == deviceId);

            if (device != null)
            {
                return Result<Device>.Success(device);
            }
        }

        return Result<Device>.Failure($"Device with ID '{deviceId}' not found.");
    }
}
