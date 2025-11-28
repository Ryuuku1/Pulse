namespace SolarMonitor.Domain.ValueObjects;

/// <summary>
/// Represents real-time power and operational metrics for a plant or device.
/// All power values are in kilowatts (kW).
/// </summary>
public record RealtimeMetrics
{
    public required DateTime TimestampUtc { get; init; }

    /// <summary>
    /// Current PV generation power (kW). Null if not available.
    /// </summary>
    public decimal? PvPowerKw { get; init; }

    /// <summary>
    /// Current grid power (kW). Positive = exporting to grid, Negative = importing from grid.
    /// </summary>
    public decimal? GridPowerKw { get; init; }

    /// <summary>
    /// Current load/consumption power (kW). Null if not measured.
    /// </summary>
    public decimal? LoadPowerKw { get; init; }

    /// <summary>
    /// Current battery power (kW). Positive = charging, Negative = discharging. Null if no battery.
    /// </summary>
    public decimal? BatteryPowerKw { get; init; }

    /// <summary>
    /// Battery state of charge (0-100%). Null if no battery.
    /// </summary>
    public decimal? StateOfChargePercent { get; init; }

    /// <summary>
    /// Inverter efficiency (0-100%). Null if not available.
    /// </summary>
    public decimal? EfficiencyPercent { get; init; }

    /// <summary>
    /// Current day's generated energy so far (kWh).
    /// </summary>
    public decimal? DayEnergyKwh { get; init; }

    /// <summary>
    /// Grid voltage (V). Null if not measured.
    /// </summary>
    public decimal? GridVoltageV { get; init; }

    /// <summary>
    /// Grid frequency (Hz). Null if not measured.
    /// </summary>
    public decimal? GridFrequencyHz { get; init; }

    /// <summary>
    /// PV input voltage (V). Null if not measured or multiple strings.
    /// </summary>
    public decimal? PvVoltageV { get; init; }

    /// <summary>
    /// Inverter temperature (°C). Null if not measured.
    /// </summary>
    public decimal? TemperatureC { get; init; }
}
