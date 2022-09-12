using System.Reflection;
using System.Text.RegularExpressions;

namespace CarbonAware.DataSources.Json.Configuration;

/// <summary>
/// A configuration class for holding Json Data config values.
/// </summary>
public class JsonDataSourceConfiguration
{
    private const string BaseDirectory = "data-sources/json";
    private const string DefaultDataFile = "test-data-azure-emissions.json";
    private const string DirectoryRegExPattern = @"^[-/a-zA-Z_\d ]*$";
    private string assemblyDirectory;
    private string? dataFileLocation;

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
                throw new ArgumentException($"File path '{value}' contains not supported characters.");
            }
            dataFileLocation = Path.Combine(assemblyDirectory, BaseDirectory, value);
        }
    }

    public const string Key = "JsonDataSourceConfiguration";

    public JsonDataSourceConfiguration()
    {
        var assemblyPath = Assembly.GetExecutingAssembly().Location;
        assemblyDirectory = Path.GetDirectoryName(assemblyPath)!;
        DataFileLocation = DefaultDataFile;
    }

    private static bool IsValidDirPath(string fileName)
    {
        if (String.IsNullOrEmpty(fileName))
        {
            return false;
        }
        var dirName = Path.GetDirectoryName(fileName);
        if (dirName is null)
        {
            return false;
        }
        var match = Regex.Match(dirName, DirectoryRegExPattern);
        return match.Success;
    }
}
