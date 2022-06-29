using System.Globalization;
using System.Reflection;
using CarbonAware.Model;
using CarbonAware.Interfaces;
using CarbonAware.Exceptions;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using CarbonAware.LocationSources.Azure;

namespace CarbonAware.LocationSources.Azure;

/// <summary>
/// Reprsents an azure location source.
/// </summary>
public class AzureLocationSource : ILocationSource
{
    public string Name => "Azure Location Source";

    public string Description => "Location source that knows how to get and work with Azure location information.";

    private readonly ILogger<AzureLocationSource> _logger;

    private IDictionary<string, NamedGeoposition>? namedGeopositions;

    private static readonly JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

    /// <summary>
    /// Creates a new instance of the <see cref="AzureLocationSource"/> class.
    /// </summary>
    /// <param name="logger">The logger for the LocationSource</param>
    public AzureLocationSource(ILogger<AzureLocationSource> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Location> ToGeopositionLocationAsync(Location location)
    {
        switch (location.LocationType)
        {
            case LocationType.Geoposition:
            {
                return await Task.FromResult(location);
            }
            case LocationType.CloudProvider: 
            {
                if (location.CloudProvider != null && location.CloudProvider != CloudProvider.Azure ) 
                {
                    throw new LocationConversionException($"Incorrect Cloud provider region. Expected Azure but found '{ location.CloudProvider }'");
                }
                var geoPositionLocation = await GetGeoPositionLocationOrThrowAsync(location);
                return geoPositionLocation;
            }
        }
        
        throw new LocationConversionException($"Location '{ location.CloudProvider }' cannot be converted to Geoposition. ");
    }

    private Task<Location> GetGeoPositionLocationOrThrowAsync(Location location)
    {
        LoadRegionsFromFileIfNotPresentAsync();

        var regionName = location.RegionName ?? string.Empty;
        if (! namedGeopositions!.ContainsKey(regionName))
        {
            throw new ArgumentException($"Unknown region: Region name '{regionName}' not found");
        }

        NamedGeoposition geopositionLocation = namedGeopositions![regionName];    
        if (!geopositionLocation.IsValidGeopositionLocation())  
        {
            throw new LocationConversionException($"Lat/long cannot be retrieved for region '{regionName}'");
        }
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug($"Converted Azure Location named '{location.RegionName}' to Geoposition Location at latitude '{location.Latitude}'" 
                                + "and logitude '{longitude}'.", location.RegionName, geopositionLocation.Latitude, geopositionLocation.Longitude);
        }
        Location geoPosistionLocation = new Location 
                {
                    LocationType = LocationType.Geoposition,
                    Latitude = Convert.ToDecimal(geopositionLocation.Latitude),
                    Longitude = Convert.ToDecimal(geopositionLocation.Longitude)
                };

        return Task.FromResult(geoPosistionLocation);        
    }

    protected virtual Task<Dictionary<String, NamedGeoposition>> LoadRegionsFromJsonAsync()
    {
        var data = ReadFromResource("CarbonAware.LocationSources.Azure.azure-regions.json");
        List<NamedGeoposition> regionList = JsonSerializer.Deserialize<List<NamedGeoposition>>(data, options) ?? new List<NamedGeoposition>();
        Dictionary<String, NamedGeoposition> regionGeopositionMapping = new Dictionary<String, NamedGeoposition>();
        foreach (NamedGeoposition region in regionList) 
        {
            regionGeopositionMapping.Add(region.RegionName, region);
        }

        return Task.FromResult(regionGeopositionMapping);
    }

    private async void LoadRegionsFromFileIfNotPresentAsync() {
        if (namedGeopositions == null || !namedGeopositions.Any())
        {
            namedGeopositions = await LoadRegionsFromJsonAsync();
        }
    }
    private string ReadFromResource(string key)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using Stream streamMetaData = assembly.GetManifestResourceStream(key) ?? throw new NullReferenceException("StreamMedataData is null");
        using StreamReader readerMetaData = new StreamReader(streamMetaData);
        return readerMetaData.ReadToEnd();
    }

}