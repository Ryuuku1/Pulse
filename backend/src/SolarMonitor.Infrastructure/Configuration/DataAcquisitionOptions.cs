namespace SolarMonitor.Infrastructure.Configuration;

/// <summary>
/// Global data acquisition settings controlling whether the app reads from Huawei cloud API or Modbus TCP.
/// </summary>
public class DataAcquisitionOptions
{
    public const string SectionName = "DataAcquisition";

    /// <summary>
    /// Source to use when polling inverter data.
    /// </summary>
    public DataSourceMode Mode { get; set; } = DataSourceMode.HuaweiApi;

    /// <summary>
    /// Optional override for polling interval in seconds. If zero or less, a mode-specific default is used.
    /// </summary>
    public int PollingIntervalSeconds { get; set; } = 0;
}

public enum DataSourceMode
{
    HuaweiApi = 0,
    ModbusTcp = 1
}
