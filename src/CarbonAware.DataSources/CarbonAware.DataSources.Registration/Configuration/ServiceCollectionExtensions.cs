using CarbonAware.Configuration;
using CarbonAware.Interfaces;
using CarbonAware.DataSources.ElectricityMaps.Configuration;
using CarbonAware.DataSources.ElectricityMapsFree.Configuration;
using CarbonAware.DataSources.Json.Configuration;
using CarbonAware.DataSources.WattTime.Configuration;
using CarbonAware.Exceptions;
using CarbonAware.Proxies.Cache;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CarbonAware.DataSources.Configuration;
internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataSourceService(this IServiceCollection services, IConfiguration configuration)
    {
        var dataSources = configuration.DataSources();

        var emissionsDataSource = GetDataSourceTypeFromValue(dataSources.EmissionsConfigurationType());
        var forecastDataSource = GetDataSourceTypeFromValue(dataSources.ForecastConfigurationType());

        switch (emissionsDataSource)
        {
            case DataSourceType.JSON:
            {
                services.AddJsonEmissionsDataSource(dataSources);
                break;
            }
            case DataSourceType.WattTime:
            {
                services.AddWattTimeEmissionsDataSource(dataSources);
                break;
            }
            case DataSourceType.ElectricityMaps:
            {
                services.AddElectricityMapsEmissionsDataSource(dataSources);
                break;
            }
            case DataSourceType.ElectricityMapsFree:
            {
                services.AddElectricityMapsFreeEmissionsDataSource(dataSources);
                break;
            }
            case DataSourceType.None:
            {
                services.TryAddSingleton<IEmissionsDataSource, NullEmissionsDataSource>();
                break;
            }
        }

        services.SetupCacheForEmissionsDataSource(configuration);

        switch (forecastDataSource)
        {
            case DataSourceType.JSON:
            {
                throw new ArgumentException("JSON data source is not supported for forecast data");
            }
            case DataSourceType.WattTime:
            {
                services.AddWattTimeForecastDataSource(dataSources);
                break;
            }
            case DataSourceType.ElectricityMaps:
            {
                services.AddElectricityMapsForecastDataSource(dataSources);
                break;
            }
            case DataSourceType.ElectricityMapsFree:
            {
                throw new ArgumentException("ElectricityMapsFree data source is not supported for forecast data");
            }
            case DataSourceType.None:
            {
                services.TryAddSingleton<IForecastDataSource, NullForecastDataSource>();
                break;
            }
        }

        if (forecastDataSource == DataSourceType.None && emissionsDataSource == DataSourceType.None)
        {
            throw new ConfigurationException("No data sources are configured");
        }
        
        return services;
    }

    private static DataSourceType GetDataSourceTypeFromValue(string? srcVal)
    {
        DataSourceType result;
        if (String.IsNullOrWhiteSpace(srcVal))
        {
            result = DataSourceType.None;
        }
        else if (!Enum.TryParse<DataSourceType>(srcVal, true, out result))
        {
            throw new ArgumentException($"Unknown data source type: {srcVal}");
        }
        return result;
    }

    private static IServiceCollection SetupCacheForEmissionsDataSource(this IServiceCollection services, IConfiguration configuration)
    {
        var emissionsDataCache = configuration.EmissionsDataCache();
        if(emissionsDataCache.Enabled){
            var emissionsDataSourceDescriptor = services.SingleOrDefault(s => s.ServiceType == typeof(IEmissionsDataSource));
            var type = emissionsDataSourceDescriptor!.ImplementationType;
            if(type == null) return services;
            services.Replace
            (
                ServiceDescriptor.Describe
                (
                    typeof(IEmissionsDataSource),
                    serviceProvider =>
                        LatestEmissionsCache<IEmissionsDataSource>.CreateProxy
                        (
                            (IEmissionsDataSource)ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, type!),
                            emissionsDataCache
                        )!,
                    emissionsDataSourceDescriptor.Lifetime
                )
            );
        }
        return services;
    }
}
