using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using CarbonAware.Plugins;

namespace CarbonAware.Plugins.JsonReaderPlugin.Configuration;

public static class CarbonAwareServicesConfiguration
{
    public static void AddJsonEmissionServices(this IServiceCollection services)
    {
        services.TryAddSingleton<ICarbonAware, CarbonAwareJsonReaderPlugin>();
    }
}