using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Configuration;
using CarbonAware.Interfaces;
using CarbonAware.Tools.WattTimeClient.Configuration;
using CarbonAware.LocationSources;
using CarbonAware.LocationSources.Configuration;

namespace CarbonAware.DataSources.WattTime.Configuration;

public static class ServiceCollectionExtensions
{
    public static void AddWattTimeForecastDataSource(this IServiceCollection services, IConfiguration? configuration)
    {
        _ = configuration ?? throw new ConfigurationException("WattTime configuration required.");
        services.ConfigureWattTimeClient(configuration);
        services.TryAddSingleton<IForecastDataSource, WattTimeDataSource>();
        services.Configure<LocationDataSourcesConfiguration>(c =>
        {
            configuration.GetSection(LocationDataSourcesConfiguration.Key).Bind(c);
        });
        services.TryAddSingleton<ILocationSource, LocationSource>();
    }

    public static void AddWattTimeEmissionsDataSource(this IServiceCollection services, IConfiguration? configuration)
    {
        _ = configuration ?? throw new ConfigurationException("WattTime configuration required.");
        services.ConfigureWattTimeClient(configuration);
        services.TryAddSingleton<IEmissionsDataSource, WattTimeDataSource>();
        services.Configure<LocationDataSourcesConfiguration>(c =>
        {
            configuration.GetSection(LocationDataSourcesConfiguration.Key).Bind(c);
        });
        services.TryAddSingleton<ILocationSource, LocationSource>();
    }
}
