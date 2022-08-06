using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Configuration;
using CarbonAware.Interfaces;
using CarbonAware.Tools.ElectricityMapClient.Configuration;
using CarbonAware.LocationSources.Azure;

namespace CarbonAware.DataSources.ElectricityMap.Configuration;

public static class ServiceCollectionExtensions
{
    public static void AddElectricityMapDataSourceService(this IServiceCollection services, IConfiguration? configuration)
    {
        _ = configuration ?? throw new ConfigurationException("ElectricityMap configuration required.");
        services.ConfigureElectricityMapClient(configuration);
        services.TryAddSingleton<ICarbonIntensityDataSource, ElectricityMapDataSource>();
        services.TryAddSingleton<ILocationSource, AzureLocationSource>();
    }
}