using Microsoft.Extensions.DependencyInjection;
using CarbonAware.DataSources.Json.Configuration;
using CarbonAware.DataSources.WattTime.Configuration;

namespace CarbonAware.DataSources.Configuration;
public static class ServiceCollectionExtensions
{
    public static void AddDataSourceService(this IServiceCollection services, DataSourceType dataSourceType)
    {
        // find all the Classes in the Assembly that implements AddEmissionServices method,
        // and added them here with the specific implementation class.
        switch (dataSourceType)
        {
            case DataSourceType.JSON:
            {
                    services.AddJsonDataSourceService();
                    break;
            }
            case DataSourceType.WattTime:
            {
                    services.AddWattTimeDataSourceService();
                    break;
            }
        }
    }
}