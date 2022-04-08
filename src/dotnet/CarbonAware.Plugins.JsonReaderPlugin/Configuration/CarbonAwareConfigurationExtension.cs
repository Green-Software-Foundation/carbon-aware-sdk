
using Microsoft.Extensions.DependencyInjection;

namespace CarbonAware.Plugins.JsonReaderPlugin.Configuration;

public static class CarbonAwareServicesConfiguration
{
    public static void AddCarbonAwareServices(this IServiceCollection services)
    {
        services.AddSingleton<ICarbonAware, CarbonAwareJsonReaderPlugin>();
    }
}