namespace CarbonAware.Interfaces;

public interface IEmissionsDataSource
{
    /// <summary>
    /// Gets the carbon intensity for multiple locations for a given start and end time
    /// </summary>
    /// <param name="locations">The locations that should be used for getting emissions data.</param>
    /// <param name="periodStartTime">The start time of the period.</param>
    /// <param name="periodEndTime">The end time of the period.</param>
    /// <returns>A list of emissions data for the given time period.</returns>
    public Task<IEnumerable<EmissionsData>> GetCarbonIntensityAsync(IEnumerable<Location> locations, DateTimeOffset periodStartTime, DateTimeOffset periodEndTime);

    /// <summary>
    /// Gets the carbon intensity for a single location for a given start and end time
    /// </summary>
    /// <param name="location">The location that should be used for getting emissions data.</param>
    /// <param name="periodStartTime">The start time of the period.</param>
    /// <param name="periodEndTime">The end time of the period.</param>
    /// <returns>A list of emissions data for the given time period.</returns>
    public Task<IEnumerable<EmissionsData>> GetCarbonIntensityAsync(Location location, DateTimeOffset periodStartTime, DateTimeOffset periodEndTime);
}
