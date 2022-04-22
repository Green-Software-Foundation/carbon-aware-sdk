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
    /// Gets the carbon intensity for a given SciScoreCalculation.
    /// </summary>
    /// <param name="locations">The locations that should be used for getting emissions data.</param>
    /// <param name="startPeriod">The start period.</param>
    /// <param name="endPeriod">The end period.</param>
    /// <returns>A list of emissions data for the given time period.</returns>
    public Task<IEnumerable<EmissionsData>> GetCarbonIntensityAsync(IEnumerable<Location> locations, DateTimeOffset startPeriod, DateTimeOffset endPeriod);
}