using Microsoft.Extensions.DependencyInjection;
using SolarMonitor.Application.Services;

namespace SolarMonitor.Application;

/// <summary>
/// Extension methods for configuring Application layer services in DI container.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IPlantService, PlantService>();
        services.AddScoped<IMetricsService, MetricsService>();

        return services;
    }
}
