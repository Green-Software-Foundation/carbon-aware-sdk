using CarbonAware.DataSources.Json.Configuration;
using CarbonAware.Exceptions;
using CarbonAware.Interfaces;
using CarbonAware.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace CarbonAware.DataSources.Json;

/// <summary>
/// Represents a JSON data source.
/// </summary>
public class JsonDataSource : IEmissionsDataSource, IForecastDataSource
{
    public string Name => "JsonDataSource";

    public string Description => "Example plugin to read data from a json for Carbon Aware SDK";

    public string Author => "Microsoft";

    public string Version => "0.0.1";

    public double MinSamplingWindow => 1440;  // 24 hrs

    private List<EmissionsData>? _emissionsData;

    private List<EmissionsForecast>? _emissionsForecasts;

    private readonly ILogger<JsonDataSource> _logger;

    private IOptionsMonitor<JsonDataSourceConfiguration> _configurationMonitor { get; }

    private JsonDataSourceConfiguration _configuration => _configurationMonitor.CurrentValue;



    /// <summary>
    /// Creates a new instance of the <see cref="JsonDataSource"/> class.
    /// </summary>
    /// <param name="logger">The logger for the datasource</param>
    public JsonDataSource(ILogger<JsonDataSource> logger, IOptionsMonitor<JsonDataSourceConfiguration> monitor)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configurationMonitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
    }

    /// <inheritdoc />
    public Task<IEnumerable<EmissionsData>> GetCarbonIntensityAsync(Location location, DateTimeOffset periodStartTime, DateTimeOffset periodEndTime)
    {
        return GetCarbonIntensityAsync(new List<Location>() { location }, periodStartTime, periodEndTime);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmissionsData>> GetCarbonIntensityAsync(IEnumerable<Location> locations, DateTimeOffset periodStartTime, DateTimeOffset periodEndTime)
    {
        _logger.LogInformation("JSON data source getting carbon intensity for locations {locations} for period {periodStartTime} to {periodEndTime}.", locations, periodStartTime, periodEndTime);

        IEnumerable<EmissionsData>? emissionsData = await GetJsonDataAsync();
        if (emissionsData == null || !emissionsData.Any())
        {
            _logger.LogDebug("Emission data list is empty");
            return Array.Empty<EmissionsData>();
        }
        _logger.LogDebug($"Total emission records retrieved {emissionsData.Count()}");
        var stringLocations = locations.Select(loc => loc.Name);
            
        emissionsData = FilterByLocation(emissionsData, stringLocations);
        emissionsData = FilterByDateRange(emissionsData, periodStartTime, periodEndTime);

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Found {count} total emissions data records for locations {stringLocations} for period {periodStartTime} to {periodEndTime}.", emissionsData.Count(), stringLocations, periodStartTime, periodEndTime);
        }

        return emissionsData;
    }

    public async Task<EmissionsForecast> GetCurrentCarbonIntensityForecastAsync(Location location)
    {
        _logger.LogInformation("JSON data source getting current carbon intensity forecast for location {locations}", location);

        IEnumerable<EmissionsForecast>? emissionsForecasts = await GetJsonEmissionsForecastsAsync();
        if (emissionsForecasts is null || !emissionsForecasts.Any())
        {
            throw new CarbonAwareException("EmissionsForecasts data is empty in JSON data source file.");
        }

        var emissionsForecast = GetEmissionsForecastByLocation(emissionsForecasts, location);

        if (emissionsForecast is null) 
        {
            throw new CarbonAwareException($"JSON data source file doesn't contain forecast data for location: {location}.");
        }

        return emissionsForecast;
    }

    public Task<EmissionsForecast> GetCarbonIntensityForecastAsync(Location location, DateTimeOffset requestedAt)
    {
        // Similarly to ElectricityMaps Data Source this Data Source doesn't implement RequestedAt since JSON source is static.
        throw new NotImplementedException();
    }

    private IEnumerable<EmissionsData> FilterByDateRange(IEnumerable<EmissionsData> data, DateTimeOffset startTime, DateTimeOffset endTime)
    {
        var (newStartTime, newEndTime) = IntervalHelper.ExtendTimeByWindow(startTime, endTime, MinSamplingWindow);
        var windowData = data.Where(ed => ed.TimeBetween(newStartTime, newEndTime));
        var filteredData = IntervalHelper.FilterByDuration(windowData, startTime, endTime);

        if (!filteredData.Any())
        {
            _logger.LogInformation($"Not enough data with {MinSamplingWindow} window");
        }
        return filteredData;
    }

    private IEnumerable<EmissionsData> FilterByLocation(IEnumerable<EmissionsData> data, IEnumerable<string?> locations)
    {
        if (locations.Any())
        {
            data = data.Where(ed => locations.Contains(ed.Location));
        }

        return data;
    }

    private EmissionsForecast? GetEmissionsForecastByLocation(IEnumerable<EmissionsForecast> data, Location location)
    {
        return data.Where(ef => location.Name == ef.Location.Name).SingleOrDefault();        
    }

    protected virtual async Task<List<EmissionsData>?> GetJsonDataAsync()
    {
        if (_emissionsData is not null)
        {
            return _emissionsData;
        }        
        if (_emissionsData is null || !_emissionsData.Any())
        {
            _emissionsData = (await GetEmissionsJsonFile())?.Emissions;
        }
        return _emissionsData;
    }

    protected virtual async Task<List<EmissionsForecast>?> GetJsonEmissionsForecastsAsync()
    {
        if (_emissionsForecasts is not null)
        {
            return _emissionsForecasts;
        }
        if (_emissionsForecasts is null || !_emissionsForecasts.Any())
        {
            _emissionsForecasts = (await GetEmissionsJsonFile())?.EmissionsForecasts;
        }
        return _emissionsForecasts;
    }

    private async Task<EmissionsJsonFile?> GetEmissionsJsonFile()
    {
        _logger.LogInformation($"Reading Json data from {_configuration.DataFileLocation}");        
        using Stream stream = File.OpenRead(_configuration.DataFileLocation!);
        return await JsonSerializer.DeserializeAsync<EmissionsJsonFile>(stream);
    }    
}
