using GSF.CarbonAware.Models;

namespace GSF.CarbonAware.Handlers;

public interface ILocationHandler
{
    /// <summary>
    /// Retrieves all the locations available.
    /// </summary>
    /// <returns>A Dictionary with <see cref="Location"/> information that can be used to retrieve <see cref="EmissionsData"/> or <see cref="EmissionsForecast"/> instances.</returns>
    Task<IDictionary<string, Location>> GetLocationsAsync();
}
