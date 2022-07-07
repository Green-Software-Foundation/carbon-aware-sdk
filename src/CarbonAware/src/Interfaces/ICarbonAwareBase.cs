namespace CarbonAware.Interfaces;

public interface ICarbonAwareBase
{
    /// <summary>
    /// Returns the most recent prior emissions data record for the specified location if matching data is available.
    /// </summary>
    /// <param name="location">The name of the location to filter by.</param>
    /// <param name="time">The time to retrieve the most recent data for.</param>
    /// <param name="toTime">Optional: The end time of the time window.</param>
    /// <param name="durationMinutes">Optional: The duration over which to analyse emissions.</param>
    /// <returns>A single emissions data record for the location, and EmissionsData.None if no results are found.</returns>
    List<EmissionsData> GetEmissionsDataForLocationByTime(string location, DateTime time, DateTime? toTime = null, int durationMinutes = 0);

    /// <summary>
    /// Returns the most recent prior emissions data record for a list of specified locations if matching data is available.
    /// </summary>
    /// <param name="locations">The name of the locations to filter by.</param>
    /// <param name="time">The time to retrieve the most recent data for.</param>
    /// <param name="toTime">Optional: The end time of the time window.</param>
    /// /// <param name="durationMinutes">Optional: The duration over which to analyse emissions.</param>
    /// <returns>A List&lt;EmissionsData&gt; for each emissions data record for each location available.  
    /// If no records are found, returns an empty List.</returns>
    List<EmissionsData> GetEmissionsDataForLocationsByTime(List<string> locations, DateTime time, DateTime? toTime = null, int durationMinutes = 0);

    /// <summary>
    /// Returns the lowest emissions record for a list of specified locations at a specific time.
    /// </summary>
    /// <param name="location">The name of the locations to filter by.</param>
    /// <param name="time">The time to retrieve the most recent data for.  The "fromTime" if "toTime" is provided.</param>
    /// <param name="toTime">Optional: The end time of the time window.</param>
    /// /// <param name="durationMinutes">Optional: The duration over which to analyse emissions.</param>
    /// <returns>>The matching emissions data record for the locations based on the "best" emissions 
    /// i.e. in thie case, the lowest.  Returns EmissionsData.  May be mutliple emissions results and times if
    /// the emissions are equal.</returns>
    List<EmissionsData> GetBestEmissionsDataForLocationsByTime(List<string> locations, DateTime time, DateTime? toTime = null, int durationMinutes = 0);
}
