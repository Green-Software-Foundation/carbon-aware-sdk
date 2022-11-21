using GSF.CarbonAware.Models;

namespace GSF.CarbonAware.Handlers
{
    public interface IEmissionsHandler
    {
        /// <summary>
        /// Calculate the observed emission data for a location and a specified time period.
        /// </summary>
        /// <param name="location">String location (ex: "eastus")</param>
        /// <param name="start">[Optional] Start time for the data query. (ex: 2022-03-01T15:30:00Z)</param>
        /// <param name="end">[Optional] End time for the data query. (ex: 2022-03-01T18:30:00Z)</param>
        /// <returns>Array of EmissionsData objects that contains the location, time and the rating in g/kWh</returns>
        Task<IEnumerable<EmissionsData>> GetEmissionsDataAsync(string location, DateTimeOffset? start = null, DateTimeOffset? end = null);

        /// <summary>
        /// Calculate the observed emission data by list of locations for a specified time period.
        /// </summary>
        /// <param name="locations">String array of named locations, must be non-empty (ex: ["eastus"])</param>
        /// <param name="start">[Optional] Start time for the data query. (ex: 2022-03-01T15:30:00Z)</param>
        /// <param name="end">[Optional] End time for the data query. (ex: 2022-03-01T18:30:00Z)</param>
        /// <returns>Array of EmissionsData objects that contains the location, time and the rating in g/kWh</returns>
        Task<IEnumerable<EmissionsData>> GetEmissionsDataAsync(string[] locations, DateTimeOffset? start = null, DateTimeOffset? end = null);

        /// <summary>
        /// Calculate the best emission data by list of locations for a specified time period.
        /// </summary>
        /// <param name="locations">String array of named locations, must be non-empty (ex: ["eastus"])</param>
        /// <param name="start">[Optional] Start time for the data query. (ex: 2022-03-01T15:30:00Z)</param>
        /// <param name="end">[Optional] End time for the data query. (ex: 2022-03-01T18:30:00Z)</param>
        /// <returns>An array of EmissionsData objects that contains the location, time and the rating in g/kWh</returns>
        Task<IEnumerable<EmissionsData>> GetBestEmissionsDataAsync(string[] locations, DateTimeOffset? start = null, DateTimeOffset? end = null);

        /// <summary>
        /// Given a location, start, and end time, retrieves the measured carbon intensity data for the given location between the time boundaries and calculates the average carbon intensity during that period. 
        /// </summary>
        /// <remarks> This function is useful for reporting the measured carbon intensity for a specific time period in a specific location. </remarks>
        /// <param name="location">The location name where workflow is run (ex: eastus)</param>
        /// <param name="requestedTimeRange"> A TimeRange object that contains a "startTime" and "endTime" for which to calculate average marginal carbon intensity. </param>
        /// <returns>A CarbonIntensity object with a single carbon intensity data point containing the average marginal carbon intensity for the requested time range.</returns>
        Task<CarbonIntensity> GetAverageCarbonIntensityAsync(string location, TimeRange requestedTimeRange);

        /// <summary>
        /// Given a location and an array of request TimeRange objects, for each time boundary, retrieves the measured carbon intensity data and calculates the average carbon intensity during that period. 
        /// </summary>
        /// <param name="location">The location name where workflow is run (ex: eastus)</param>
        /// <param name="requestedTimeRanges"> Array of TimeRange inputs where each contains "startDate" and "endDate" for which to calculate average marginal carbon intensity. </param>
        /// <returns>A CarbonIntensity object with an array of carbon intensity data points, each containing the average marignal carbon intensity for each requested time range.</returns>
        public Task<CarbonIntensity> GetAverageCarbonIntensityAsync(string location, TimeRange[] requestedTimeRanges);
    }
}
