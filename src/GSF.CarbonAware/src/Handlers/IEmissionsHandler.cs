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
        /// <returns>Array of <see cref="EmissionsData"/> objects that contains the location, time and the rating in g/kWh</returns>
        Task<IEnumerable<EmissionsData>> GetEmissionsDataAsync(string location, DateTimeOffset? start = null, DateTimeOffset? end = null);

        /// <summary>
        /// Calculate the observed emission data by list of locations for a specified time period.
        /// </summary>
        /// <param name="locations">String array of named locations, must be non-empty (ex: ["eastus"])</param>
        /// <param name="start">[Optional] Start time for the data query. (ex: 2022-03-01T15:30:00Z)</param>
        /// <param name="end">[Optional] End time for the data query. (ex: 2022-03-01T18:30:00Z)</param>
        /// <returns>Array of <see cref="EmissionsData"/> objects that contains the location, time and the rating in g/kWh</returns>
        Task<IEnumerable<EmissionsData>> GetEmissionsDataAsync(string[] locations, DateTimeOffset? start = null, DateTimeOffset? end = null);

        /// <summary>
        /// Calculate the best emission data for the location and specified time period.
        /// </summary>
        /// <param name="location">String location (ex: "eastus")</param>
        /// <param name="start">[Optional] Start time for the data query. (ex: 2022-03-01T15:30:00Z)</param>
        /// <param name="end">[Optional] End time for the data query. (ex: 2022-03-01T18:30:00Z)</param>
        /// <returns>An array of <see cref="EmissionsData"/> objects that contains the location, and the best time and rating in g/kWh</returns>
        Task<IEnumerable<EmissionsData>> GetBestEmissionsDataAsync(string location, DateTimeOffset? start = null, DateTimeOffset? end = null);

        /// <summary>
        /// Calculate the best emission data by list of locations for a specified time period.
        /// </summary>
        /// <param name="locations">String array of named locations, must be non-empty (ex: ["eastus"])</param>
        /// <param name="start">[Optional] Start time for the data query. (ex: 2022-03-01T15:30:00Z)</param>
        /// <param name="end">[Optional] End time for the data query. (ex: 2022-03-01T18:30:00Z)</param>
        /// <returns>An array of <see cref="EmissionsData"/> objects that contains the location, time and the rating in g/kWh</returns>
        Task<IEnumerable<EmissionsData>> GetBestEmissionsDataAsync(string[] locations, DateTimeOffset? start = null, DateTimeOffset? end = null);

        /// </summary>
        /// <remarks> This function is useful for reporting the measured carbon intensity for a specific time period in a specific location. </remarks>
        /// <param name="location">The location name where workflow is run (ex: eastus)</param>
        /// <param name="start">The time at which the workflow we are measuring carbon intensity for started (ex: 2022-03-01T15:30:00Z)</param>
        /// <param name="end">The time at which the workflow we are measuring carbon intensity for ended (ex: 2022-03-01T18:30:00Z)</param>
        /// <returns>The average carbon intensity value.</returns>
        Task<double> GetAverageCarbonIntensityAsync(string location, DateTimeOffset start, DateTimeOffset end);
    }
}
