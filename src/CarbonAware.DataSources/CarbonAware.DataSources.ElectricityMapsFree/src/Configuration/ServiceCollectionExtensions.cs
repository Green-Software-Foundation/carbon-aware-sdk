using CarbonAware.Configuration;
using CarbonAware.DataSources.ElectricityMapsFree.Client;
using CarbonAware.Exceptions;
using CarbonAware.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net;

namespace CarbonAware.DataSources.ElectricityMapsFree.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddElectricityMapsFreeForecastDataSource(this IServiceCollection services, DataSourcesConfiguration dataSourcesConfig)
    {
        AddElectricityMapsFreeClient(services, dataSourcesConfig.ForecastConfigurationSection());
        services.TryAddSingleton<IForecastDataSource, ElectricityMapsFreeDataSource>();
        return services;
    }

    // ElectricityMapsFree does not implement IEmissionsDataSource
    /*
    public static IServiceCollection AddElectricityMapsFreeEmissionsDataSource(this IServiceCollection services, DataSourcesConfiguration dataSourcesConfig)
    {
        AddElectricityMapsFreeClient(services, dataSourcesConfig.EmissionsConfigurationSection());
        services.TryAddSingleton<IEmissionsDataSource, ElectricityMapsFreeDataSource>();
        return services;
    }
    */

    private static void AddElectricityMapsFreeClient(IServiceCollection services, IConfigurationSection configSection)
    {
        services.Configure<ElectricityMapsFreeClientConfiguration>(c =>
        {
            configSection.Bind(c);
        });

        var httpClientBuilder = services.AddHttpClient<ElectricityMapsFreeClient>(IElectricityMapsFreeClient.NamedClient);

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
        services.TryAddSingleton<IElectricityMapsFreeClient, ElectricityMapsFreeClient>();
    }
}