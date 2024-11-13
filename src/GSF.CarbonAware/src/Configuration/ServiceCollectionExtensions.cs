using CarbonAware.DataSources.Configuration;
using CarbonAware.Interfaces;
using CarbonAware.LocationSources;
using CarbonAware.LocationSources.Configuration;
using GSF.CarbonAware.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Linq;

namespace GSF.CarbonAware.Configuration;

public static class ServiceCollectionExtensions
{

    private  static IServiceCollection ConfigureLocationDataSourcesConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<LocationDataSourcesConfiguration>(c =>
        {
            configuration.GetSection(LocationDataSourcesConfiguration.Key).Bind(c);
        });
        return services;
    }

    /// <summary>
    /// Add services needed in order to use an Emissions service.
    /// </summary>
    public static IServiceCollection AddEmissionsServices(this IServiceCollection services, IConfiguration configuration)
    {
        AddLocationService(services, configuration);
        services.AddDataSourceService(configuration);
        services.TryAddSingleton<IEmissionsHandler, EmissionsHandler>();
        services.TryAddSingleton<ILocationHandler, LocationHandler>();
        return services;
    }

    /// <summary>
    /// This stops the location configuration being loaded twice if needed for 
    /// historical emissions and forecasted emissions services.  
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    private static void AddLocationService(IServiceCollection services, IConfiguration configuration)
    {
        if (!services.Any(x => x.ServiceType == typeof(ILocationSource)))
        {
            services.ConfigureLocationDataSourcesConfiguration(configuration);
            services.TryAddSingleton<ILocationSource, LocationSource>();
        }
    }

    /// <summary>
    /// Add services needed in order to use an Forecast service.
    /// </summary>
    public static IServiceCollection AddForecastServices(this IServiceCollection services, IConfiguration configuration)
    {
        AddLocationService(services, configuration);
        services.AddDataSourceService(configuration);
        services.TryAddSingleton<IForecastHandler, ForecastHandler>();
        services.TryAddSingleton<ILocationHandler, LocationHandler>();
        return services;
    }
}
