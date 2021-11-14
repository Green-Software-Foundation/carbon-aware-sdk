using System;
using System.Collections.Generic;

namespace CarbonAware
{
    public interface ICarbonAwareBase
    {
        /// <summary>
        /// Returns the most recent prior emissions data record for the specified location.
        /// </summary>
        /// <param name="location">The name of the location to filter by.</param>
        /// <param name="time">The time to retrieve the most recent data for.</param>
        /// <returns>A single emissions data record for the location, and EmissionsData.None if no results are found.</returns>
        EmissionsData GetEmissionsDataForLocationByTime(string location, DateTime time);

        /// <summary>
        /// Returns the most recent prior emissions data record for a list of specified locations.
        /// </summary>
        /// <param name="locations">The name of the locations to filter by.</param>
        /// <param name="time">The time to retrieve the most recent data for.</param>
        /// <returns>A List&lt;EmissionsData&gt; for each emissions data record for each location available.  
        /// If no records are found, returns an empty List.</returns>
        List<EmissionsData> GetEmissionsDataForLocationsByTime(List<string> locations, DateTime time);
        EmissionsData GetEmissionsDataForLocationByTimeWindow(string location, TimeWindow timeWindow);
        List<EmissionsData> GetEmissionsDataForLocationsByTimeWindow(List<string> locations, TimeWindow timeWindow);

        /// <summary>
        /// Returns the lowest emissions record for a list of specified locations at a specific time.
        /// </summary>
        /// <param name="location">The name of the locations to filter by.</param>
        /// <param name="time">The time to retrieve the most recent data for.</param>
        /// <returns>>A single emissions data record for the location based on the "best" emissions 
        /// i.e. in thie case, the lowest.  Returns EmissionsData.None if no results are found.</returns>
        EmissionsData GetBestEmissionsDataForLocationsByTime(List<string> locations, DateTime time);
    }
}
