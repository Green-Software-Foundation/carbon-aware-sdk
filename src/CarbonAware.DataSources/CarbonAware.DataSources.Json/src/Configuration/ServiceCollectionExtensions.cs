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
        services.Configure<JsonDataConfiguration>(c =>
        {
            configuration.GetSection(JsonDataConfiguration.Key).Bind(c);
        });
        services.TryAddSingleton<ICarbonIntensityDataSource, JsonDataSource>();
    }
}