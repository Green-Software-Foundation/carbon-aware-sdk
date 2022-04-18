using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CarbonAware.Plugins.CarbonIntensity.Configuration;

public static class ServiceCollectionExtensions
{
    public static void AddCarbonIntensityPluginService(this IServiceCollection services)
    {
        services.TryAddSingleton<ICarbonAware, CarbonIntensityPlugin>();
    }
}