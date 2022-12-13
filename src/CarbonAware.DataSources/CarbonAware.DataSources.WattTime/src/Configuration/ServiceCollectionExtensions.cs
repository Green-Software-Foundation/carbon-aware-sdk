using CarbonAware.Configuration;
using CarbonAware.Exceptions;
using CarbonAware.Interfaces;
using CarbonAware.Tools.WattTimeClient;
using CarbonAware.Tools.WattTimeClient.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net;

namespace CarbonAware.DataSources.WattTime.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWattTimeForecastDataSource(this IServiceCollection services, DataSourcesConfiguration dataSourcesConfig)
    {
        AddDependencies(services, dataSourcesConfig.ForecastConfigurationSection());
        services.TryAddSingleton<IForecastDataSource, WattTimeDataSource>();
        return services;
    }

    public static IServiceCollection AddWattTimeEmissionsDataSource(this IServiceCollection services, DataSourcesConfiguration dataSourcesConfig)
    {
        AddDependencies(services, dataSourcesConfig.EmissionsConfigurationSection());
        services.TryAddSingleton<IEmissionsDataSource, WattTimeDataSource>();
        return services;
    }
    
    private static void AddDependencies(IServiceCollection services, IConfigurationSection configSection)
    {
        AddWattTimeClient(services, configSection);
        services.AddMemoryCache();
    }

    private static void AddWattTimeClient(IServiceCollection services, IConfigurationSection configSection)
    {
        services.Configure<WattTimeClientConfiguration>(c =>
        {
            configSection.Bind(c);
        });

        var httpClientBuilder = services.AddHttpClient<WattTimeClient>(IWattTimeClient.NamedClient);

        var Proxy = configSection.GetSection("Proxy").Get<WebProxyConfiguration>();
        if (Proxy != null && Proxy.UseProxy == true)
        {
            if (String.IsNullOrEmpty(Proxy.Url))
            {
                throw new Exceptions.ConfigurationException("Proxy Url is not configured.");
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
        services.TryAddSingleton<IWattTimeClient, WattTimeClient>();
    }
}
