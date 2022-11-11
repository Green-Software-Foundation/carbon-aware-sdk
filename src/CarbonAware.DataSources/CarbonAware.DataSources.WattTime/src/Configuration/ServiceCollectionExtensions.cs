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
    public static void AddWattTimeDataSourceService(this IServiceCollection services, IConfiguration? configuration)
    {
        _ = configuration ?? throw new ConfigurationException("WattTime configuration required.");
        services.ConfigureWattTimeClient(configuration);
        services.TryAddSingleton<ICarbonIntensityDataSource, WattTimeDataSource>();
        // configuring dependency injection to have config.
        services.Configure<LocationDataSourcesConfiguration>(c =>
        {
            configuration.GetSection(LocationDataSourcesConfiguration.Key).Bind(c);
        });
        services.TryAddSingleton<ILocationSource, LocationSource>();
    }
}
