using Microsoft.Extensions.Configuration;

namespace CarbonAware.Configuration;

internal static class EmissionsDataCacheConfigurationExtensions
{
    public static EmissionsDataCacheConfiguration EmissionsDataCache(this IConfiguration configuration)
    {
        var dataCache = configuration.GetSection(EmissionsDataCacheConfiguration.Key).Get<EmissionsDataCacheConfiguration>() ?? new EmissionsDataCacheConfiguration();
        dataCache.AssertValid();

        return dataCache;
    }
}