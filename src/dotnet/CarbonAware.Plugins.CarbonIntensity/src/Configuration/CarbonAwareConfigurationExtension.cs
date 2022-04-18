using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using CarbonAware.Plugins;

namespace CarbonAware.Plugins.CarbonIntensity.Configuration;

public static class CarbonAwareServicesConfiguration
{
    public static void AddCarbonIntensityServices(this IServiceCollection services)
    {
        services.TryAddSingleton<ICarbonAware, CarbonIntensityPlugin>();
    }
}