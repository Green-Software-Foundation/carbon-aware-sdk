namespace CarbonAware.DataSources.Json.Configuration;

/// <summary>
/// A configuration class for holding Json Data config values.
/// </summary>
public class JsonDataConfiguration
{
    private const string BaseDir = "../data/data-files/";
    private const string DefaultDataFile = "test-data-azure-emissions.json";
    private string dataFileLocation = Path.Combine(BaseDir, DefaultDataFile);

    public const string Key = "JsonData";

    /// <summary>
    /// Json data file location
    /// </summary>
    public string? DataFileLocation
    {
        get => dataFileLocation;
        set
        {
            if (value is null)
                throw new ArgumentException("Value is null");
            dataFileLocation = Path.Combine(BaseDir, value);
        }
    }
}
