using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Configuration;
using CarbonAware.Interfaces;
using CarbonAware.Tools.electricityMapClient.Configuration;
using CarbonAware.LocationSources.Azure;

namespace CarbonAware.DataSources.electricityMap.Configuration;

public static class ServiceCollectionExtensions
{
    public static void AddelectricityMapDataSourceService(this IServiceCollection services, IConfiguration? configuration)
    {
        _ = configuration ?? throw new ConfigurationException("electricityMap configuration required.");
        services.ConfigureelectricityMapClient(configuration);
        services.TryAddSingleton<ICarbonIntensityDataSource, electricityMapDataSource>();
        services.TryAddSingleton<ILocationSource, AzureLocationSource>();
    }
}