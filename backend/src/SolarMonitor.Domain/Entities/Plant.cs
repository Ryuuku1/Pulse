namespace SolarMonitor.Domain.Entities;

/// <summary>
/// Represents a solar plant (station) in the Huawei FusionSolar system.
/// </summary>
public class Plant
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string? Address { get; init; }
    public decimal? InstalledCapacityKw { get; init; }
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }
    public DateTime? InstallationDate { get; init; }
    public PlantStatus Status { get; set; }
    public DateTime? LastUpdateTime { get; set; }
}

public enum PlantStatus
{
    Unknown = 0,
    Connected = 1,
    Disconnected = 2,
    Fault = 3,
    Offline = 4
}
