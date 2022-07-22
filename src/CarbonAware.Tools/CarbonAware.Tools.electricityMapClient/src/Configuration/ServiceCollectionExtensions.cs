using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System.Net;

namespace CarbonAware.Tools.electricityMapClient.Configuration;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Method to configure and add the WattTime client to the service collection.
    /// <param name="services">The service collection to add the client to.</param>
    /// <param name="configuration">The configuration to use to configure the client.</param>
    /// <returns>The service collection with the configured client added.</returns>
    /// </summary>
    public static IServiceCollection ConfigureelectricityMapClient(this IServiceCollection services, IConfiguration configuration)
    {

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
            LogProxyConfiguration(configuration, configVars);
            services.AddHttpClient<electricityMapClient>(IelectricityMapClient.NamedClient)
                .ConfigurePrimaryHttpMessageHandler(() =>
                    new HttpClientHandler()
                    {
                        Proxy = new WebProxy
                        {
                            Address = new Uri(configVars.Proxy.Url),
                            Credentials = new NetworkCredential(configVars.Proxy.Username, configVars.Proxy.Password),
                            BypassProxyOnLocal = true
                        }
                    });
        }
        else
        {
            services.AddHttpClient<electricityMapClient>(IelectricityMapClient.NamedClient);
        }

        services.TryAddSingleton<IelectricityMapClient, electricityMapClient>();

        return services;
    }

    private static void LogProxyConfiguration(IConfiguration config, CarbonAwareVariablesConfiguration caVars)
    {
        ILoggerFactory factory = LoggerFactory.Create(b => {
            b.AddConfiguration(config.GetSection("Logging"));
            b.AddConsole();
        });
        var logger = factory.CreateLogger<IServiceCollection>();
        logger.LogInformation($"Proxy configured to Url {caVars?.Proxy?.Url} with username {caVars?.Proxy?.Username}");
    }
}
