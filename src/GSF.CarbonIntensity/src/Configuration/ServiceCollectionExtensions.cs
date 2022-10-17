using CarbonAware.Aggregators.Configuration;
using GSF.CarbonIntensity.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GSF.CarbonIntensity.Configuration;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add services needed in order to use an CarbonIntensity service.
    /// </summary>
    public static IServiceCollection AddCarbonIntensityServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCarbonAwareEmissionServices(configuration);
        services.TryAddSingleton<IEmissionsHandler, EmissionsHandler>();
        services.TryAddSingleton<IForecastHandler, ForecastHandler>();
        return services;
    }
}
