namespace CarbonAware.Model;

/// <summary>
/// The type for the location.
/// </summary>
public enum LocationType
{
    // No type was provided.  Setting this value will cause an error when processed.
    NotProvided,

    // A geo position is provided.  Latitude and longitude are expected to be set.
    Geoposition,

    // A cloud provider region location.  ProviderName and RegionName are expected to be set.
    CloudProvider
}

/// <summary>
/// The cloud provider type for the location.
/// </summary>
public enum CloudProvider
{
    // Azure location.  Region is expected to be set to an Azure region name.
    Azure,

    // Aws location.  Region is expected to be set to an AWS region.
    AWS
}