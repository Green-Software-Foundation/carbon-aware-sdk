using CarbonAware.Configuration;
using CarbonAware.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CarbonAware.DataSources.Json.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJsonEmissionsDataSource(this IServiceCollection services, DataSourcesConfiguration dataSourcesConfig)
    {
        // configuring dependency injection to have config.
        services.Configure<JsonDataSourceConfiguration>(config =>
        {
            dataSourcesConfig.EmissionsConfigurationSection().Bind(config);
        });
        services.TryAddSingleton<IEmissionsDataSource, JsonDataSource>();
        
        return services;
    }

    public static IServiceCollection AddJsonForecastDataSource(this IServiceCollection services, DataSourcesConfiguration dataSourcesConfig)
    {
        services.Configure<JsonDataSourceConfiguration>(config =>
        {
            dataSourcesConfig.ForecastConfigurationSection().Bind(config);
        });
        services.TryAddSingleton<IForecastDataSource, JsonDataSource>();

        return services;
    }
}