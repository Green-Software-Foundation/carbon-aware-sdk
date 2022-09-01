using System.Reflection;
using System.Text.RegularExpressions;

namespace CarbonAware.DataSources.Json.Configuration;

/// <summary>
/// A configuration class for holding Json Data config values.
/// </summary>
public class JsonDataConfiguration
{
    private const string BaseDir = "data-files";
    private const string DefaultDataFile = "test-data-azure-emissions.json";
    private const string RegExDir = "[^a-zA-Z0-9_]+";
    private string? dataFileLocation;
    private string assemblyDirectory;

    public const string Key = "JsonData";


    public JsonDataConfiguration()
    {
        var assemblyPath = Assembly.GetExecutingAssembly().Location;
        assemblyDirectory = Path.GetDirectoryName(assemblyPath)!;
        DataFileLocation = DefaultDataFile;
    }

    /// <summary>
    /// Json data file location
    /// </summary>
    public string DataFileLocation
    {
        get => dataFileLocation!;
        set
        {
            if (!IsValidDirPath(value))
            {
                throw new ArgumentException("File path {value} contains invalid characters.", value);
            }
            dataFileLocation = Path.Combine(assemblyDirectory, BaseDir, value);
        }
    }

    private static bool IsValidDirPath(string fileName)
    {
        var dirName = Path.GetDirectoryName(fileName);
        var rgex = new Regex(RegExDir);
        var matched = rgex.Matches(dirName!);
        return matched.Count == 0;
    }
}
