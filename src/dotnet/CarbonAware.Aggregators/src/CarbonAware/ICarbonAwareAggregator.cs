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
    /// Calculate Emissions Rating average.
    /// </summary>
    /// <param name="props">IDictionary with properties required by concrete classes.</param>
    /// <para>
    /// The following properties should be used:
    /// CarbonAwareConstants.Locations : A list of locations (i.e eastus, westus)
    /// CarbonAwareConstants.Start: <see cref="DateTime"/> representing the beginning of the sample.
    /// CarbonAwareConstants.End: <see cref="DateTime"/> representing the end of the sample.
    /// </para>
    /// <see cref="CarbonAwareConstants"/>
    /// <returns>Emissions Rating average.</returns>
    Task<double> CalcEmissionsAverageAsync(IDictionary props);

    /// <summary>
    /// Get forecasted emissions data.
    /// </summary>
    /// <param name="props">IDictionary with properties required by concrete classes.</param>
    /// <see cref="CarbonAwareConstants"/>
    /// <returns>List of current emissions forecasts by location.</returns>
    Task<IEnumerable<EmissionsForecast>> GetCurrentForecastDataAsync(IDictionary props);
}
