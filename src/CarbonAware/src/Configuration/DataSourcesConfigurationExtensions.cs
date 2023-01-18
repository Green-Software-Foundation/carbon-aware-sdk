using Microsoft.Extensions.Configuration;

namespace CarbonAware.Configuration;

public static class DataSourcesConfigurationExtensions
{
    public static DataSourcesConfiguration DataSources(this IConfiguration configuration)
    {
        var dataSources = configuration.GetSection(DataSourcesConfiguration.Key).Get<DataSourcesConfiguration>() ?? new DataSourcesConfiguration();
        dataSources.ConfigurationSection = configuration.GetSection($"{DataSourcesConfiguration.Key}:Configurations");
        dataSources.AssertValid();

        return dataSources;
    }
}