using CarbonAware.Configuration;
using CarbonAware.DataSources.Json.Configuration;
using CarbonAware.DataSources.WattTime.Configuration;
using CarbonAware.Exceptions;
using CarbonAware.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CarbonAware.DataSources.Configuration;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataSourceService(this IServiceCollection services, IConfiguration configuration)
    {
        var dataSources = configuration.GetSection(DataSourcesConfiguration.Key).Get<DataSourcesConfiguration>() ?? new DataSourcesConfiguration();
        dataSources.ConfigurationSection = configuration.GetSection($"{DataSourcesConfiguration.Key}:Configurations");
        dataSources.AssertValid();

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
            case DataSourceType.None:
            {
                services.TryAddSingleton<IEmissionsDataSource, NullEmissionsDataSource>();
                break;
            }
        }

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
}
