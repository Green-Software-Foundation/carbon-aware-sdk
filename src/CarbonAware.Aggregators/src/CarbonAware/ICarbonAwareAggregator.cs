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
    /// <param name="props">IDictionary with properties required by concrete classes</param>
    /// <returns>The best EmissionsData object from the requested dataset.</returns>
    Task<EmissionsData> GetBestEmissionsDataAsync(IDictionary props);


    /// <summary>
    /// Get forecasted emissions data.
    /// </summary>
    /// <param name="props">IDictionary with properties required by concrete classes.</param>
    /// <see cref="CarbonAwareConstants"/>
    /// <returns>List of current emissions forecasts by location.</returns>
    Task<IEnumerable<EmissionsForecast>> GetCurrentForecastDataAsync(IDictionary props);
}
