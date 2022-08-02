namespace CarbonAware.Interfaces;

/// <summary>
/// Represents a data source for carbon intensity.
/// </summary>
public interface ICarbonIntensityDataSource
{
    string Name { get; }
    string Description { get; }
    string Author { get; }
    string Version { get; }

    /// <summary>
    /// Minimum sampling window for data, in minutes
    /// </summary>
    double MinSamplingWindow { get; }

    /// <summary>
    /// Gets the carbon intensity for a location and start and end time
    /// </summary>
    /// <param name="locations">The locations that should be used for getting emissions data.</param>
    /// <param name="periodStartTime">The start time of the period.</param>
    /// <param name="periodEndTime">The end time of the period.</param>
    /// <returns>A list of emissions data for the given time period.</returns>
    public Task<IEnumerable<EmissionsData>> GetCarbonIntensityAsync(IEnumerable<Location> locations, DateTimeOffset periodStartTime, DateTimeOffset periodEndTime);

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
