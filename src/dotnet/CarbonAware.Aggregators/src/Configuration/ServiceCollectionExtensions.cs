using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using CarbonAware.Plugins.Configuration;
using CarbonAware.Aggregators.CarbonAware;

namespace CarbonAware.Aggregators.Configuration;

public static class ServiceCollectionExtensions
{
    public static void AddCarbonAwareEmissionServices(this IServiceCollection services)
    {
        services.AddPluginService(PluginType.JSON);
        services.TryAddSingleton<ICarbonAwareAggregator, CarbonAwareAggregator>();
    }
}