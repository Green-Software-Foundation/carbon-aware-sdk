using Microsoft.Extensions.DependencyInjection;
using CarbonAware.DataSources.Json.Configuration;
using CarbonAware.DataSources.WattTime.Configuration;
using Microsoft.Extensions.Configuration;

namespace CarbonAware.DataSources.Configuration;
public static class ServiceCollectionExtensions
{
    public static void AddDataSourceService(this IServiceCollection services, IConfiguration configuration)
    {
        // find all the Classes in the Assembly that implements AddEmissionServices method,
        // and added them here with the specific implementation class.
        var envVars = configuration.GetSection(CarbonAwareVariablesConfiguration.Key).Get<CarbonAwareVariablesConfiguration>();
        var dataSourceType = GetDataSourceTypeFromValue(envVars?.CarbonIntensityDataSource);

        switch (dataSourceType)
        {
            case DataSourceType.JSON:
            {
                    services.AddJsonDataSourceService(configuration);
                    break;
            }
            case DataSourceType.WattTime:
            {
                    services.AddWattTimeDataSourceService(configuration);
                    break;
            }
            case DataSourceType.None:
            {
                throw new NotSupportedException($"DataSourceType {dataSourceType.ToString()} not supported");
            }
        }
    }
    private static DataSourceType GetDataSourceTypeFromValue(string? srcVal)
    {
        DataSourceType result;
        if (String.IsNullOrEmpty(srcVal) ||
            !Enum.TryParse<DataSourceType>(srcVal, true, out result))
        {
            // defaults to JSON in case env is empty, null or parsing fails
            return DataSourceType.JSON;
        }
        return result;
    }
}
