namespace SolarMonitor.Domain.ValueObjects;

/// <summary>
/// Represents cumulative energy production over various time periods.
/// All energy values are in kilowatt-hours (kWh).
/// </summary>
public record EnergySummary
{
    public required DateTime TimestampUtc { get; init; }

    /// <summary>
    /// Energy generated today (kWh).
    /// </summary>
    public decimal EnergyTodayKwh { get; init; }

    /// <summary>
    /// Energy generated this month (kWh).
    /// </summary>
    public decimal EnergyMonthKwh { get; init; }

    /// <summary>
    /// Energy generated this year (kWh).
    /// </summary>
    public decimal EnergyYearKwh { get; init; }

    /// <summary>
    /// Total lifetime energy generated (kWh).
    /// </summary>
    public decimal EnergyTotalKwh { get; init; }

    /// <summary>
    /// Optional: energy consumed from grid today (kWh). Null if not measured.
    /// </summary>
    public decimal? GridImportTodayKwh { get; init; }

    /// <summary>
    /// Optional: energy exported to grid today (kWh). Null if not measured.
    /// </summary>
    public decimal? GridExportTodayKwh { get; init; }

    /// <summary>
    /// Optional: battery charged energy today (kWh). Null if no battery.
    /// </summary>
    public decimal? BatteryChargeTodayKwh { get; init; }

    /// <summary>
    /// Optional: battery discharged energy today (kWh). Null if no battery.
    /// </summary>
    public decimal? BatteryDischargeTodayKwh { get; init; }
}
