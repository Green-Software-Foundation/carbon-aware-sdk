using CarbonAware.Configuration;
using CarbonAware.DataSources.ElectricityMaps.Client;
using CarbonAware.Exceptions;
using CarbonAware.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net;

namespace CarbonAware.DataSources.ElectricityMaps.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddElectricityMapsForecastDataSource(this IServiceCollection services, DataSourcesConfiguration dataSourcesConfig)
    {
        AddElectricityMapsClient(services, dataSourcesConfig.ForecastConfigurationSection());
        services.TryAddSingleton<IForecastDataSource, ElectricityMapsDataSource>();
        return services;
    }

    public static IServiceCollection AddElectricityMapsEmissionsDataSource(this IServiceCollection services, DataSourcesConfiguration dataSourcesConfig)
    {
        throw new NotImplementedException();
    }
    
    private static void AddElectricityMapsClient(IServiceCollection services, IConfigurationSection configSection)
    {
        services.Configure<ElectricityMapsClientConfiguration>(c =>
        {
            configSection.Bind(c);
        });

        var httpClientBuilder = services.AddHttpClient<ElectricityMapsClient>(IElectricityMapsClient.NamedClient);

        var Proxy = configSection.GetSection("Proxy").Get<WebProxyConfiguration>();
        if (Proxy?.UseProxy == true)
        {
            if (String.IsNullOrEmpty(Proxy.Url))
            {
                throw new ConfigurationException("Proxy Url is not configured.");
            }
            httpClientBuilder.ConfigurePrimaryHttpMessageHandler(() => 
                new HttpClientHandler() {
                    Proxy = new WebProxy {
                        Address = new Uri(Proxy.Url),
                        Credentials = new NetworkCredential(Proxy.Username, Proxy.Password),
                        BypassProxyOnLocal = true
                    }
                }
            );
        }
        services.TryAddSingleton<IElectricityMapsClient, ElectricityMapsClient>();
    }
}
