using CarbonAware.Aggregators.CarbonAware;
using GSF.CarbonIntensity.Models;

namespace GSF.CarbonIntensity.Handlers;

// TODO document methods
public interface IForecastHandler
{
    Task<ForecastData> GetCurrent(CarbonAwareParameters parameters);
}
