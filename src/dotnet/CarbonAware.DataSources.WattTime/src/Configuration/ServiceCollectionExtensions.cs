using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Configuration;
using CarbonAware.Interfaces;
using CarbonAware.Tools.WattTimeClient.Configuration;
using CarbonAware.LocationSources.Azure;

namespace CarbonAware.DataSources.WattTime.Configuration;

public static class ServiceCollectionExtensions
{
    public static void AddWattTimeDataSourceService(this IServiceCollection services)
    {
        var configurationBuilder = new ConfigurationBuilder();
        var config = configurationBuilder.Build();
        services.ConfigureWattTimeClient(config);
        services.TryAddSingleton<ICarbonIntensityDataSource, WattTimeDataSource>();
        services.TryAddSingleton<ILocationSource, AzureLocationSource>();
    }
}