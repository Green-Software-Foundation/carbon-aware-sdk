using System.Globalization;
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
    /// Gets or sets the location name.
    /// </summary>
    #nullable enable
    public string? Name { get; set; }
    #nullable disable

    /// <inheritdoc />
    public override string ToString()
    {
        return JsonSerializer.Serialize(this, SerializerOptions);
    }

    public String LatitudeAsCultureInvariantString()
    {
        return Convert.ToString(this.Latitude, CultureInfo.InvariantCulture) ?? "";
    }

    public String LongitudeAsCultureInvariantString()
    {
        return Convert.ToString(this.Longitude, CultureInfo.InvariantCulture) ?? "";
    }
}