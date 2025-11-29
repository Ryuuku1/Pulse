using SolarMonitor.Domain.Common;
using SolarMonitor.Domain.ValueObjects;

namespace SolarMonitor.Application.Ports;

/// <summary>
/// Port (interface) for reading inverter data over Modbus TCP without using the Huawei cloud API.
/// </summary>
public interface IModbusClient
{
    /// <summary>
    /// Reads real-time metrics from the inverter via Modbus.
    /// </summary>
    Task<Result<RealtimeMetrics>> ReadRealtimeMetricsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads cumulative energy values from the inverter via Modbus.
    /// </summary>
    Task<Result<EnergySummary>> ReadEnergySummaryAsync(CancellationToken cancellationToken = default);
}
