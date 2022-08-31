namespace CarbonAware.DataSources.Json.Configuration;

/// <summary>
/// A configuration class for holding Json Data config values.
/// </summary>
public class JsonDataConfiguration
{
    public const string Key = "JsonData";

    /// <summary>
    /// Json data file location
    /// </summary>
    public string? DataFileLocation { get; set; }
}
