using CarbonAware.Configuration;
using CarbonAware.Interfaces;
using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;

namespace CarbonAware.Proxies.Cache;

/// <summary>
/// A proxy class for <c>IEmissionsDataSource</c> to cache <c>EmissionsData</c>.
/// This class caches data which is queried latestly for each <c>Location</c>.
/// The cached value are used if it satisfies all of the conditions listed below.
/// <list type="bullet">
/// <item>
/// <description>it is not exceeded the expiration period</description>
/// </item>
/// <item>
/// <description>the name of the location is match with the query</description>
/// </item>
/// <item>
/// <description>the time is match with the query</description>
/// </item>
/// </list>
/// </summary>
/// <typeparam name="T">The target class of the proxy</typeparam>
class LatestEmissionsCache<T> : DispatchProxy where T : class, IEmissionsDataSource
{
 
    private IEmissionsDataSource? _target { get; set; }

    readonly private ConcurrentDictionary<string, (DateTimeOffset, IEnumerable<EmissionsData>)> _cache =
        new ConcurrentDictionary<string, (DateTimeOffset, IEnumerable<EmissionsData>)>();

    private EmissionsDataCacheConfiguration? _config { get; set; }

    public static T? CreateProxy(T target, EmissionsDataCacheConfiguration config)
    {
        var proxy = Create<T, LatestEmissionsCache<T>>() as LatestEmissionsCache<T>;
        proxy!._target = target;
        proxy._config = config;
        return proxy as T;
    }

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        if(targetMethod!.Name.Equals("GetCarbonIntensityAsync") && args![0]!.GetType() == typeof(Location))
        {
            var location = (Location?)args[0];
            var start = (DateTimeOffset)args[1]!;
            var end = (DateTimeOffset)args[2]!;
            return ProxyGetCarbonIntensityAsync(targetMethod, location!, start, end);
        } else if(targetMethod.Name.Equals("GetCarbonIntensityAsync") && args![0]!.GetType().GetInterfaces().Contains(typeof(IEnumerable<Location>)))
        {
            var locations = (IEnumerable<Location>?)args[0];
            var start = (DateTimeOffset)args[1]!;
            var end = (DateTimeOffset)args[2]!;
            return ProxyGetCarbonIntensityAsync(targetMethod, locations!, start, end);
        }
        return targetMethod.Invoke(_target, args);
    }

    private Task<IEnumerable<EmissionsData>> ProxyGetCarbonIntensityAsync(MethodInfo? original, Location location, DateTimeOffset periodStartTime, DateTimeOffset periodEndTime)
    {
        var cachedData = GetCachedData(location, periodStartTime, periodEndTime);
        if(cachedData.Count() != 0)
        {
            return Task.FromResult(cachedData);
        }

        object[] args = {location, periodStartTime, periodEndTime};
        var resultFromOriginal = (Task<IEnumerable<EmissionsData>>?)original!.Invoke(_target, args);

        if(string.IsNullOrEmpty(location.Name))
        {
            return resultFromOriginal!;
        } else 
        {
            var expiration = DateTimeOffset.UtcNow.AddMinutes(_config!.ExpirationMin);
            var result = resultFromOriginal!.ContinueWith
            (
                c =>
                {
                    _cache.AddOrUpdate(location.Name, (expiration, c.Result), (_, _) => (expiration, c.Result));
                    return c.Result;
                }
            );
            return result;
        }
    }

    private Task<IEnumerable<EmissionsData>> ProxyGetCarbonIntensityAsync(MethodInfo? original, IEnumerable<Location> locations, DateTimeOffset periodStartTime, DateTimeOffset periodEndTime)
    {
        var cachedData = Enumerable.Empty<EmissionsData>();
        var useCacheFor = Enumerable.Empty<string?>();
        foreach (var location in locations)
        {
            var cachedDataForLocation = GetCachedData(location, periodStartTime, periodEndTime);
            if(cachedDataForLocation.Count() != 0)
            {
                cachedData = cachedData.Union(cachedDataForLocation);
                useCacheFor = useCacheFor.Append(location.Name);
            }
        }

        var locationsForQuery = locations.Where(l => string.IsNullOrEmpty(l.Name) || !useCacheFor.Contains(l.Name));
        if(locationsForQuery.Count() == 0){
            return Task.FromResult(cachedData);
        }

        object[] args = {locationsForQuery, periodStartTime, periodEndTime};
        var resultFromOriginal = (Task<IEnumerable<EmissionsData>>?)original!.Invoke(_target, args);
        
        var expiration = DateTimeOffset.UtcNow.AddMinutes(_config!.ExpirationMin);
        var result = resultFromOriginal!.ContinueWith
            (
                c =>
                {
                    foreach (var location in locationsForQuery)
                    {
                        if(!string.IsNullOrEmpty(location.Name))
                        {
                            var data = c.Result.Where(d => d.Location.Equals(location.Name));
                            _cache.AddOrUpdate(location.Name, (expiration, data), (_, _) => (expiration, data));
                        }
                    }
                    return c.Result.Union(cachedData);
                }
            );
            return result;
    }

    private IEnumerable<EmissionsData> GetCachedData(Location location, DateTimeOffset periodStartTime, DateTimeOffset periodEndTime)
    {
        if(!string.IsNullOrEmpty(location.Name) && _cache.ContainsKey(location.Name))
        {
            var cachedValue = _cache.GetValueOrDefault(location.Name);

            // check expiration
            if(cachedValue.Item1.CompareTo(DateTimeOffset.UtcNow) > 0)
            {
                IEnumerable<EmissionsData> emissions = cachedValue.Item2.Where(d => d.TimeBetween(periodStartTime, periodEndTime));
                if(emissions.Count() != 0)
                {
                    return emissions;
                }
            }
        }
        return Enumerable.Empty<EmissionsData>();
    }
}