using CarbonAware.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CarbonAware.Interfaces;

internal interface IDataSource
{
    public static IServiceCollection ConfigureDI<T>(IServiceCollection services, DataSourcesConfiguration dataSourcesConfig)
    {
        throw new NotImplementedException();
    }
}