using CarbonAware.DataSources.ElectricityMaps.Configuration;
using CarbonAware.DataSources.ElectricityMaps.Constants;
using CarbonAware.DataSources.ElectricityMaps.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text.Json;
using System.Web;

namespace CarbonAware.DataSources.ElectricityMaps.Client;

internal class ElectricityMapsClient : IElectricityMapsClient
{
    private static readonly JsonSerializerOptions _options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
    private readonly HttpClient _client;
    private readonly IOptionsMonitor<ElectricityMapsClientConfiguration> _configurationMonitor;
    private ElectricityMapsClientConfiguration _configuration => this._configurationMonitor.CurrentValue;
    private readonly ILogger<ElectricityMapsClient> _log;
    private readonly Lazy<Task<Dictionary<string, ZoneData>>> _zonesAllowed;

    public ElectricityMapsClient(IHttpClientFactory factory, IOptionsMonitor<ElectricityMapsClientConfiguration> monitor, ILogger<ElectricityMapsClient> log)
    {
        _client = factory.CreateClient(IElectricityMapsClient.NamedClient);
        _configurationMonitor = monitor;
        _log = log;
        _configuration.Validate();
        _client.BaseAddress = new Uri(this._configuration.BaseUrl);
        _client.DefaultRequestHeaders.Accept.Clear();
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
        if (!string.IsNullOrWhiteSpace(_configuration.APITokenHeader))
        {
            _client.DefaultRequestHeaders.Add(_configuration.APITokenHeader, _configuration.APIToken);
        }
        _zonesAllowed = new Lazy<Task<Dictionary<string, ZoneData>>>(async () => await PopulateZonesData());
    }

    /// <inheritdoc/>
    public async Task<HistoryCarbonIntensityData> GetRecentCarbonIntensityHistoryAsync(string zoneName)
    {
        _log.LogDebug("Requesting past carbon intensity using zone name {zoneName}",
            zoneName);

        var parameters = new Dictionary<string, string>()
        {
            { QueryStrings.ZoneName, zoneName },
        };

        return await GetHistoryCarbonIntensityDataAsync(parameters);
    }

    /// <inheritdoc/>
    public async Task<HistoryCarbonIntensityData> GetRecentCarbonIntensityHistoryAsync(string latitude, string longitude)
    {
        _log.LogDebug("Requesting past carbon intensity using latitude {latitude} longitude {longitude}",
            latitude, longitude);

        var parameters = new Dictionary<string, string>()
        {
            { QueryStrings.Latitude, latitude },
            { QueryStrings.Longitude, longitude }
        };

        return await GetHistoryCarbonIntensityDataAsync(parameters);
    }

    /// <inheritdoc/>
    public async Task<ForecastedCarbonIntensityData> GetForecastedCarbonIntensityAsync(string zoneName)
    {
        _log.LogDebug("Requesting forecasted carbon intensity using zone name {zoneName}",
            zoneName);

        var parameters = new Dictionary<string, string>()
        {
            { QueryStrings.ZoneName, zoneName },
        };

        return await GetCurrentForecastAsync(parameters);
    }

    /// <inheritdoc/>
    public async Task<ForecastedCarbonIntensityData> GetForecastedCarbonIntensityAsync(string latitude, string longitude)
    {
        _log.LogDebug("Requesting forecasted carbon intensity using latitude {latitude} longitude {longitude}",
            latitude, longitude);

        var parameters = new Dictionary<string, string>()
        {
            { QueryStrings.Latitude, latitude },
            { QueryStrings.Longitude, longitude },
        };

        return await GetCurrentForecastAsync(parameters);
    }

    /// <inheritdoc/>
    public async Task<PastRangeData> GetPastRangeDataAsync(string latitude, string longitude, DateTimeOffset startTime, DateTimeOffset endTime)
    {
        _log.LogDebug("Requesting carbon intensity using latitude {latitude} longitude {longitude}",
           latitude, longitude);

        var parameters = new Dictionary<string, string>()
        {
            { QueryStrings.Latitude, latitude },
            { QueryStrings.Longitude, longitude },
            { QueryStrings.StartTime, DateTimeToString(startTime) },
            { QueryStrings.EndTime, DateTimeToString(endTime) },
        };

        return await GetPastRangeDataAsync(parameters);
    }

    /// <inheritdoc/>
    public async Task<PastRangeData> GetPastRangeDataAsync(string zone, DateTimeOffset startTime, DateTimeOffset endTime)
    {
        _log.LogDebug("Requesting carbon intensity using zone {zone}",
           zone);

        var parameters = new Dictionary<string, string>()
        {
            { QueryStrings.ZoneName, zone },
            { QueryStrings.StartTime, DateTimeToString(startTime) },
            { QueryStrings.EndTime, DateTimeToString(endTime) },
        };

        return await GetPastRangeDataAsync(parameters);
    }

    // The ElectricityMaps API has strict checks about datetime formatting.
    // This helper method ensures that all DateTimeOffsets are properly formatted.
    private static string DateTimeToString(DateTimeOffset dateTime)
    {
        return dateTime.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Async method to check for allowed zones and then make GET request to History endpoint
    /// </summary>
    /// <param name="parameters">List of query params</param>
    /// <returns>A <see cref="Task{HistoryCarbonIntensityData}"/> which contains all emissions data points in the 24 hour period.</returns>
    /// <exception cref="ElectricityMapsClientException">Can be thrown when errors occur connecting to ElectricityMaps client.  See the ElectricityMapsClientException class for documentation of expected status codes.</exception>
    private async Task<HistoryCarbonIntensityData> GetHistoryCarbonIntensityDataAsync(Dictionary<string, string> parameters)
    {
        await CheckZonesAllowedForPathAsync(Paths.History, parameters);
        AddOptionalParameters(parameters);

        using Stream result = await this.MakeRequestGetStreamAsync(Paths.History, parameters);
        return await JsonSerializer.DeserializeAsync<HistoryCarbonIntensityData>(result, _options) ?? throw new ElectricityMapsClientException($"Error getting history carbon intensity data");
    }

    private void AddOptionalParameters(Dictionary<string, string> parameters)
    {
        if (_configuration.EmissionFactorType != null)
        {
            parameters.Add(QueryStrings.EmissionFactorType, _configuration.EmissionFactorType);
        }
        if (_configuration.DisableEstimations != null)
        {
            parameters.Add(QueryStrings.DisableEstimations, _configuration.DisableEstimations.ToString()!.ToLowerInvariant());
        }
    }

    private async Task<PastRangeData> GetPastRangeDataAsync(Dictionary<string, string> parameters)
    {
        await CheckZonesAllowedForPathAsync(Paths.PastRange, parameters);
        AddOptionalParameters(parameters);
        using Stream result = await this.MakeRequestGetStreamAsync(Paths.PastRange, parameters);
        return await JsonSerializer.DeserializeAsync<PastRangeData>(result, _options) ?? throw new ElectricityMapsClientException($"Error getting emissions data");
    }

    /// <summary>
    /// Async method to check for allowed zones and then make GET request to Forecast endpoint
    /// </summary>
    /// <param name="parameters">List of query params</param>
    /// <returns>A <see cref="Task{ForecastedCarbonIntensityData}"/> which contains all emissions data points in the 24 hour period.</returns>
    /// <exception cref="ElectricityMapsClientException">Can be thrown when errors occur connecting to ElectricityMaps client.  See the ElectricityMapsClientException class for documentation of expected status codes.</exception>
    private async Task<ForecastedCarbonIntensityData> GetCurrentForecastAsync(Dictionary<string, string> parameters)
    {
        await CheckZonesAllowedForPathAsync(Paths.Forecast, parameters);
        using Stream result = await this.MakeRequestGetStreamAsync(Paths.Forecast, parameters);
        return await JsonSerializer.DeserializeAsync<ForecastedCarbonIntensityData>(result, _options) ?? throw new ElectricityMapsClientException($"Error getting forecasted data");
    }

    private async Task<HttpResponseMessage> GetResponseAsync(string uriPath)
    {
        HttpResponseMessage response = await _client.GetAsync(uriPath, HttpCompletionOption.ResponseHeadersRead);
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            if (content.Contains(ErrorMessages.UnauthorizedZone))
            {
                _log.LogError("Error getting data from ElectricityMaps {uriPath}. Your token is not authorized for this zone. StatusCode: {statusCode}. Content: {content}", uriPath, response.StatusCode, content);
                throw new ElectricityMapsClientHttpException($"Error requesting {uriPath}. Your token is not authorized for this zone - Content: {content}", response);
            }
            else if (content.Contains(ErrorMessages.UnauthorizedEndpoint))
            {
                _log.LogError("Error getting data from ElectricityMaps {uriPath}. Your token is not authorized for this endpoint. StatusCode: {statusCode}. Content: {content}", uriPath, response.StatusCode, content);
                throw new ElectricityMapsClientHttpException($"Error requesting {uriPath}. Your token is not authorized for this endpoint - Content: {content}", response);
            }
            _log.LogError("Error getting data from ElectricityMaps {uriPath}. StatusCode: {statusCode}. Content: {content}", uriPath, response.StatusCode, content);
            throw new ElectricityMapsClientHttpException($"Error requesting {uriPath} - Content: {content}", response);
        }
        return response;
    }

    private async Task<Stream> MakeRequestGetStreamAsync(string path, Dictionary<string, string>? parameters = null)
    {
        var url = path;
        if (parameters is not null)
        {
            url = BuildUrlWithQueryString(path, parameters);
        }
        _log.LogDebug("Requesting data using url {url}", url);
        var response = await this.GetResponseAsync(url);
        return await response.Content.ReadAsStreamAsync();
    }

    private string BuildUrlWithQueryString(string url, IDictionary<string, string> queryStringParams)
    {
        if (_log.IsEnabled(LogLevel.Debug))
        {
            _log.LogDebug("Attempting to build a url using url {url} and query string parameters {parameters}", url, string.Join(";", queryStringParams.Select(k => $"\"{k.Key}\":\"{k.Value}\"")));
        }
        var query = HttpUtility.ParseQueryString(string.Empty);
        foreach (var kvp in queryStringParams)
        {
            query[kvp.Key] = kvp.Value;
        }
        var result = $"{url}?{query}";
        if (_log.IsEnabled(LogLevel.Debug))
        {
            _log.LogDebug("Built url {result} from url {url} and query string parameters {parameters}", result, url, string.Join(";", queryStringParams.Select(k => $"\"{k.Key}\":\"{k.Value}\"")));
        }
        return result;
    }

    // Checks the current supported client's endpoint paths.
    private async Task CheckZonesAllowedForPathAsync(string path, Dictionary<string, string> parameters)
    {
        // Parameters don't contain a ZoneName to check, exit
        if (!parameters.ContainsKey(QueryStrings.ZoneName)) return;

        Dictionary<string, ZoneData> values = new();
        try
        {
            values = await _zonesAllowed.Value;
        }
        catch (ElectricityMapsClientException e)
        {
            // Zones call fails, exit
            // Remark: as of 12/1/22, all trial user request to /zones fails with a 500 error
            _log.LogDebug(e.Message);
            return;
        }

        string zoneName = parameters[QueryStrings.ZoneName];

        // If zone name not contained within allowed zones, throw
        if (!values.ContainsKey(zoneName))
        {
            _log.LogError("Zone {zoneName} not supported for current token", zoneName);
            _log.LogError("Zones supported {zoneList}",
                String.Join(", ", values.Select(kvp => string.Format("{0}", kvp.Key))));
            throw new ElectricityMapsClientException($"Zone {zoneName} not supported for current token.");
        }

        ZoneData zoneData = values[zoneName]!;

        // If path not supported within zone, throw
        if (!(zoneData.Access.Contains(path) || zoneData.Access.Contains("*")))
        {
            _log.LogError("Path {path} not supported for current token on zone {zoneName}", path, zoneName);
            _log.LogError("Zones supported {zoneList}",
                string.Join(", ", values.Select(kvp => string.Format("{0}", kvp.Key))));
            throw new ElectricityMapsClientException($"Path {path} not supported for current token on zone {zoneName}.");
        }
    }

    // Make GET request to Zones endpoint to cache allowed zones given user token
    private async Task<Dictionary<string, ZoneData>> PopulateZonesData()
    {
        using var result = await MakeRequestGetStreamAsync(Paths.Zones);
        return await JsonSerializer.DeserializeAsync<Dictionary<string, ZoneData>>(result, _options) ?? throw new ElectricityMapsClientException($"Error getting zone data");
    }
}
