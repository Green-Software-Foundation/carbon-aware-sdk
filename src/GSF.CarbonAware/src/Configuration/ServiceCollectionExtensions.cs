using CarbonAware.DataSources.Configuration;
using CarbonAware.Interfaces;
using CarbonAware.LocationSources;
using CarbonAware.LocationSources.Configuration;
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
        services.Configure<LocationDataSourcesConfiguration>(c =>
        {
            configuration.GetSection(LocationDataSourcesConfiguration.Key).Bind(c);
        });
        services.TryAddSingleton<ILocationSource, LocationSource>();
        services.AddDataSourceService(configuration);
        services.TryAddSingleton<IEmissionsHandler, EmissionsHandler>();
        services.TryAddSingleton<ILocationHandler, LocationHandler>();
        return services;
    }

    /// <summary>
    /// Add services needed in order to use an Forecast service.
    /// </summary>
    public static IServiceCollection AddForecastServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<LocationDataSourcesConfiguration>(c =>
        {
            configuration.GetSection(LocationDataSourcesConfiguration.Key).Bind(c);
        });
        services.TryAddSingleton<ILocationSource, LocationSource>();
        services.AddDataSourceService(configuration);
        services.TryAddSingleton<IForecastHandler, ForecastHandler>();
        services.TryAddSingleton<ILocationHandler, LocationHandler>();
        return services;
    }
}
