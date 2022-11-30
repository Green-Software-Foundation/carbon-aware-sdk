using CarbonAware.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CarbonAware.DataSources.Json.Configuration;

public static class ServiceCollectionExtensions
{
    public static void AddJsonEmissionsDataSource(this IServiceCollection services, IConfiguration configuration)
    {
        // configuring dependency injection to have config.
        services.Configure<JsonDataSourceConfiguration>(c =>
        {
            configuration.GetSection(JsonDataSourceConfiguration.Key).Bind(c);
        });
        services.TryAddSingleton<IEmissionsDataSource, JsonDataSource>();
    }
}