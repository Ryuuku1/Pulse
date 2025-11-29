using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SolarMonitor.Application.Ports;
using SolarMonitor.Domain.Entities;
using SolarMonitor.Domain.ValueObjects;
using SolarMonitor.Infrastructure.Configuration;

namespace SolarMonitor.Infrastructure.BackgroundServices;

/// <summary>
/// Background service that polls inverter data either via Huawei FusionSolar API or Modbus TCP.
/// </summary>
public class PlantDataPollingService : BackgroundService
{
    private readonly IHuaweiClient _huaweiClient;
    private readonly IModbusClient _modbusClient;
    private readonly IMetricsCache _cache;
    private readonly ILogger<PlantDataPollingService> _logger;
    private readonly HuaweiApiOptions _huaweiOptions;
    private readonly ModbusOptions _modbusOptions;
    private readonly DataAcquisitionOptions _dataOptions;

    public PlantDataPollingService(
        IHuaweiClient huaweiClient,
        IModbusClient modbusClient,
        IMetricsCache cache,
        IOptions<HuaweiApiOptions> huaweiOptions,
        IOptions<ModbusOptions> modbusOptions,
        IOptions<DataAcquisitionOptions> dataOptions,
        ILogger<PlantDataPollingService> logger)
    {
        _huaweiClient = huaweiClient ?? throw new ArgumentNullException(nameof(huaweiClient));
        _modbusClient = modbusClient ?? throw new ArgumentNullException(nameof(modbusClient));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _huaweiOptions = huaweiOptions?.Value ?? throw new ArgumentNullException(nameof(huaweiOptions));
        _modbusOptions = modbusOptions?.Value ?? throw new ArgumentNullException(nameof(modbusOptions));
        _dataOptions = dataOptions?.Value ?? throw new ArgumentNullException(nameof(dataOptions));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Plant data polling service starting in {Mode} mode", _dataOptions.Mode);

        if (_dataOptions.Mode == DataSourceMode.HuaweiApi)
        {
            var loginResult = await _huaweiClient.LoginAsync(stoppingToken);
            if (loginResult.IsFailure)
            {
                _logger.LogError("Initial authentication failed: {Error}", loginResult.Error);
                _logger.LogWarning("Polling service will retry on next interval");
            }
        }
        else
        {
            await EnsureModbusMetadataAsync(stoppingToken);
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_dataOptions.Mode == DataSourceMode.ModbusTcp)
                {
                    await PollModbusAsync(stoppingToken);
                }
                else
                {
                    await PollHuaweiAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during data polling");
            }

            var delay = TimeSpan.FromSeconds(GetPollingIntervalSeconds());
            _logger.LogDebug("Next poll in {Delay} seconds", delay.TotalSeconds);

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // shutting down
            }
        }

        _logger.LogInformation("Plant data polling service stopping...");
    }

    private async Task PollHuaweiAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Polling Huawei FusionSolar API...");

        var plantsResult = await _huaweiClient.GetPlantsAsync(cancellationToken);
        if (plantsResult.IsFailure)
        {
            _logger.LogWarning("Failed to fetch plants: {Error}", plantsResult.Error);
            return;
        }

        var plants = plantsResult.Value!;
        await _cache.SetPlantsAsync(plants, cancellationToken);
        _logger.LogInformation("Cached {Count} plants", plants.Count);

        foreach (var plant in plants)
        {
            try
            {
                await PollHuaweiPlantDataAsync(plant.Id, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error polling data for plant {PlantId}", plant.Id);
            }
        }

        _logger.LogInformation("Huawei polling complete");
    }

    private async Task PollHuaweiPlantDataAsync(string plantId, CancellationToken cancellationToken)
    {
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

        var realtimeResult = await _huaweiClient.GetPlantRealtimeMetricsAsync(plantId, cancellationToken);
        if (realtimeResult.IsSuccess)
        {
            await _cache.SetPlantRealtimeMetricsAsync(plantId, realtimeResult.Value!, cancellationToken);
            _logger.LogDebug("Cached real-time metrics for plant {PlantId}", plantId);

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

    private async Task PollModbusAsync(CancellationToken cancellationToken)
    {
        var plantId = _modbusOptions.Plant.PlantId;

        await EnsureModbusMetadataAsync(cancellationToken);

        var realtimeResult = await _modbusClient.ReadRealtimeMetricsAsync(cancellationToken);
        if (realtimeResult.IsSuccess)
        {
            await _cache.SetPlantRealtimeMetricsAsync(plantId, realtimeResult.Value!, cancellationToken);

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
            _logger.LogWarning("Modbus realtime poll failed: {Error}", realtimeResult.Error);
        }

        var energyResult = await _modbusClient.ReadEnergySummaryAsync(cancellationToken);
        if (energyResult.IsSuccess)
        {
            await _cache.SetPlantEnergySummaryAsync(plantId, energyResult.Value!, cancellationToken);
        }
        else
        {
            _logger.LogWarning("Modbus energy poll failed: {Error}", energyResult.Error);
        }
    }

    private async Task EnsureModbusMetadataAsync(CancellationToken cancellationToken)
    {
        var plantOptions = _modbusOptions.Plant;

        var plant = new Plant
        {
            Id = plantOptions.PlantId,
            Name = plantOptions.PlantName,
            Address = "Local network",
            InstalledCapacityKw = plantOptions.InstalledCapacityKw,
            Latitude = null,
            Longitude = null,
            InstallationDate = null,
            Status = PlantStatus.Connected,
            LastUpdateTime = DateTime.UtcNow
        };

        var device = new Device
        {
            Id = plantOptions.DeviceId,
            PlantId = plant.Id,
            Name = plantOptions.DeviceName,
            Type = DeviceType.ResidentialInverter,
            Model = plantOptions.DeviceModel,
            SerialNumber = null,
            FirmwareVersion = null,
            Status = DeviceStatus.Normal,
            LastCommunicationTime = DateTime.UtcNow
        };

        await _cache.SetPlantsAsync(new[] { plant }, cancellationToken);
        await _cache.SetDevicesAsync(plant.Id, new[] { device }, cancellationToken);
    }

    private int GetPollingIntervalSeconds()
    {
        if (_dataOptions.PollingIntervalSeconds > 0)
        {
            return _dataOptions.PollingIntervalSeconds;
        }

        return _dataOptions.Mode == DataSourceMode.ModbusTcp
            ? _modbusOptions.PollingIntervalSeconds
            : _huaweiOptions.PollingIntervalSeconds;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_dataOptions.Mode == DataSourceMode.HuaweiApi)
        {
            _logger.LogInformation("Logging out from Huawei API...");
            await _huaweiClient.LogoutAsync(cancellationToken);
        }

        await base.StopAsync(cancellationToken);
    }
}
