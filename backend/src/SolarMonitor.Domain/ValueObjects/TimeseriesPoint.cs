namespace SolarMonitor.Domain.ValueObjects;

/// <summary>
/// Represents a single data point in a time series.
/// </summary>
public record TimeseriesPoint
{
    public required DateTime TimestampUtc { get; init; }
    public required MetricType MetricType { get; init; }
    public required decimal Value { get; init; }
    public string? Unit { get; init; }
}

public enum MetricType
{
    Power = 1,
    Energy = 2,
    Voltage = 3,
    Current = 4,
    Temperature = 5,
    Frequency = 6,
    StateOfCharge = 7,
    Efficiency = 8
}
