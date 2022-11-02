using CarbonAware.Aggregators.Configuration;
using GSF.CarbonAware.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GSF.CarbonAware.Configuration;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add services needed in order to use an Emissions service.
    /// </summary>
    public static IServiceCollection AddEmissionsServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCarbonAwareEmissionServices(configuration);
        services.TryAddSingleton<IEmissionsHandler, EmissionsHandler>();
        return services;
    }

    /// <summary>
    /// Add services needed in order to use an Forecast service.
    /// </summary>
    public static IServiceCollection AddForecastServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCarbonAwareEmissionServices(configuration);
        services.TryAddSingleton<IForecastHandler, ForecastHandler>();
        return services;
    }
}
