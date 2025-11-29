using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Modbus;
using Modbus.Device;
using SolarMonitor.Application.Ports;
using SolarMonitor.Domain.Common;
using SolarMonitor.Domain.ValueObjects;
using SolarMonitor.Infrastructure.Configuration;

namespace SolarMonitor.Infrastructure.Modbus;

/// <summary>
/// Modbus TCP client for Huawei SUN2000 inverters using the NModbus library.
/// </summary>
public class ModbusTcpClient : IModbusClient
{
    private readonly ILogger<ModbusTcpClient> _logger;
    private readonly ModbusOptions _options;

    public ModbusTcpClient(
        IOptions<ModbusOptions> options,
        ILogger<ModbusTcpClient> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<Result<RealtimeMetrics>> ReadRealtimeMetricsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var (tcpClient, master) = await CreateMasterAsync(cancellationToken);

            using (tcpClient)
            using (master)
            {
                var map = _options.RegisterMap;

                var pvPower = await ReadDecimalAsync(master, map.ActivePower, cancellationToken);
                var gridPower = await ReadDecimalAsync(master, map.GridPower, cancellationToken);
                var batteryPower = await ReadDecimalAsync(master, map.BatteryPower, cancellationToken);
                var soc = await ReadDecimalAsync(master, map.StateOfCharge, cancellationToken);
                var gridVoltage = await ReadDecimalAsync(master, map.GridVoltage, cancellationToken);
                var gridFrequency = await ReadDecimalAsync(master, map.GridFrequency, cancellationToken);
                var pvVoltage = await ReadDecimalAsync(master, map.PvVoltage, cancellationToken);
                var temperature = await ReadDecimalAsync(master, map.Temperature, cancellationToken);
                var dayEnergy = await ReadDecimalAsync(master, map.DayEnergy, cancellationToken);

                var metrics = new RealtimeMetrics
                {
                    TimestampUtc = DateTime.UtcNow,
                    PvPowerKw = pvPower,
                    GridPowerKw = gridPower ?? pvPower,
                    LoadPowerKw = null, // Not available via default register map
                    BatteryPowerKw = batteryPower,
                    StateOfChargePercent = soc,
                    EfficiencyPercent = null,
                    DayEnergyKwh = dayEnergy,
                    GridVoltageV = gridVoltage,
                    GridFrequencyHz = gridFrequency,
                    PvVoltageV = pvVoltage,
                    TemperatureC = temperature
                };

                return Result<RealtimeMetrics>.Success(metrics);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Modbus realtime read failed");
            return Result<RealtimeMetrics>.Failure($"Modbus realtime read failed: {ex.Message}");
        }
    }

    public async Task<Result<EnergySummary>> ReadEnergySummaryAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var (tcpClient, master) = await CreateMasterAsync(cancellationToken);

            using (tcpClient)
            using (master)
            {
                var map = _options.RegisterMap;

                var dayEnergy = await ReadDecimalAsync(master, map.DayEnergy, cancellationToken) ?? 0m;
                var monthEnergy = await ReadDecimalAsync(master, map.MonthEnergy, cancellationToken) ?? dayEnergy;
                var yearEnergy = await ReadDecimalAsync(master, map.YearEnergy, cancellationToken) ?? monthEnergy;
                var totalEnergy = await ReadDecimalAsync(master, map.TotalEnergy, cancellationToken);

                if (totalEnergy is null)
                {
                    return Result<EnergySummary>.Failure("Failed to read total energy from Modbus registers.");
                }

                var summary = new EnergySummary
                {
                    TimestampUtc = DateTime.UtcNow,
                    EnergyTodayKwh = dayEnergy,
                    EnergyMonthKwh = monthEnergy,
                    EnergyYearKwh = yearEnergy,
                    EnergyTotalKwh = totalEnergy.Value
                };

                return Result<EnergySummary>.Success(summary);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Modbus energy summary read failed");
            return Result<EnergySummary>.Failure($"Modbus energy summary read failed: {ex.Message}");
        }
    }

    private async Task<(TcpClient TcpClient, IModbusMaster Master)> CreateMasterAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var tcpClient = new TcpClient
        {
            ReceiveTimeout = _options.RequestTimeoutSeconds * 1000,
            SendTimeout = _options.RequestTimeoutSeconds * 1000,
            NoDelay = true
        };

        using var connectCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        connectCts.CancelAfter(TimeSpan.FromSeconds(_options.ConnectTimeoutSeconds));

        await tcpClient.ConnectAsync(_options.Host, _options.Port, connectCts.Token);

        var master = ModbusIpMaster.CreateIp(tcpClient);
        return (tcpClient, master);
    }

    private async Task<decimal?> ReadDecimalAsync(
        IModbusMaster master,
        ModbusRegisterSpec? spec,
        CancellationToken cancellationToken)
    {
        if (spec == null)
        {
            return null;
        }

        cancellationToken.ThrowIfCancellationRequested();

        var registerCount = GetRegisterCount(spec.Type);
        var raw = await master.ReadHoldingRegistersAsync(_options.UnitId, spec.Address, registerCount);

        if (raw is null || raw.Length != registerCount)
        {
            throw new InvalidOperationException($"Unexpected register count while reading {spec.Address}.");
        }

        var value = spec.Type switch
        {
            RegisterValueType.Int16 => (decimal)(short)raw[0],
            RegisterValueType.UInt16 => raw[0],
            RegisterValueType.Int32 => (decimal)unchecked((int)(((uint)raw[0] << 16) | raw[1])),
            RegisterValueType.UInt32 => (decimal)(((uint)raw[0] << 16) | raw[1]),
            RegisterValueType.UInt64 => (decimal)(((ulong)raw[0] << 48) | ((ulong)raw[1] << 32) | ((ulong)raw[2] << 16) | raw[3]),
            _ => throw new NotSupportedException($"Unsupported register type {spec.Type}")
        };

        var scale = spec.Scale <= 0 ? 1m : spec.Scale;
        return value / scale;
    }

    private ushort GetRegisterCount(RegisterValueType type) => type switch
    {
        RegisterValueType.Int16 or RegisterValueType.UInt16 => 1,
        RegisterValueType.Int32 or RegisterValueType.UInt32 => 2,
        RegisterValueType.UInt64 => 4,
        _ => throw new NotSupportedException($"Unsupported register type {type}")
    };
}
