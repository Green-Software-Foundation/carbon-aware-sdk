using CarbonAware.Plugins.NewBasicJsonPlugin;

namespace CarbonAware.WebApi.Configuration;

public static class CarbonAwareServicesConfiguration
{
    public static void AddCarbonAwareServices(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection("ConfigDataSection");
        var pluginType = section.GetValue<string>("PLUGIN");
        if (pluginType == "WattTime")
        {
            services.AddHttpClient();
            // services.AddSingleton<ICarbonAware, CarbonAwareWattTime>();
            // services.AddSingleton<IRestClientWattTime, RestClientWattTime>();
        } else {
            services.AddSingleton<ICarbonAware, CarbonAwareJsonReaderPlugin>();
        }
    }
}