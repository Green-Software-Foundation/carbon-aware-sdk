using CarbonAware.Exceptions;
using CarbonAware.Interfaces;
using CarbonAware.Model;
using CarbonAware.LocationSources.Configuration;
using CarbonAware.LocationSources.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Reflection;
using System.Globalization;
using System.Text.Json;

namespace CarbonAware.LocationSources;

/// <summary>
/// Represents a location source.
/// </summary>
public class LocationSource : ILocationSource
{
    private readonly ILogger<LocationSource> _logger;

    private IDictionary<string, NamedGeoposition> _namedGeopositions;

    private static readonly JsonSerializerOptions _options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

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
        _namedGeopositions = new Dictionary<string, NamedGeoposition>(StringComparer.InvariantCultureIgnoreCase);
    }

    public async Task<Location> ToGeopositionLocationAsync(Location location)
    {
        await LoadLocationFromFileIfNotPresentAsync();

        var name = location.Name ?? string.Empty;
        if (!_namedGeopositions.ContainsKey(name))
        {
            throw new ArgumentException($"Unknown Location: '{name}' not found");
        }

        var geopositionLocation = _namedGeopositions[name];    
        geopositionLocation.AssertValid(name);

        return (Location) geopositionLocation;
    }

    private async Task LoadLocationJsonFileAsync()
    {
        var sourceFiles = !_configuration.LocationSourceFiles.Any() ? DiscoverFiles() : _configuration.LocationSourceFiles;
        foreach (var source in sourceFiles)
        {
            using Stream stream = GetStreamFromFileLocation(source);
            var locationMapping = await JsonSerializer.DeserializeAsync<Dictionary<string, NamedGeoposition>>(stream, _options) ?? new Dictionary<string, NamedGeoposition>();
            foreach (var locationName in locationMapping.Keys) 
            {
                var key = BuildKey(source, locationName);
                _namedGeopositions[key] = locationMapping[locationName];
            }
        }
    }

    private String BuildKey(LocationSourceFile locationData, string locationName)
    {
        return $"{locationData.Prefix}{locationData.Delimiter}{locationName}";
    }

    private async Task LoadLocationFromFileIfNotPresentAsync()
    {
        if (!_namedGeopositions.Any())
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
        var files = Directory.GetFiles(pathCombined, "*.json", SearchOption.AllDirectories);
        if (files is null)
        {
            _logger.LogWarning($"No location files found under {pathCombined}");
            return Array.Empty<LocationSourceFile>();
        }
        _logger.LogInformation($"{files.Length} files discovered");
        return files.Select(x => x.Substring(pathCombined.Length + 1)).Select(n => new LocationSourceFile { DataFileLocation = n });
    }
}
