using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.Model;

namespace CarbonAware.Aggregators.Forecast;

[Obsolete]
public interface IForecastAggregator
{
    /// <summary>
    /// Get current forecasted emissions data.
    /// </summary>
    /// <param name="parameters"><see cref="CarbonAwareParameters"> with properties required by concrete classes</param>
    /// <returns>List of current emissions forecasts by location.</returns>
    Task<IEnumerable<EmissionsForecast>> GetCurrentForecastDataAsync(CarbonAwareParameters parameters);

    /// <summary>
    /// Get forecasted emissions data.
    /// </summary>
    /// <param name="parameters"><see cref="CarbonAwareParameters"> with properties required by concrete classes</param>
    /// <returns>Single emissions forecast for a given location generated at the requested time given start and end periods.</returns>
    Task<EmissionsForecast> GetForecastDataAsync(CarbonAwareParameters parameters);
}
