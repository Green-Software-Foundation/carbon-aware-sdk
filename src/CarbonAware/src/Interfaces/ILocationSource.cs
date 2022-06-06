namespace CarbonAware.Interfaces;

/// <summary>
/// Represents a location source for Location type.
/// </summary>
public interface ILocationSource
{
    string Name { get; }
    string Description { get; }

    /// <summary>
    /// Converts given Location to a new Location with type Geoposition
    /// </summary>
    /// <param name="location">The location to be converted. </param>
    /// <returns>New location representing Geoposition information.</returns>
    public Task<Location> ToGeopositionLocationAsync(Location location);
}
