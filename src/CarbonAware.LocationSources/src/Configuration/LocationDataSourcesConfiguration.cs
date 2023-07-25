namespace CarbonAware.LocationSources.Configuration;

/// <summary>
/// A configuration class for holding Location Data config values.
/// </summary>
internal class LocationDataSourcesConfiguration
{
    public const string Key = "LocationDataSourcesConfiguration";

    public List<LocationSourceFile> LocationSourceFiles { get; set; } = new();
}
