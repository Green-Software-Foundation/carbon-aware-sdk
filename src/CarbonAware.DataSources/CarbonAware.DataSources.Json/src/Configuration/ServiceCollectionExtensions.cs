using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using CarbonAware.Interfaces;
namespace CarbonAware.DataSources.Json.Configuration;

public static class ServiceCollectionExtensions
{
    public static void AddJsonDataSourceService(this IServiceCollection services)
    {
        services.TryAddSingleton<ICarbonIntensityDataSource, JsonDataSource>();
    }
}