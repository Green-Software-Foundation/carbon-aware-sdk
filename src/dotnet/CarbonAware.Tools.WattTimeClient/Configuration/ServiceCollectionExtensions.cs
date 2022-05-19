using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics;
using System.Net;

namespace CarbonAware.Tools.WattTimeClient.Configuration;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Method to configure and add the WattTime client to the service collection.
    /// <param name="services">The service collection to add the client to.</param>
    /// <param name="configuration">The configuration to use to configure the client.</param>
    /// <returns>The service collection with the configured client added.</returns>
    /// </summary>
    public static IServiceCollection ConfigureWattTimeClient(this IServiceCollection services, IConfiguration configuration)
    {

        var source = new ActivitySource("WattTimeClient");

        WattTimeClientConfiguration config = new WattTimeClientConfiguration();

        // configuring dependency injection to have config.
        services.Configure<WattTimeClientConfiguration>(c =>
        {
            configuration.GetSection(WattTimeClientConfiguration.Key).Bind(c);
        });
        var configVars = configuration.GetSection(CarbonAwareVariablesConfiguration.Key).Get<CarbonAwareVariablesConfiguration>();
        if (configVars?.Proxy?.UseProxy == true)
        {
            if (String.IsNullOrEmpty(configVars.Proxy.Url))
            {
                throw new ConfigurationException("Url is missing.");
            }
            services.AddHttpClient<WattTimeClient>(IWattTimeClient.NamedClient)
                .ConfigurePrimaryHttpMessageHandler(() => 
                    new HttpClientHandler() {
                        Proxy = new WebProxy(configVars.Proxy.Url, true),
                        Credentials = new NetworkCredential(configVars.Proxy.Username, configVars.Proxy.Password)
                    });
        }
        else
        {
            services.AddHttpClient<WattTimeClient>(IWattTimeClient.NamedClient);
        }

        services.TryAddSingleton<IWattTimeClient, WattTimeClient>();
        services.TryAddSingleton<ActivitySource>(source);

        return services;
    }
}
