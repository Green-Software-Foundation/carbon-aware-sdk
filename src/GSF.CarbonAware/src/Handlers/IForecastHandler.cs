using GSF.CarbonAware.Models;

namespace GSF.CarbonAware.Handlers;

public interface IForecastHandler
{
    /// <summary>
    /// Retrieves the most recent forecasted data and calculates the optimal marginal carbon intensity window.
    /// </summary>
    /// <param name="locations">Array of locations where the workflow is run (ex: ["eastus", "westus"])</param>
    /// <param name="dataStartAt">Start time boundary of forecasted data points. Ignores current forecast data points before this time (ex: 2022-03-01T15:30:00Z)</param>
    /// <param name="dataEndAt">End time boundary of forecasted data points. Ignores current forecast data points after this time (ex: 2022-03-01T18:30:00Z)</param>
    /// <param name="windowSize">The estimated duration (in minutes) of the workload.</param>
    /// <returns>List of current emissions forecasts by location.</returns>
    Task<IEnumerable<EmissionsForecast>> GetCurrentAsync(string[] locations, DateTimeOffset? dataStartAt = null, DateTimeOffset? dataEndAt = null, int? windowSize = null);
}
