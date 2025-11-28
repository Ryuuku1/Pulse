using System.Text.Json.Serialization;

namespace SolarMonitor.Infrastructure.Huawei.DTOs;

/// <summary>
/// Base response structure for Huawei FusionSolar API calls.
/// </summary>
internal class HuaweiApiResponse<T>
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("failCode")]
    public int? FailCode { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("data")]
    public T? Data { get; set; }
}

/// <summary>
/// Login response containing the access token.
/// </summary>
internal class LoginData
{
    [JsonPropertyName("token")]
    public string? Token { get; set; }
}

/// <summary>
/// Station list response data.
/// </summary>
internal class StationListData
{
    [JsonPropertyName("list")]
    public List<StationDto>? List { get; set; }
}

/// <summary>
/// Station/Plant DTO from Huawei API.
/// </summary>
internal class StationDto
{
    [JsonPropertyName("stationCode")]
    public string? StationCode { get; set; }

    [JsonPropertyName("stationName")]
    public string? StationName { get; set; }

    [JsonPropertyName("stationAddr")]
    public string? StationAddr { get; set; }

    [JsonPropertyName("capacity")]
    public decimal? Capacity { get; set; }

    [JsonPropertyName("latitude")]
    public decimal? Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public decimal? Longitude { get; set; }

    [JsonPropertyName("buildTime")]
    public long? BuildTime { get; set; }

    [JsonPropertyName("stationLinkStatus")]
    public int? StationLinkStatus { get; set; }
}

/// <summary>
/// Device list response data.
/// </summary>
internal class DeviceListData
{
    [JsonPropertyName("list")]
    public List<DeviceDto>? List { get; set; }
}

/// <summary>
/// Device DTO from Huawei API.
/// </summary>
internal class DeviceDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("devName")]
    public string? DevName { get; set; }

    [JsonPropertyName("devTypeId")]
    public int? DevTypeId { get; set; }

    [JsonPropertyName("esnCode")]
    public string? EsnCode { get; set; }

    [JsonPropertyName("softwareVersion")]
    public string? SoftwareVersion { get; set; }

    [JsonPropertyName("invType")]
    public string? InvType { get; set; }

    [JsonPropertyName("stationCode")]
    public string? StationCode { get; set; }
}

/// <summary>
/// Real-time KPI data for a station.
/// </summary>
internal class RealtimeKpiData
{
    [JsonPropertyName("dataItemMap")]
    public Dictionary<string, DataItem>? DataItemMap { get; set; }
}

/// <summary>
/// Individual data item in real-time KPI response.
/// </summary>
internal class DataItem
{
    [JsonPropertyName("value")]
    public string? Value { get; set; }

    [JsonPropertyName("unit")]
    public string? Unit { get; set; }
}

/// <summary>
/// Daily/Monthly/Yearly KPI data.
/// </summary>
internal class KpiData
{
    [JsonPropertyName("dataItemMap")]
    public Dictionary<string, DataItem>? DataItemMap { get; set; }
}
