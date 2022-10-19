namespace CarbonAware.Interfaces;
public interface IForecastDataSource
{
    /// <summary>
    /// Gets the current forecasted carbon intensity for a location
    /// </summary>
    /// <param name="location">The location that should be used for getting the forecast.</param>
    /// <returns>A forecasted emissions object for the given location.</returns>
    public Task<EmissionsForecast> GetCurrentCarbonIntensityForecastAsync(Location location);

    /// <summary>
    /// Gets the forecasted carbon intensity for a location generated at a requested time
    /// <param name="location">The location that should be used for getting the forecast.</param>
    /// <param name="requestedAt">The historical time used to fetch the most recent forecast generated as of that time.</param>
    /// <returns>A forecasted emissions object for the given location generated at the given time.</returns>
    public Task<EmissionsForecast> GetCarbonIntensityForecastAsync(Location location, DateTimeOffset requestedAt);
}