using Microsoft.Extensions.DependencyInjection;
using CarbonAware.Plugins.JsonReaderPlugin.Configuration;
using CarbonAware.Plugins.CarbonIntensity.Configuration;

namespace CarbonAware.Plugins.Configuration;
public static class ServiceCollectionExtensions
{
    public static void AddPluginService(this IServiceCollection services, PluginType pType)
    {
        // find all the Classes in the Assembly that implements AddEmissionServices method,
        // and added them here with the specific implementation class.
        switch (pType)
        {
            case PluginType.JSON:
            {
                    services.AddJsonPluginService();
                    break;
            }
            case PluginType.CarbonIntensity:
            {
                    services.AddCarbonIntensityPluginService();
                    break;
            }
        }
    }
}