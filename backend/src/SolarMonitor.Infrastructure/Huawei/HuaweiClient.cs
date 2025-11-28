using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SolarMonitor.Application.Ports;
using SolarMonitor.Domain.Common;
using SolarMonitor.Domain.Entities;
using SolarMonitor.Domain.ValueObjects;
using SolarMonitor.Infrastructure.Configuration;
using SolarMonitor.Infrastructure.Huawei.DTOs;

namespace SolarMonitor.Infrastructure.Huawei;

/// <summary>
/// Implementation of the Huawei FusionSolar API client.
/// Handles authentication, API calls, retries, and mapping to domain models.
/// </summary>
public class HuaweiClient : IHuaweiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HuaweiClient> _logger;
    private readonly HuaweiApiOptions _options;
    private string? _authToken;
    private DateTime _tokenExpiry = DateTime.MinValue;
    private readonly SemaphoreSlim _authLock = new(1, 1);

    public HuaweiClient(
        HttpClient httpClient,
        IOptions<HuaweiApiOptions> options,
        ILogger<HuaweiClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_options.RequestTimeoutSeconds);
    }

    public async Task<Result> LoginAsync(CancellationToken cancellationToken = default)
    {
        await _authLock.WaitAsync(cancellationToken);
        try
        {
            _logger.LogInformation("Authenticating with Huawei FusionSolar API...");

            var payload = new
            {
                userName = _options.Username,
                systemCode = _options.Password
            };

            var response = await _httpClient.PostAsJsonAsync("/thirdData/login", payload, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = $"Authentication failed with status code: {response.StatusCode}";
                _logger.LogError(error);
                return Result.Failure(error);
            }

            var result = await response.Content.ReadFromJsonAsync<HuaweiApiResponse<LoginData>>(cancellationToken);

            if (result == null || !result.Success || string.IsNullOrWhiteSpace(result.Data?.Token))
            {
                var error = $"Authentication failed: {result?.Message ?? "Unknown error"}";
                _logger.LogError(error);
                return Result.Failure(error);
            }

            _authToken = result.Data.Token;
            _tokenExpiry = DateTime.UtcNow.AddHours(1); // Assume 1-hour token validity
            _logger.LogInformation("Authentication successful.");

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during authentication");
            return Result.Failure($"Authentication exception: {ex.Message}");
        }
        finally
        {
            _authLock.Release();
        }
    }

    public async Task<Result> LogoutAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_authToken))
        {
            return Result.Success(); // Already logged out
        }

        try
        {
            var response = await PostAuthenticatedAsync("/thirdData/logout", null, cancellationToken);
            _authToken = null;
            _tokenExpiry = DateTime.MinValue;
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Logout failed, clearing token anyway");
            _authToken = null;
            _tokenExpiry = DateTime.MinValue;
            return Result.Success();
        }
    }

    public async Task<Result<IReadOnlyList<Plant>>> GetPlantsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await PostAuthenticatedAsync<StationListData>("/thirdData/getStationList", null, cancellationToken);

            if (response == null || response.List == null)
            {
                return Result<IReadOnlyList<Plant>>.Failure("No station data returned.");
            }

            var plants = response.List.Select(MapToPlant).ToList();
            return Result<IReadOnlyList<Plant>>.Success(plants);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving plants");
            return Result<IReadOnlyList<Plant>>.Failure($"Error retrieving plants: {ex.Message}");
        }
    }

    public async Task<Result<Plant>> GetPlantByIdAsync(string plantId, CancellationToken cancellationToken = default)
    {
        var plantsResult = await GetPlantsAsync(cancellationToken);
        if (plantsResult.IsFailure)
        {
            return Result<Plant>.Failure(plantsResult.Error!);
        }

        var plant = plantsResult.Value?.FirstOrDefault(p => p.Id == plantId);
        if (plant == null)
        {
            return Result<Plant>.Failure($"Plant with ID '{plantId}' not found.");
        }

        return Result<Plant>.Success(plant);
    }

    public async Task<Result<IReadOnlyList<Device>>> GetDevicesAsync(string plantId, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new { stationCodes = plantId };
            var response = await PostAuthenticatedAsync<DeviceListData>("/thirdData/getDevList", payload, cancellationToken);

            if (response == null || response.List == null)
            {
                return Result<IReadOnlyList<Device>>.Success(Array.Empty<Device>());
            }

            var devices = response.List.Select(d => MapToDevice(d, plantId)).ToList();
            return Result<IReadOnlyList<Device>>.Success(devices);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving devices for plant {PlantId}", plantId);
            return Result<IReadOnlyList<Device>>.Failure($"Error retrieving devices: {ex.Message}");
        }
    }

    public async Task<Result<RealtimeMetrics>> GetPlantRealtimeMetricsAsync(string plantId, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new { stationCodes = plantId };
            var response = await PostAuthenticatedAsync<RealtimeKpiData>("/thirdData/getStationRealKpi", payload, cancellationToken);

            if (response == null || response.DataItemMap == null)
            {
                return Result<RealtimeMetrics>.Failure("No real-time data returned.");
            }

            var metrics = MapToRealtimeMetrics(response.DataItemMap);
            return Result<RealtimeMetrics>.Success(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving real-time metrics for plant {PlantId}", plantId);
            return Result<RealtimeMetrics>.Failure($"Error retrieving metrics: {ex.Message}");
        }
    }

    public Task<Result<RealtimeMetrics>> GetDeviceRealtimeMetricsAsync(string deviceId, CancellationToken cancellationToken = default)
    {
        // Device-level real-time metrics would use /thirdData/getDevRealKpi
        // Implementation similar to plant metrics
        return Task.FromResult(Result<RealtimeMetrics>.Failure("Device real-time metrics not yet implemented."));
    }

    public async Task<Result<EnergySummary>> GetPlantEnergySummaryAsync(string plantId, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new { stationCodes = plantId };
            var response = await PostAuthenticatedAsync<KpiData>("/thirdData/getKpiStationDay", payload, cancellationToken);

            if (response == null || response.DataItemMap == null)
            {
                return Result<EnergySummary>.Failure("No energy summary data returned.");
            }

            var summary = MapToEnergySummary(response.DataItemMap);
            return Result<EnergySummary>.Success(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving energy summary for plant {PlantId}", plantId);
            return Result<EnergySummary>.Failure($"Error retrieving energy summary: {ex.Message}");
        }
    }

    public Task<Result<IReadOnlyList<TimeseriesPoint>>> GetPlantHistoricalDataAsync(
        string plantId,
        MetricType metricType,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        // Historical data would typically use endpoints like /thirdData/getKpiStationHour
        // with collectTime parameter (timestamp in milliseconds)
        // For brevity, returning placeholder
        _logger.LogWarning("Historical data retrieval not yet fully implemented");
        return Task.FromResult(Result<IReadOnlyList<TimeseriesPoint>>.Success(
            (IReadOnlyList<TimeseriesPoint>)Array.Empty<TimeseriesPoint>()));
    }

    private async Task<T?> PostAuthenticatedAsync<T>(string endpoint, object? payload, CancellationToken cancellationToken)
    {
        await EnsureAuthenticatedAsync(cancellationToken);

        var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Headers.Add("XSRF-TOKEN", _authToken);

        if (payload != null)
        {
            request.Content = JsonContent.Create(payload);
        }

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var apiResponse = await response.Content.ReadFromJsonAsync<HuaweiApiResponse<T>>(cancellationToken);

        if (apiResponse == null || !apiResponse.Success)
        {
            throw new HttpRequestException($"API call failed: {apiResponse?.Message ?? "Unknown error"}");
        }

        return apiResponse.Data;
    }

    private async Task<HttpResponseMessage> PostAuthenticatedAsync(string endpoint, object? payload, CancellationToken cancellationToken)
    {
        await EnsureAuthenticatedAsync(cancellationToken);

        var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Headers.Add("XSRF-TOKEN", _authToken);

        if (payload != null)
        {
            request.Content = JsonContent.Create(payload);
        }

        return await _httpClient.SendAsync(request, cancellationToken);
    }

    private async Task EnsureAuthenticatedAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_authToken) || DateTime.UtcNow >= _tokenExpiry)
        {
            var result = await LoginAsync(cancellationToken);
            if (result.IsFailure)
            {
                throw new InvalidOperationException($"Authentication failed: {result.Error}");
            }
        }
    }

    private Plant MapToPlant(StationDto dto)
    {
        return new Plant
        {
            Id = dto.StationCode ?? string.Empty,
            Name = dto.StationName ?? "Unknown",
            Address = dto.StationAddr,
            InstalledCapacityKw = dto.Capacity,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            InstallationDate = dto.BuildTime.HasValue
                ? DateTimeOffset.FromUnixTimeMilliseconds(dto.BuildTime.Value).UtcDateTime
                : null,
            Status = MapPlantStatus(dto.StationLinkStatus),
            LastUpdateTime = DateTime.UtcNow
        };
    }

    private Device MapToDevice(DeviceDto dto, string plantId)
    {
        return new Device
        {
            Id = dto.Id ?? string.Empty,
            PlantId = plantId,
            Name = dto.DevName ?? "Unknown",
            Type = MapDeviceType(dto.DevTypeId),
            Model = dto.InvType,
            SerialNumber = dto.EsnCode,
            FirmwareVersion = dto.SoftwareVersion,
            Status = DeviceStatus.Unknown,
            LastCommunicationTime = DateTime.UtcNow
        };
    }

    private RealtimeMetrics MapToRealtimeMetrics(Dictionary<string, DataItem> dataMap)
    {
        return new RealtimeMetrics
        {
            TimestampUtc = DateTime.UtcNow,
            PvPowerKw = ParseDecimal(dataMap, "total_power"),
            GridPowerKw = ParseDecimal(dataMap, "power_profit"),
            DayEnergyKwh = ParseDecimal(dataMap, "day_power"),
            // Add more mappings based on actual Huawei API field names
        };
    }

    private EnergySummary MapToEnergySummary(Dictionary<string, DataItem> dataMap)
    {
        return new EnergySummary
        {
            TimestampUtc = DateTime.UtcNow,
            EnergyTodayKwh = ParseDecimal(dataMap, "day_power") ?? 0,
            EnergyMonthKwh = ParseDecimal(dataMap, "month_power") ?? 0,
            EnergyYearKwh = ParseDecimal(dataMap, "year_power") ?? 0,
            EnergyTotalKwh = ParseDecimal(dataMap, "total_power") ?? 0
        };
    }

    private decimal? ParseDecimal(Dictionary<string, DataItem> dataMap, string key)
    {
        if (dataMap.TryGetValue(key, out var item) && !string.IsNullOrWhiteSpace(item.Value))
        {
            if (decimal.TryParse(item.Value, out var value))
            {
                return value;
            }
        }
        return null;
    }

    private PlantStatus MapPlantStatus(int? status)
    {
        return status switch
        {
            1 => PlantStatus.Connected,
            0 => PlantStatus.Disconnected,
            _ => PlantStatus.Unknown
        };
    }

    private DeviceType MapDeviceType(int? typeId)
    {
        return typeId switch
        {
            1 => DeviceType.StringInverter,
            38 => DeviceType.ResidentialInverter,
            39 => DeviceType.Battery,
            17 => DeviceType.GridMeter,
            _ => DeviceType.Unknown
        };
    }
}
