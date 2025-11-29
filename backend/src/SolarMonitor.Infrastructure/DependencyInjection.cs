using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SolarMonitor.Application.Ports;
using SolarMonitor.Infrastructure.BackgroundServices;
using SolarMonitor.Infrastructure.Caching;
using SolarMonitor.Infrastructure.Configuration;
using SolarMonitor.Infrastructure.Huawei;
using SolarMonitor.Infrastructure.Modbus;

namespace SolarMonitor.Infrastructure;

/// <summary>
/// Extension methods for configuring Infrastructure layer services in DI container.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configuration
        services.Configure<HuaweiApiOptions>(
            configuration.GetSection(HuaweiApiOptions.SectionName));
        services.Configure<ModbusOptions>(
            configuration.GetSection(ModbusOptions.SectionName));
        services.Configure<DataAcquisitionOptions>(
            configuration.GetSection(DataAcquisitionOptions.SectionName));

        // Huawei API Client
        services.AddHttpClient<IHuaweiClient, HuaweiClient>();

        // Modbus Client
        services.AddSingleton<IModbusClient, ModbusTcpClient>();

        // Caching
        services.AddSingleton<IMetricsCache, InMemoryMetricsCache>();

        // Background Services
        services.AddHostedService<PlantDataPollingService>();

        return services;
    }
}
