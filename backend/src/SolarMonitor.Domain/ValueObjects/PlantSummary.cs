using SolarMonitor.Domain.Entities;

namespace SolarMonitor.Domain.ValueObjects;

/// <summary>
/// Aggregates plant metadata with current metrics and energy summary.
/// This is a convenient read model for dashboard displays.
/// </summary>
public record PlantSummary
{
    public required Plant Plant { get; init; }
    public RealtimeMetrics? CurrentMetrics { get; init; }
    public EnergySummary? EnergySummary { get; init; }
    public int TotalDevices { get; init; }
    public int ActiveDevices { get; init; }
}
