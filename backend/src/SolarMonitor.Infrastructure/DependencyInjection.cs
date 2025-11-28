using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SolarMonitor.Application.Ports;
using SolarMonitor.Infrastructure.BackgroundServices;
using SolarMonitor.Infrastructure.Caching;
using SolarMonitor.Infrastructure.Configuration;
using SolarMonitor.Infrastructure.Huawei;

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

        // Huawei API Client
        services.AddHttpClient<IHuaweiClient, HuaweiClient>();

        // Caching
        services.AddSingleton<IMetricsCache, InMemoryMetricsCache>();

        // Background Services
        services.AddHostedService<HuaweiDataPollingService>();

        return services;
    }
}
