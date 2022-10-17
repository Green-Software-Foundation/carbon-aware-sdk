using GSF.CarbonIntensity.Model;

namespace GSF.CarbonIntensity.Handlers;

// TODO document methods
public interface IForecastHandler
{
    Task<ForecastData> GetCurrent();
}
