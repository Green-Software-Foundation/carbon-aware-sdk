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
        // find all the Classes in the Assembly that implements AddEmissionServices method,
        // and added them here with the specific implementation class.
        var carbonAwareConfig = configuration.GetSection(CarbonAwareVariablesConfiguration.Key).Get<CarbonAwareVariablesConfiguration>();
        var forecastDataSource = GetDataSourceTypeFromValue(carbonAwareConfig?.ForecastDataSource);
        var emissionsDataSource = GetDataSourceTypeFromValue(carbonAwareConfig?.EmissionsDataSource);


        switch (forecastDataSource)
        {
            case DataSourceType.JSON:
            {
                throw new ArgumentException("JSON data source is not supported for forecast data");
            }
            case DataSourceType.WattTime:
            {
                services.AddWattTimeForecastDataSource(configuration);
                break;
            }
            case DataSourceType.None:
            {
                services.TryAddSingleton<IForecastDataSource, NullForecastDataSource>();
                break;
            }
        }

        switch (emissionsDataSource)
        {
            case DataSourceType.JSON:
            {
                services.AddJsonEmissionsDataSource(configuration);
                break;
            }
            case DataSourceType.WattTime:
            {
                services.AddWattTimeEmissionsDataSource(configuration);
                break;
            }
            case DataSourceType.None:
            {
                services.TryAddSingleton<IEmissionsDataSource, NullEmissionsDataSource>();
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
