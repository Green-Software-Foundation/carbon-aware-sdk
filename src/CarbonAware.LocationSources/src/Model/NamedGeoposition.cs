using CarbonAware.LocationSources.Exceptions;
using CarbonAware.Model;
using System.Globalization;

namespace CarbonAware.LocationSources.Model;

public class NamedGeoposition
{
    public string Name { get; set; } = string.Empty;
    public string Latitude { get; set; } = string.Empty;
    public string Longitude { get; set; } = string.Empty;

    /// <summary>
    /// Asserts that the object has at least a Name or both Latitude and Longitude.   
    /// </summary>
    /// <exception cref="LocationConversionException">Thrown if object does not have at least a Name or both Latitude and Longitude.</exception>
    public void AssertValid(string lookupName = "") {
        if ( string.IsNullOrWhiteSpace(Name) && 
            ( string.IsNullOrWhiteSpace(Latitude) || string.IsNullOrWhiteSpace(Longitude) ) )
        {
            throw new LocationConversionException($"Invalid Location: '{lookupName}' must have a Name or Latitude/Longitude pair.");
        }
    }

    public static explicit operator Location(NamedGeoposition namedGeoposition)
    {
        var location = new Location();
        location.Name = namedGeoposition.Name;
        if (!string.IsNullOrWhiteSpace(namedGeoposition.Latitude))
            location.Latitude = Convert.ToDecimal(namedGeoposition.Latitude, CultureInfo.InvariantCulture);

        if (!string.IsNullOrWhiteSpace(namedGeoposition.Longitude))
            location.Longitude = Convert.ToDecimal(namedGeoposition.Longitude, CultureInfo.InvariantCulture);

        return location;
    }
}
