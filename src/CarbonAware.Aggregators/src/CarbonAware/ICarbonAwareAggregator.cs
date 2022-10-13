using CarbonAware.Model;
using System.Collections;

namespace CarbonAware.Aggregators.CarbonAware;

public interface ICarbonAwareAggregator : IAggregator
{
    /// <summary>
    /// Returns emissions data records.
    /// </summary>
    /// <param name="parameters"><see cref="CarbonAwareParameters"> with properties required by concrete classes</param>
    /// <returns>An IEnumerable instance with EmissionsData instances.</returns>
    Task<IEnumerable<EmissionsData>> GetEmissionsDataAsync(CarbonAwareParameters parameters);

    /// <summary>
    /// Returns best emissions data record(s).
    /// </summary>
    /// <remarks>
    /// Multiple "best" records are returned in case
    /// of a tie, as callers of the SDK might have different business logic (i.e. if there's a tie
    /// in two locations, then one might be a lot more preferable due to proximity)
    /// </remarks>
    /// <param name="parameters"><see cref="CarbonAwareParameters"> with properties required by concrete classes</param>
    /// <returns>The best EmissionsData object(s) from the requested dataset or an empty array if no dataset was found.</returns>
    Task<IEnumerable<EmissionsData>> GetBestEmissionsDataAsync(CarbonAwareParameters parameters);

    /// <summary>
    /// Get current forecasted emissions data.
    /// </summary>
    /// <param name="parameters"><see cref="CarbonAwareParameters"> with properties required by concrete classes</param>
    /// <returns>List of current emissions forecasts by location.</returns>
    Task<IEnumerable<EmissionsForecast>> GetCurrentForecastDataAsync(CarbonAwareParameters parameters);

    /// <summary>
    /// Get the average carbon intensity.
    /// </summary>
    /// <param name="parameters"><see cref="CarbonAwareParameters"> with properties required by concrete classes</param>
    /// <returns>The the average carbon-intensity value by location for the time-interval.</returns>
    Task<double> CalculateAverageCarbonIntensityAsync(CarbonAwareParameters parameters);

    /// <summary>
    /// Get forecasted emissions data.
    /// </summary>
    /// <param name="parameters"><see cref="CarbonAwareParameters"> with properties required by concrete classes</param>
    /// <returns>Single emissions forecast for a given location generated at the requested time given start and end periods.</returns>
    Task<EmissionsForecast> GetForecastDataAsync(CarbonAwareParameters parameters);
}
