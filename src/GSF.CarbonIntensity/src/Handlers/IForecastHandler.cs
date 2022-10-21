using GSF.CarbonIntensity.Models;

namespace GSF.CarbonIntensity.Handlers;

public interface IForecastHandler
{
    /// <summary>
    ///   Retrieves the most recent forecasted data and calculates the optimal marginal carbon intensity window.
    /// </summary>
    /// <param name="location">The location name where workflow is run (ex: eastus)</param>
    /// <param name="start">Start time boundary of forecasted data points. Ignores current forecast data points before this time (ex: 2022-03-01T15:30:00Z)</param>
    /// <param name="end">End time boundary of forecasted data points. Ignores current forecast data points after this time (ex: 2022-03-01T18:30:00Z)</param>
    /// <param name="duration">The estimated duration (in minutes) of the workload.</param>
    Task<EmissionsForecast> GetCurrentAsync(string location, DateTimeOffset? start, DateTimeOffset? end, int? duration);
}
