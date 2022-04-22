using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using CarbonAware.Interfaces;
using System.Diagnostics;

namespace CarbonAware.DataSources.Json.Configuration;

public static class ServiceCollectionExtensions
{
    public static void AddJsonDataSourceService(this IServiceCollection services)
    {
        var source = new ActivitySource("JsonDataSource");
        services.TryAddSingleton<ICarbonIntensityDataSource, JsonDataSource>();
        services.TryAddSingleton<ActivitySource>(source);
    }
}