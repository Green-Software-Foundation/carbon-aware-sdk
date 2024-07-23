using CarbonAware.DataSources.WattTime.Configuration;
using CarbonAware.DataSources.WattTime.Constants;
using CarbonAware.DataSources.WattTime.Model;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Web;

namespace CarbonAware.DataSources.WattTime.Client;

internal class WattTimeClient : IWattTimeClient
{
    private static readonly JsonSerializerOptions _options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

    private static readonly HttpStatusCode[] _retriableStatusCodes = new HttpStatusCode[]
    {
        HttpStatusCode.Unauthorized,
        HttpStatusCode.Forbidden
    };

    private HttpClient _client;
    private HttpClient _authenticationClient;

    private IOptionsMonitor<WattTimeClientConfiguration> _configurationMonitor { get; }

    private WattTimeClientConfiguration _configuration => this._configurationMonitor.CurrentValue;

    private ILogger<WattTimeClient> _log { get; }

    private IMemoryCache _memoryCache { get; }

    public WattTimeClient(IHttpClientFactory factory, IOptionsMonitor<WattTimeClientConfiguration> configurationMonitor, ILogger<WattTimeClient> log, IMemoryCache memoryCache)
    {
        _client = factory.CreateClient(IWattTimeClient.NamedClient);
        _authenticationClient = factory.CreateClient(IWattTimeClient.NamedAuthenticationClient);

        _configurationMonitor = configurationMonitor;
        _log = log;
        _configuration.Validate();
        _client.BaseAddress = new Uri(this._configuration.BaseUrl);
        _client.DefaultRequestHeaders.Accept.Clear();
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));

        _authenticationClient.BaseAddress = new Uri(this._configuration.AuthenticationBaseUrl);
        _authenticationClient.DefaultRequestHeaders.Accept.Clear();
        _authenticationClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));

        _memoryCache = memoryCache;
    }

    /// <inheritdoc/>
    public async Task<GridEmissionsDataResponse> GetDataAsync(string regionAbbreviation, DateTimeOffset startTime, DateTimeOffset endTime)
    {
        _log.LogInformation("Requesting grid emission data using start time {startTime} and endTime {endTime}", startTime, endTime);

        var parameters = new Dictionary<string, string>()
        {
            { QueryStrings.Region, regionAbbreviation },
            { QueryStrings.StartTime, startTime.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture) },
            { QueryStrings.EndTime, endTime.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture) },
            { QueryStrings.SignalType, SignalTypes.co2_moer},
        };

        var tags = new Dictionary<string, string>()
        {
            { QueryStrings.Region, regionAbbreviation }
        };

        using (var result = await this.MakeRequestGetStreamAsync(Paths.Data, parameters, tags))
        {
            return await JsonSerializer.DeserializeAsync<GridEmissionsDataResponse>(result, _options) ?? throw new WattTimeClientException($"Error getting forecasts for {regionAbbreviation}");
        }
    }

    /// <inheritdoc/>
    public Task<GridEmissionsDataResponse> GetDataAsync(RegionResponse region, DateTimeOffset startTime, DateTimeOffset endTime)
    {
        return this.GetDataAsync(region.Region, startTime, endTime);
    }

    /// <inheritdoc/>
    public async Task<ForecastEmissionsDataResponse> GetCurrentForecastAsync(string region)
    {

        _log.LogInformation("Requesting current forecast from region: {region}", region);

        var parameters = new Dictionary<string, string>()
        {
            { QueryStrings.Region, region },
            { QueryStrings.SignalType, SignalTypes.co2_moer }
        };

        var tags = new Dictionary<string, string>()
        {
            { QueryStrings.Region, region }
        };

        var result = await this.MakeRequestGetStreamAsync(Paths.Forecast, parameters, tags);

        var forecast = await JsonSerializer.DeserializeAsync<ForecastEmissionsDataResponse?>(result, _options) ?? throw new WattTimeClientException($"Error getting forecast for  {region}");

        return forecast;
    }

    /// <inheritdoc/>
    public Task<ForecastEmissionsDataResponse> GetCurrentForecastAsync(RegionResponse region)
    {
        return this.GetCurrentForecastAsync(region.Region);
    }

    /// <inheritdoc/>
    public async Task<HistoricalForecastEmissionsDataResponse?> GetForecastOnDateAsync(string region, DateTimeOffset requestedAt)
    {
        _log.LogInformation($"Requesting forecast from region {region} generated at {requestedAt}.");

        var parameters = new Dictionary<string, string>()
        {
            { QueryStrings.Region, region },
            { QueryStrings.StartTime, requestedAt.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture) },
            { QueryStrings.EndTime, requestedAt.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture) },
            { QueryStrings.SignalType, SignalTypes.co2_moer }
        };

        var tags = new Dictionary<string, string>()
        {
            { QueryStrings.Region, region }
        };
        using (var result = await this.MakeRequestGetStreamAsync(Paths.ForecastHistorical, parameters, tags))
        {
            var historicalForecastResponse = await JsonSerializer.DeserializeAsync<HistoricalForecastEmissionsDataResponse>(result, _options) ?? throw new WattTimeClientException($"Error getting forecasts for {region}");
            return historicalForecastResponse;
        }
    }

    /// <inheritdoc/>
    public Task<HistoricalForecastEmissionsDataResponse?> GetForecastOnDateAsync(RegionResponse region, DateTimeOffset requestedAt)
    {
        return this.GetForecastOnDateAsync(region.Region, requestedAt);
    }

    /// <inheritdoc/>
    public async Task<RegionResponse> GetRegionAsync(string latitude, string longitude)
    {
        _log.LogInformation("Requesting region for latitude {latitude} and longitude {longitude}", latitude, longitude);
        return await GetRegionFromCacheAsync(latitude, longitude);
    }

    /// <inheritdoc/>
    public async Task<string?> GetRegionAbbreviationAsync(string latitude, string longitude)
    {
        return (await this.GetRegionAsync(latitude, longitude))?.Region;
    }

    /// <inheritdoc/>
    public async Task<Stream> GetHistoricalDataAsync(string regionAbbreviation)
    {
        _log.LogInformation("Requesting historical data for region {regionAbbreviation}", regionAbbreviation);

        var parameters = new Dictionary<string, string>()
        {
            { QueryStrings.Region, regionAbbreviation }
        };

        var url = BuildUrlWithQueryString(Paths.Historical, parameters);

        _log.LogInformation("Requesting data using url {url}", url);

        var result = await this.GetStreamWithAuthRetryAsync(url);

        _log.LogDebug("For query {url}, received data stream", url);

        return result;
    }

    /// <inheritdoc/>
    public Task<Stream> GetHistoricalDataAsync(RegionResponse region)
    {
        return this.GetHistoricalDataAsync(region.Region);
    }

    private async Task<HttpResponseMessage> GetAsyncWithAuthRetry(string uriPath)
    {
        await this.EnsureTokenAsync();
        HttpResponseMessage response = await this._client.GetAsync(uriPath, HttpCompletionOption.ResponseHeadersRead);

        if (_retriableStatusCodes.Contains(response.StatusCode))
        {
            _log.LogDebug("Failed to get url {url} with status code {statusCode}.  Attempting to log in again.", uriPath, response.StatusCode);
            await this.UpdateAuthTokenAsync();
            response = await this._client.GetAsync(uriPath, HttpCompletionOption.ResponseHeadersRead);
        }

        if (!response.IsSuccessStatusCode)
        {
            _log.LogError("Error getting data from WattTime.  StatusCode: {statusCode}. Response: {response}", response.StatusCode, response);

            throw new WattTimeClientHttpException($"Error requesting {uriPath}", response);
        }

        return response;
    }

    private async Task<Stream> GetStreamWithAuthRetryAsync(string uriPath)
    {
        var response = await this.GetAsyncWithAuthRetry(uriPath);
        return await response.Content.ReadAsStreamAsync();
    }


    private async Task EnsureTokenAsync()
    {
        if (this._client.DefaultRequestHeaders.Authorization == null)
        {
            await this.UpdateAuthTokenAsync();
        }
    }

    private async Task UpdateAuthTokenAsync()
    {
        _log.LogInformation("Attempting to log in user {username}", this._configuration.Username);

        this.SetBasicAuthenticationHeader();
        HttpResponseMessage response = await this._authenticationClient.GetAsync(Paths.Login);

        LoginResult? data = null;

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync() ?? String.Empty;

            data = JsonSerializer.Deserialize<LoginResult>(json, _options);
        }

        if (data == null)
        {
            _log.LogError("Login failed for user {username}.  Response: {response}", this._configuration.Username, response);
            throw new WattTimeClientHttpException($"Login failed for user: '{this._configuration.Username}'", response);
        }

        this.SetBearerAuthenticationHeader(data.Token);
    }

    private void SetBasicAuthenticationHeader()
    {
        var authToken = Encoding.UTF8.GetBytes($"{this._configuration.Username}:{this._configuration.Password}");
        //this._client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AuthenticationHeaderTypes.Basic, Convert.ToBase64String(authToken));
        this._authenticationClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AuthenticationHeaderTypes.Basic, Convert.ToBase64String(authToken));
    }

    internal void SetBearerAuthenticationHeader(string token)
    {
        this._client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AuthenticationHeaderTypes.Bearer, token);
    }

    private async Task<Stream> MakeRequestGetStreamAsync(string path, Dictionary<string, string> parameters, Dictionary<string, string>? tags = null)
    {
        var url = BuildUrlWithQueryString(path, parameters);
        _log.LogInformation("Requesting data using url {url}", url);
        var result = await this.GetStreamWithAuthRetryAsync(url);
        _log.LogDebug("For query {url}, received data {result}", url, result);
        return result;
    }


    private string BuildUrlWithQueryString(string url, IDictionary<string, string> queryStringParams)
    {
        if (_log.IsEnabled(LogLevel.Debug))
        {
            _log.LogDebug("Attempting to build a url using url {url} and query string parameters {parameters}", url, string.Join(";", queryStringParams.Select(k => $"\"{k.Key}\":\"{k.Value}\"")));
        }

        // this will get a specialized namevalue collection for formatting query strings.
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

    private async Task<RegionResponse> GetRegionFromCacheAsync(string latitude, string longitude)
    {
        var key = new Tuple<string, string>(latitude, longitude);
        var region = await this._memoryCache.GetOrCreateAsync(key, async entry =>
        {
            var parameters = new Dictionary<string, string>()
            {
                { QueryStrings.Latitude, latitude },
                { QueryStrings.Longitude, longitude },
                { QueryStrings.SignalType, SignalTypes.co2_moer}
            };

            var tags = new Dictionary<string, string>()
            {
                { QueryStrings.Latitude, latitude },
                { QueryStrings.Longitude, longitude },
                { QueryStrings.SignalType, SignalTypes.co2_moer }
            };
            var result = await this.MakeRequestGetStreamAsync(Paths.RegionFromLocation, parameters, tags);
            var regionResponse = await JsonSerializer.DeserializeAsync<RegionResponse>(result, _options) ?? throw new WattTimeClientException($"Error getting Region for latitude {latitude} and longitude {longitude}");
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_configuration.RegionCacheTTL);
            entry.Value = regionResponse;
            return regionResponse;
        });
        return region;
    }
}
