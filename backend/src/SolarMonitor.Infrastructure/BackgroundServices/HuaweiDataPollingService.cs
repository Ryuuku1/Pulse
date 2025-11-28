using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SolarMonitor.Application.Ports;
using SolarMonitor.Domain.ValueObjects;
using SolarMonitor.Infrastructure.Configuration;

namespace SolarMonitor.Infrastructure.BackgroundServices;

/// <summary>
/// Background service that periodically polls the Huawei FusionSolar API
/// and updates the metrics cache.
/// </summary>
public class HuaweiDataPollingService : BackgroundService
{
    private readonly IHuaweiClient _huaweiClient;
    private readonly IMetricsCache _cache;
    private readonly ILogger<HuaweiDataPollingService> _logger;
    private readonly HuaweiApiOptions _options;

    public HuaweiDataPollingService(
        IHuaweiClient huaweiClient,
        IMetricsCache cache,
        IOptions<HuaweiApiOptions> options,
        ILogger<HuaweiDataPollingService> logger)
    {
        _huaweiClient = huaweiClient ?? throw new ArgumentNullException(nameof(huaweiClient));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Huawei Data Polling Service starting...");

        // Initial authentication
        var loginResult = await _huaweiClient.LoginAsync(stoppingToken);
        if (loginResult.IsFailure)
        {
            _logger.LogError("Initial authentication failed: {Error}", loginResult.Error);
            _logger.LogWarning("Polling service will retry on next interval");
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PollDataAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during data polling");
            }

            var delay = TimeSpan.FromSeconds(_options.PollingIntervalSeconds);
            _logger.LogDebug("Next poll in {Delay} seconds", delay.TotalSeconds);

            await Task.Delay(delay, stoppingToken);
        }

        _logger.LogInformation("Huawei Data Polling Service stopping...");
    }

    private async Task PollDataAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Polling Huawei FusionSolar API...");

        // Fetch and cache plants
        var plantsResult = await _huaweiClient.GetPlantsAsync(cancellationToken);
        if (plantsResult.IsFailure)
        {
            _logger.LogWarning("Failed to fetch plants: {Error}", plantsResult.Error);
            return;
        }

        var plants = plantsResult.Value!;
        await _cache.SetPlantsAsync(plants, cancellationToken);
        _logger.LogInformation("Cached {Count} plants", plants.Count);

        // For each plant, fetch devices and metrics
        foreach (var plant in plants)
        {
            try
            {
                await PollPlantDataAsync(plant.Id, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error polling data for plant {PlantId}", plant.Id);
            }
        }

        _logger.LogInformation("Polling complete");
    }

    private async Task PollPlantDataAsync(string plantId, CancellationToken cancellationToken)
    {
        // Fetch devices
        var devicesResult = await _huaweiClient.GetDevicesAsync(plantId, cancellationToken);
        if (devicesResult.IsSuccess)
        {
            await _cache.SetDevicesAsync(plantId, devicesResult.Value!, cancellationToken);
            _logger.LogDebug("Cached {Count} devices for plant {PlantId}", devicesResult.Value!.Count, plantId);
        }
        else
        {
            _logger.LogWarning("Failed to fetch devices for plant {PlantId}: {Error}", plantId, devicesResult.Error);
        }

        // Fetch real-time metrics
        var realtimeResult = await _huaweiClient.GetPlantRealtimeMetricsAsync(plantId, cancellationToken);
        if (realtimeResult.IsSuccess)
        {
            await _cache.SetPlantRealtimeMetricsAsync(plantId, realtimeResult.Value!, cancellationToken);
            _logger.LogDebug("Cached real-time metrics for plant {PlantId}", plantId);

            // Store as timeseries point for historical tracking
            var point = new TimeseriesPoint
            {
                TimestampUtc = realtimeResult.Value!.TimestampUtc,
                MetricType = MetricType.Power,
                Value = realtimeResult.Value.PvPowerKw ?? 0,
                Unit = "kW"
            };

            await _cache.AddTimeseriesPointsAsync(plantId, new[] { point }, cancellationToken);
        }
        else
        {
            _logger.LogWarning("Failed to fetch real-time metrics for plant {PlantId}: {Error}", plantId, realtimeResult.Error);
        }

        // Fetch energy summary
        var energyResult = await _huaweiClient.GetPlantEnergySummaryAsync(plantId, cancellationToken);
        if (energyResult.IsSuccess)
        {
            await _cache.SetPlantEnergySummaryAsync(plantId, energyResult.Value!, cancellationToken);
            _logger.LogDebug("Cached energy summary for plant {PlantId}", plantId);
        }
        else
        {
            _logger.LogWarning("Failed to fetch energy summary for plant {PlantId}: {Error}", plantId, energyResult.Error);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Logging out from Huawei API...");
        await _huaweiClient.LogoutAsync(cancellationToken);
        await base.StopAsync(cancellationToken);
    }
}
