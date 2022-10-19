using CarbonAware.Aggregators.Emissions;
using CarbonAware.Aggregators.Forecast;
using CarbonAware.DataSources.Configuration;
using CarbonAware.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CarbonAware.Aggregators.Configuration;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add services needed in order to pull data from a Carbon Intensity data source.
    /// </summary>
    public static IServiceCollection AddCarbonAwareEmissionServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddSingleton<IForecastAggregator, ForecastAggregator>();
        services.TryAddSingleton<IEmissionsAggregator, EmissionsAggregator>();
        services.AddDataSourceService(configuration);
        return services;
    }

    /// <summary>
    /// Add services needed in order to pull data from a Carbon Intensity data source.
    /// </summary>
    public static bool TryAddCarbonAwareEmissionServices(this IServiceCollection services, IConfiguration configuration, out string? errorMessage)
    {
        try
        {
            services.AddCarbonAwareEmissionServices(configuration);
        } catch (ConfigurationException e) {
            errorMessage = e.Message;
            return false;
        }
        errorMessage = null;
        return true;
    }
}
