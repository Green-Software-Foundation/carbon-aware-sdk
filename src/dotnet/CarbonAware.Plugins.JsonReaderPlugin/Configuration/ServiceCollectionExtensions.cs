using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CarbonAware.Plugins.JsonReaderPlugin.Configuration;

public static class ServiceCollectionExtensions
{
    public static void AddJsonPluginService(this IServiceCollection services)
    {
        services.TryAddSingleton<ICarbonAware, CarbonAwareJsonReaderPlugin>();
    }
}