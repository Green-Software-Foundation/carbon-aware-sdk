namespace CarbonAware.Model;

/// <summary>
/// The provider type for the location.
/// </summary>
public enum LocationType
{
    // No type was provided.  Setting this value will cause an error when processed.
    NotProvided,

    // A geo position is provided.  Latitude and longitude are expected to be set.
    Geoposition,

    // Azure location.  Region is expected to be set to an Azure region name.
    Azure,

    // Aws location.  Region is expected to be set to an AWS region.
    AWS
}
