using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics;
using System.Net;

namespace CarbonAware.Tools.electricityMapClient.Configuration;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Method to configure and add the electricityMap client to the service collection.
    /// <param name="services">The service collection to add the client to.</param>
    /// <param name="configuration">The configuration to use to configure the client.</param>
    /// <returns>The service collection with the configured client added.</returns>
    /// </summary>
    public static IServiceCollection ConfigureelectricityMapClient(this IServiceCollection services, IConfiguration configuration)
    {

        var source = new ActivitySource("electricityMapClient");

        electricityMapClientConfiguration config = new electricityMapClientConfiguration();

        // configuring dependency injection to have config.
        services.Configure<electricityMapClientConfiguration>(c =>
        {
            configuration.GetSection(electricityMapClientConfiguration.Key).Bind(c);
        });
        var configVars = configuration.GetSection(CarbonAwareVariablesConfiguration.Key).Get<CarbonAwareVariablesConfiguration>();
        if (configVars?.Proxy?.UseProxy == true)
        {
            if (String.IsNullOrEmpty(configVars.Proxy.Url))
            {
                throw new ConfigurationException("Url is missing.");
            }
            services.AddHttpClient<electricityMapClient>(IelectricityMapClient.NamedClient)
                .ConfigurePrimaryHttpMessageHandler(() => 
                    new HttpClientHandler() {
                        Proxy = new WebProxy(configVars.Proxy.Url, true),
                        Credentials = new NetworkCredential(configVars.Proxy.Username, configVars.Proxy.Password)
                    });
        }
        else
        {
            services.AddHttpClient<electricityMapClient>(IelectricityMapClient.NamedClient);
        }

        services.TryAddSingleton<IelectricityMapClient, electricityMapClient>();
        services.TryAddSingleton<ActivitySource>(source);

        return services;
    }
}
