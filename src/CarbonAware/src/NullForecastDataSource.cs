using CarbonAware.Interfaces;

namespace CarbonAware;

public class NullForecastDataSource : IForecastDataSource
{
    public Task<EmissionsForecast> GetCarbonIntensityForecastAsync(Location location, DateTimeOffset requestedAt)
    {
        throw new ArgumentException("ForecastDataSource is not configured");
    }

    public Task<EmissionsForecast> GetCurrentCarbonIntensityForecastAsync(Location location)
    {
        throw new ArgumentException("ForecastDataSource is not configured");
    }
}