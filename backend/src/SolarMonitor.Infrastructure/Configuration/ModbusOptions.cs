namespace SolarMonitor.Infrastructure.Configuration;

/// <summary>
/// Configuration for local Modbus TCP polling of Huawei SUN2000 inverters.
/// </summary>
public class ModbusOptions
{
    public const string SectionName = "Modbus";

    /// <summary>
    /// IP address or hostname of the inverter / Smart Dongle.
    /// </summary>
    public string Host { get; set; } = "192.168.200.1";

    /// <summary>
    /// Modbus TCP port (default 502).
    /// </summary>
    public int Port { get; set; } = 502;

    /// <summary>
    /// Modbus unit identifier (slave id). SUN2000 default is 0x1.
    /// </summary>
    public byte UnitId { get; set; } = 1;

    /// <summary>
    /// Polling interval in seconds. Default is 1s for fast local reads.
    /// </summary>
    public int PollingIntervalSeconds { get; set; } = 1;

    /// <summary>
    /// Connect timeout (seconds) for TCP socket.
    /// </summary>
    public int ConnectTimeoutSeconds { get; set; } = 3;

    /// <summary>
    /// Request timeout (seconds) for Modbus read operations.
    /// </summary>
    public int RequestTimeoutSeconds { get; set; } = 5;

    /// <summary>
    /// Static plant/device metadata to expose to the rest of the app.
    /// </summary>
    public ModbusPlantOptions Plant { get; set; } = new();

    /// <summary>
    /// Register map describing where key measurements are stored.
    /// </summary>
    public ModbusRegisterMapOptions RegisterMap { get; set; } = new();
}

public class ModbusPlantOptions
{
    public string PlantId { get; set; } = "modbus-plant";
    public string PlantName { get; set; } = "Local Modbus Plant";
    public decimal? InstalledCapacityKw { get; set; }
    public string DeviceId { get; set; } = "inverter-1";
    public string DeviceName { get; set; } = "SUN2000 Inverter";
    public string? DeviceModel { get; set; } = "SUN2000";
}

public class ModbusRegisterMapOptions
{
    /// <summary>
    /// Inverter active power (W). Default: holding register 32064 (Int32).
    /// </summary>
    public ModbusRegisterSpec ActivePower { get; set; } = new()
    {
        Address = 32064,
        Type = RegisterValueType.Int32,
        Scale = 1000m // W -> kW
    };

    /// <summary>
    /// Grid import/export power (W). Default: meter active power register 37113 (Int32).
    /// </summary>
    public ModbusRegisterSpec? GridPower { get; set; } = new()
    {
        Address = 37113,
        Type = RegisterValueType.Int32,
        Scale = 1000m
    };

    /// <summary>
    /// Battery power (W). Optional; defaults to common storage register 37760 (Int32).
    /// </summary>
    public ModbusRegisterSpec? BatteryPower { get; set; } = new()
    {
        Address = 37760,
        Type = RegisterValueType.Int32,
        Scale = 1000m
    };

    /// <summary>
    /// Battery state of charge (%). Optional.
    /// </summary>
    public ModbusRegisterSpec? StateOfCharge { get; set; } = new()
    {
        Address = 37765,
        Type = RegisterValueType.UInt16,
        Scale = 1m
    };

    /// <summary>
    /// Day energy yield (0.01 kWh). Default register 32080 (UInt32).
    /// </summary>
    public ModbusRegisterSpec DayEnergy { get; set; } = new()
    {
        Address = 32080,
        Type = RegisterValueType.UInt32,
        Scale = 100m // 0.01 kWh -> kWh
    };

    /// <summary>
    /// Month energy yield (0.01 kWh). Default register 32082 (UInt32).
    /// </summary>
    public ModbusRegisterSpec? MonthEnergy { get; set; } = new()
    {
        Address = 32082,
        Type = RegisterValueType.UInt32,
        Scale = 100m
    };

    /// <summary>
    /// Year energy yield (0.01 kWh). Default register 32084 (UInt32).
    /// </summary>
    public ModbusRegisterSpec? YearEnergy { get; set; } = new()
    {
        Address = 32084,
        Type = RegisterValueType.UInt32,
        Scale = 100m
    };

    /// <summary>
    /// Total active energy yield (0.01 kWh). Default register 32086 (UInt64).
    /// </summary>
    public ModbusRegisterSpec TotalEnergy { get; set; } = new()
    {
        Address = 32086,
        Type = RegisterValueType.UInt64,
        Scale = 100m
    };

    /// <summary>
    /// AC grid voltage (0.1 V). Default register 32016 (UInt16).
    /// </summary>
    public ModbusRegisterSpec? GridVoltage { get; set; } = new()
    {
        Address = 32016,
        Type = RegisterValueType.UInt16,
        Scale = 10m
    };

    /// <summary>
    /// Grid frequency (0.01 Hz). Default register 32020 (UInt16).
    /// </summary>
    public ModbusRegisterSpec? GridFrequency { get; set; } = new()
    {
        Address = 32020,
        Type = RegisterValueType.UInt16,
        Scale = 100m
    };

    /// <summary>
    /// PV input voltage (0.1 V). Default register 32014 (UInt16).
    /// </summary>
    public ModbusRegisterSpec? PvVoltage { get; set; } = new()
    {
        Address = 32014,
        Type = RegisterValueType.UInt16,
        Scale = 10m
    };

    /// <summary>
    /// Inverter temperature (0.1 C). Default register 32087 (Int16).
    /// </summary>
    public ModbusRegisterSpec? Temperature { get; set; } = new()
    {
        Address = 32087,
        Type = RegisterValueType.Int16,
        Scale = 10m
    };
}

public class ModbusRegisterSpec
{
    /// <summary>
    /// Base register address.
    /// </summary>
    public ushort Address { get; set; }

    /// <summary>
    /// Data type layout for the register.
    /// </summary>
    public RegisterValueType Type { get; set; } = RegisterValueType.Int16;

    /// <summary>
    /// Divisor used to scale raw values into engineering units.
    /// </summary>
    public decimal Scale { get; set; } = 1m;
}

public enum RegisterValueType
{
    Int16 = 0,
    UInt16 = 1,
    Int32 = 2,
    UInt32 = 3,
    UInt64 = 4
}
