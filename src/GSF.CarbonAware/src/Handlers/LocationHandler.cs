using GSF.CarbonAware.Models;
using CarbonAware.Interfaces;

namespace GSF.CarbonAware.Handlers;

internal sealed class LocationHandler : ILocationHandler
{
    private readonly ILocationSource _locationSource;

    private IDictionary<string, Location> _allLocations;

    public LocationHandler(ILocationSource source)
    {
        _locationSource = source;
        _allLocations = new Dictionary<string, Location>();
    }
 
    /// <inheritdoc />
    public async Task<IDictionary<string, Location>> GetLocationsAsync()
    {
        if (!_allLocations.Any())
        {
            foreach (KeyValuePair<string, global::CarbonAware.Model.Location> elem in await _locationSource.GetGeopositionLocationsAsync())
            {
                _allLocations.Add(elem.Key, (Location) elem.Value);
            }
        }
       return _allLocations;
    }
}
