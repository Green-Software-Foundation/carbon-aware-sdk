using CarbonAware.Plugins.JsonReaderPlugin;

namespace CarbonAware.WebApi.Configuration;

public static class CarbonAwareServicesConfiguration
{
    public static void AddCarbonAwareServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ICarbonAware, CarbonAwareJsonReaderPlugin>();
    }
}