using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.DataSources.Configuration;

namespace CarbonAware.Aggregators.Configuration;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add services needed in order to pull data from a Carbon Intensity data source.
    /// </summary>
    public static IServiceCollection AddCarbonAwareEmissionServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDataSourceService(configuration);
        services.TryAddSingleton<ICarbonAwareAggregator, CarbonAwareAggregator>();
        return services;
    }
}
