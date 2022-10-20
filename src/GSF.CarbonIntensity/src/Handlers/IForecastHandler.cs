using GSF.CarbonIntensity.Models;

namespace GSF.CarbonIntensity.Handlers;

// TODO document methods
public interface IForecastHandler
{
    Task<EmissionsForecast> GetCurrent(string location, DateTimeOffset start, DateTimeOffset end, int duration);
}
