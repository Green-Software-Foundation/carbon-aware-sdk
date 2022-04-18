using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using CarbonAware.Plugins.Configuration;

namespace CarbonAware.Aggregators.Configuration;

public static class ServiceCollectionExtensions
{
    public static void AddCarbonAwareEmissionServices(this IServiceCollection services)
    {
        services.AddEmissionServices(PluginType.JSON);
    }
}