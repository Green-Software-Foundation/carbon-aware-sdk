using CarbonAware.Interfaces;
using CarbonAware.Model;
using CarbonAware.LocationSources.Configuration;
using CarbonAware.LocationSources.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Reflection;
using System.Text.Json;

namespace CarbonAware.LocationSources;

/// <summary>
/// Represents a location source.
/// </summary>
public class LocationSource : ILocationSource
{
    private readonly ILogger<LocationSource> _logger;

    private IDictionary<string, Location> _allLocations;

    private IOptionsMonitor<LocationDataSourcesConfiguration> _configurationMonitor { get; }

    private LocationDataSourcesConfiguration _configuration => _configurationMonitor.CurrentValue;


    /// <summary>
    /// Creates a new instance of the <see cref="LocationSource"/> class.
    /// </summary>
    /// <param name="logger">The logger for the LocationSource</param>
    public LocationSource(ILogger<LocationSource> logger, IOptionsMonitor<LocationDataSourcesConfiguration> monitor)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configurationMonitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
        _allLocations = new Dictionary<string, Location>(StringComparer.InvariantCultureIgnoreCase);
    }

    public async Task<Location> ToGeopositionLocationAsync(Location location)
    {
        await LoadLocationFromFileIfNotPresentAsync();

        var name = location.Name ?? string.Empty;
        if (!_allLocations.ContainsKey(name))
        {
            throw new ArgumentException($"Unknown Location: '{name}' not found");
        }
        return _allLocations[name];
    }

    /// <inheritdoc />
    public async Task<IDictionary<string, Location>> GetGeopositionLocationsAsync()
    {
        await LoadLocationFromFileIfNotPresentAsync();
        return _allLocations;
    }

    private async Task LoadLocationJsonFileAsync()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var keyCounter = new Dictionary<string, int>(); // used to keep track of key dups

        var sourceFiles = !_configuration.LocationSourceFiles.Any() ? DiscoverFiles() : _configuration.LocationSourceFiles;
        foreach (var source in sourceFiles)
        {
            using Stream stream = GetStreamFromFileLocation(source);
            var namedGeoMap = await JsonSerializer.DeserializeAsync<Dictionary<string, NamedGeoposition>>(stream, options);
            foreach (var locationKey in namedGeoMap!.Keys) 
            {
                var geoInstance = namedGeoMap[locationKey];
                geoInstance.AssertValid();
                var key = BuildKey(source, locationKey);
                AddToLocationMap(key, geoInstance, source.DataFileLocation, keyCounter);
            }
        }
    }

    private String BuildKey(LocationSourceFile locationData, string locationName)
    {
        return $"{locationData.Prefix}{locationData.Delimiter}{locationName}";
    }

    private async Task LoadLocationFromFileIfNotPresentAsync()
    {
        if (!_allLocations.Any())
        {
            await LoadLocationJsonFileAsync();
        }
    }

    private Stream GetStreamFromFileLocation(LocationSourceFile locationData)
    {
        _logger.LogInformation($"Reading Location data source from {locationData.DataFileLocation}");
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug($"Location data source Prefix '{locationData.Prefix}' and Delimiter '{locationData.Delimiter}'");
        }
        return File.OpenRead(locationData.DataFileLocation!);
    }

    private IEnumerable<LocationSourceFile> DiscoverFiles()
    {
        var assemblyPath = Assembly.GetExecutingAssembly().Location;
        var assemblyDirectory = Path.GetDirectoryName(assemblyPath)!;

        var pathCombined = Path.Combine(assemblyDirectory, LocationSourceFile.BaseDirectory);
        var files = Directory.GetFiles(pathCombined, "*.json", SearchOption.AllDirectories).OrderBy(f => f);
        if (files is null)
        {
            _logger.LogWarning($"No location files found under {pathCombined}");
            return Array.Empty<LocationSourceFile>();
        }
        _logger.LogInformation($"{files.Count()} files discovered");
        return files.Select(x => x.Substring(pathCombined.Length + 1)).Select(n => new LocationSourceFile { DataFileLocation = n });
    }

    private void AddToLocationMap(string key, NamedGeoposition data, string sourceFile, Dictionary<string, int> keyCounter)
    {
        var loc = (Location) data;
        
        if (_allLocations.TryAdd(key, loc))
        {
            keyCounter.Add(key, 0);
            return;
        }
        // Generate new key using keyCounter counter
        _logger.LogWarning("Location key {key} from {sourceFile} already exists. Creating new key.", key, sourceFile);
        var counter = keyCounter[key];
        counter++;
        var newKey = $"{key}_{counter}";
        _allLocations.Add(newKey, loc);
        keyCounter[key] = counter;
        _logger.LogWarning("New key {newKey} generated from {key}", newKey, key);
    }
}
