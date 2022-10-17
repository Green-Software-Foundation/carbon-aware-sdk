using CarbonAware.Interfaces;
using CarbonAware.Model;

namespace CarbonAware;

public class NullForecastDataSource : IForecastDataSource
{
    private static EmissionsForecast emissionsForecast = new EmissionsForecast()
    {
        RequestedAt = default,
        GeneratedAt = default,
        DataStartAt = default,
        DataEndAt = default,
        WindowSize = default,
        OptimalDataPoints = Enumerable.Empty<EmissionsData>(),
    };

    public Task<EmissionsForecast> GetCarbonIntensityForecastAsync(Location location, DateTimeOffset requestedAt)
    {
        return Task.FromResult(emissionsForecast);
    }

    public Task<EmissionsForecast> GetCurrentCarbonIntensityForecastAsync(Location location)
    {
        return Task.FromResult(emissionsForecast);
    }
}