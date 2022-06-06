namespace CarbonAware.Model;

public class NamedGeoposition
{
    public string RegionName { get; set; } = string.Empty;
    public string Latitude { get; set; } = string.Empty;
    public string Longitude { get; set; } = string.Empty;

    /// <summary>
    /// Validates the object for the presence of Latitude and Longitude  
    /// </summary>
    /// <returns>Result true if Latitude and Longitude are not null or empty, false otherwise</returns>
    public bool IsValidGeopositionLocation() {
        return !String.IsNullOrEmpty(this.Latitude) && !String.IsNullOrEmpty(this.Latitude);
    }
}
