using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using CarbonAware.Interfaces;

namespace CarbonAware.DataSources.Json.Configuration;

public static class ServiceCollectionExtensions
{
    public static void AddJsonDataSourceService(this IServiceCollection services, IConfiguration configuration)
    {
        // configuring dependency injection to have config.
        services.Configure<JsonDataSourceConfiguration>(c =>
        {
            configuration.GetSection(JsonDataSourceConfiguration.Key).Bind(c);
        });
        services.TryAddSingleton<IForecastDataSource, JsonDataSource>();
        services.TryAddSingleton<IEmissionsDataSource, JsonDataSource>();

    }
}