using System.Diagnostics;
using System.Reflection;
using CarbonAware.Interfaces;
using CarbonAware.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CarbonAware.DataSources.Json;

/// <summary>
/// Reprsents a JSON data source.
/// </summary>
public class JsonDataSource : ICarbonIntensityDataSource
{
    public string Name => "JsonDataSource";

    public string Description => "Example plugin to read data from a json for Carbon Aware SDK";

    public string Author => "Microsoft";

    public string Version => "0.0.1";

    private List<EmissionsData>? emissionsData;

    private readonly ILogger<JsonDataSource> _logger;


    /// <summary>
    /// Creates a new instance of the <see cref="JsonDataSource"/> class.
    /// </summary>
    /// <param name="logger">The logger for the datasource</param>
    public JsonDataSource(ILogger<JsonDataSource> logger )
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<IEnumerable<EmissionsData>> GetCarbonIntensityAsync(IEnumerable<Location> locations, DateTimeOffset startPeriod, DateTimeOffset endPeriod)
    {
        _logger.LogInformation("JSON data source getting carbon intensity for locations {locations} for period {startPeriod} to {endPeriod}.", locations, startPeriod, endPeriod);

        IEnumerable<EmissionsData>? emissionsData = GetSampleJson();
        if (emissionsData == null) {
            _logger.LogDebug("Emission data list is empty");
            return Task.FromResult(Enumerable.Empty<EmissionsData>());
        }
        _logger.LogDebug($"Total emission records retrieved {emissionsData.Count()}");
        IEnumerable<string> stringLocations = locations.Select(loc => loc.RegionName);

        var startDate = startPeriod.DateTime;
        var endDate = endPeriod.DateTime;
            
        emissionsData = FilterByLocation(emissionsData, stringLocations);
        emissionsData = FilterByDateRange(emissionsData, startDate, endDate);

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Found {count} total emissions data records for locations {stringLocations} for period {startPeriod} to {endPeriod}.", emissionsData.Count(), stringLocations, startPeriod, endPeriod);
        }

        return Task.FromResult(emissionsData);
        
    }

    private IEnumerable<EmissionsData> FilterByDateRange(IEnumerable<EmissionsData> data, DateTime startDate, object endDate)
    {
        DateTime end;
        DateTime.TryParse(endDate.ToString(), out end);
        data = data.Where(ed => ed.TimeBetween(startDate, end));  

        return data;
    }

    private IEnumerable<EmissionsData> FilterByLocation(IEnumerable<EmissionsData> data, IEnumerable<string> locations)
    {
        if (locations.Any()) 
        {
            data = data.Where(ed => locations.Contains(ed.Location));
        }

        return data;
    }

    private string ReadFromResource(string key)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using Stream streamMetaData = assembly.GetManifestResourceStream(key) ?? throw new NullReferenceException("StreamMedataData is null");
        using StreamReader readerMetaData = new StreamReader(streamMetaData);
        return readerMetaData.ReadToEnd();
    }

    protected virtual List<EmissionsData>? GetSampleJson()
    {
        var data = ReadFromResource("CarbonAware.DataSources.Json.test-data-azure-emissions.json");
        var jsonObject = JsonConvert.DeserializeObject<EmissionsJsonFile>(data);
        if(emissionsData == null || !emissionsData.Any()) {
           emissionsData = jsonObject.Emissions;
        }
        return emissionsData;
    }
}
