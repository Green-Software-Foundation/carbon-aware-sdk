using System.Text.Json;

namespace CarbonAware.Model;

/// <summary>
/// Represents a location.  Note that at least one value must be set.
/// </summary>
public class Location
{
    private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

    /// <summary>
    /// Gets or sets the latitude.
    /// </summary>
    public decimal? Latitude { get; set; }

    /// <summary>
    /// Gets or sets the longitude.
    /// </summary>
    public decimal? Longitude { get; set; }

    /// <summary>
    /// Gets or sets the type of location this location object represents.
    /// </summary>
    public LocationType LocationType { get; set; } = LocationType.NotProvided;

    /// <summary>
    /// Gets or sets the region name to use.  When set to GeoPosition, this value should be null, otherwise, it should be the region name for the specified provider.
    /// </summary>
    public string RegionName { get; set; }

    /// <inheritdoc />
    public override string ToString()
    {
        return JsonSerializer.Serialize(this, SerializerOptions);
    }
}