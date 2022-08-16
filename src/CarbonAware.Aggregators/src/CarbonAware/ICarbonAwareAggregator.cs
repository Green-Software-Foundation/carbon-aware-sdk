using CarbonAware.Model;
using System.Collections;

namespace CarbonAware.Aggregators.CarbonAware;

public interface ICarbonAwareAggregator : IAggregator
{
    /// <summary>
    /// Returns emissions data records.
    /// </summary>
    /// <param name="props">IDictionary with properties required by concrete classes</param>
    /// <returns>An IEnumerable instance with EmissionsData instances.</returns>
    Task<IEnumerable<EmissionsData>> GetEmissionsDataAsync(IDictionary props);

    /// <summary>
    /// Returns best emissions data record.
    /// </summary>
    /// <param name="parameters"><see cref="CarbonAwareParameters"> with properties required by concrete classes</param>
    /// <returns>The best EmissionsData object from the requested dataset or null if no dataset was found.</returns>
    Task<EmissionsData?> GetBestEmissionsDataAsync(CarbonAwareParameters parameters);

    /// <summary>
    /// Get current forecasted emissions data.
    /// </summary>
    /// <param name="props">IDictionary with properties required by concrete classes.</param>
    /// <see cref="CarbonAwareConstants"/>
    /// <returns>List of current emissions forecasts by location.</returns>
    Task<IEnumerable<EmissionsForecast>> GetCurrentForecastDataAsync(IDictionary props);

    /// <summary>
    /// Get the average carbon intensity.
    /// </summary>
    /// <param name="props">IDictionary with properties required by concrete classes</param>
    /// <returns>The the average carbon-intensity value by location for the time-interval.</returns>
    Task<double> CalculateAverageCarbonIntensityAsync(IDictionary props);

    /// Get forecasted emissions data.
    /// </summary>
    /// <param name="props">IDictionary with properties required by concrete classes.</param>
    /// <see cref="CarbonAwareConstants"/>
    /// <returns>Single emissions forecast for a given location generated at the requested time given start and end periods.</returns>
    Task<EmissionsForecast> GetForecastDataAsync(IDictionary props);
}
