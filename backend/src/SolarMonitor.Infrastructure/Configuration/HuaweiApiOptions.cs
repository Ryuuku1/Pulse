namespace SolarMonitor.Infrastructure.Configuration;

/// <summary>
/// Configuration options for Huawei FusionSolar API.
/// Bound from appsettings.json under "Huawei" section.
/// </summary>
public class HuaweiApiOptions
{
    public const string SectionName = "Huawei";

    /// <summary>
    /// Base URL of the Huawei FusionSolar API (e.g., https://eu5.fusionsolar.huawei.com).
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Username for OpenAPI account.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Password (also called SystemCode) for authentication.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Optional: Station/Plant code if required by your API setup.
    /// </summary>
    public string? StationCode { get; set; }

    /// <summary>
    /// Polling interval in seconds for background data sync (default: 30 seconds).
    /// Huawei API has strict rate limits (~1 request/minute per endpoint), so keep this reasonable.
    /// </summary>
    public int PollingIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// Request timeout in seconds (default: 30 seconds).
    /// </summary>
    public int RequestTimeoutSeconds { get; set; } = 30;
}
