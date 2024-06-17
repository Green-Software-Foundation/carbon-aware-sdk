using CarbonAware.Interfaces;

namespace CarbonAware;

internal class NullForecastDataSource : IForecastDataSource
{
    public Task<EmissionsForecast> GetHistoricalCarbonIntensityForecastAsync(Location location, DateTimeOffset requestedAt)
    {
        throw new ArgumentException("ForecastDataSource is not configured");
    }

    public Task<EmissionsForecast> GetCurrentCarbonIntensityForecastAsync(Location location)
    {
        throw new ArgumentException("ForecastDataSource is not configured");
    }
}