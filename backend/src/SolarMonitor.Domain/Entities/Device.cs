namespace SolarMonitor.Domain.Entities;

/// <summary>
/// Represents a device (inverter, battery, etc.) in a solar plant.
/// </summary>
public class Device
{
    public required string Id { get; init; }
    public required string PlantId { get; init; }
    public required string Name { get; init; }
    public DeviceType Type { get; init; }
    public string? Model { get; init; }
    public string? SerialNumber { get; init; }
    public string? FirmwareVersion { get; init; }
    public DeviceStatus Status { get; set; }
    public DateTime? LastCommunicationTime { get; set; }
}

public enum DeviceType
{
    Unknown = 0,
    StringInverter = 1,
    ResidentialInverter = 2,
    Battery = 3,
    GridMeter = 4,
    PowerSensor = 5,
    EMI = 6,
    ESS = 7
}

public enum DeviceStatus
{
    Unknown = 0,
    Normal = 1,
    Fault = 2,
    Offline = 3,
    Standby = 4,
    Shutdown = 5
}
