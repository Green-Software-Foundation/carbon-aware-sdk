using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CarbonAware.Tools.ElectricityMapClient.Model;
using System.Web;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Security.Authentication;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Net;
using CarbonAware.Tools.ElectricityMapClient.Configuration;
using CarbonAware.Tools.ElectricityMapClient.Constants;
using System.Globalization;

namespace CarbonAware.Tools.ElectricityMapClient;

public class ElectricityMapClient : IElectricityMapClient
{
    private static readonly JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

    private static readonly HttpStatusCode[] RetriableStatusCodes = new HttpStatusCode[]
    {
        HttpStatusCode.Unauthorized, 
        HttpStatusCode.Forbidden
    };

    private HttpClient client;

    private IOptionsMonitor<ElectricityMapClientConfiguration> ConfigurationMonitor { get; }

    private ElectricityMapClientConfiguration Configuration => this.ConfigurationMonitor.CurrentValue;

    private static readonly ActivitySource Activity = new ActivitySource(nameof(ElectricityMapClient));

    private ILogger<ElectricityMapClient> Log { get; }

    public ElectricityMapClient(IHttpClientFactory factory, IOptionsMonitor<ElectricityMapClientConfiguration> configurationMonitor, ILogger<ElectricityMapClient> log)
    {
        this.client = factory.CreateClient(IElectricityMapClient.NamedClient);
        this.ConfigurationMonitor = configurationMonitor;
        this.Log = log;
        this.client.BaseAddress = new Uri(this.Configuration.BaseUrl);
        this.client.DefaultRequestHeaders.Accept.Clear();
        this.client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
    }

    /// <inheritdoc/>
    public async Task<Forecast?> GetCurrentForecastAsync(string countryCodeAbbreviation)
    {

        Log.LogInformation("Requesting current forecast from zone {countryCode}", countryCodeAbbreviation);

        var parameters = new Dictionary<string, string>()
        {
            { QueryStrings.countryCodeAbbreviation, countryCodeAbbreviation }
        };

        var tags = new Dictionary<string, string>()
        {
            { QueryStrings.countryCodeAbbreviation, countryCodeAbbreviation }
        };

        var result = await this.MakeRequestAsync(parameters, tags);
 
        var emission_data = JsonSerializer.Deserialize<GridEmissionDataPoint?>(result, options) ?? throw new ElectricityMapClientException($"Error getting forecast for  {countryCodeAbbreviation}");

        // convert from emissiondata to forecast
        var forecastdata = new List<GridEmissionDataPoint>(){ emission_data };
        var forecast = new Forecast()
        {
            GeneratedAt = emission_data.Data.Datetime,
            ForecastData = forecastdata
        };

        return forecast;
    }

    /// <inheritdoc/>
    public Task<Forecast?> GetCurrentForecastAsync(Zone zone)
    {
        return this.GetCurrentForecastAsync(zone.countryCode);
    }

    private async Task<HttpResponseMessage> GetAsyncWithAuthRetry(string uriPath)
    {
        await this.EnsureTokenAsync();

        var response = await this.client.GetAsync(uriPath);

        if (RetriableStatusCodes.Contains(response.StatusCode))
        {
            Log.LogDebug("Failed to get url {url} with status code {statusCode}.  Attempting to log in again.", uriPath, response.StatusCode);
            await this.UpdateAuthTokenAsync();
            response = await this.client.GetAsync(uriPath);
        }

        if (!response.IsSuccessStatusCode)
        {
            Log.LogError("Error getting data from electricityMap.  StatusCode: {statusCode}. Response: {response}", response.StatusCode, response);
            throw new ElectricityMapClientHttpException($"Error getting data from electricityMap: {response.StatusCode}", response);
        }
        return response;
    }

    private async Task<string> GetAsyncStringWithAuthRetry(string uriPath)
    {
        var response = await this.GetAsyncWithAuthRetry(uriPath);
        var data = await response.Content.ReadAsStringAsync();
        return data ?? string.Empty;
    }

    private async Task<Stream> GetAsyncStreamWithAuthRetry(string uriPath)
    {
        var response = await this.GetAsyncWithAuthRetry(uriPath);
        return await response.Content.ReadAsStreamAsync();
    }

    private async Task EnsureTokenAsync()
    {
        if (!this.client.DefaultRequestHeaders.Contains("auth-token"))
        {
            await this.UpdateAuthTokenAsync();
        }
    }

    private async Task UpdateAuthTokenAsync()
    {
        using (var activity = Activity.StartActivity())
        {
            Log.LogInformation("Attempting to log using token {token}", this.Configuration.Token);
            this.SetAuthTokenAuthenticationHeader(this.Configuration.Token);
        }
    }

    internal void SetAuthTokenAuthenticationHeader(string token)
    {
        this.client.DefaultRequestHeaders.Add("auth-token", token);
    }

    // Overload method for electricity Map Personal 
    private async Task<string> MakeRequestAsync(Dictionary<string, string> parameters, Dictionary<string, string>? tags = null)
    {
        using (var activity = Activity.StartActivity())
        {
            var url = BuildUrlWithQueryString(parameters);

            Log.LogInformation("Requesting data using url {url}", url);

            if (tags != null)
            {
                foreach (var kvp in tags)
                {
                    activity?.AddTag(kvp.Key, kvp.Value);
                }
            }

            var result = await this.GetAsyncStringWithAuthRetry(url);

            Log.LogDebug("For query {url}, received data {result}", url, result);

            return result;
        }
    }

    private string BuildUrlWithQueryString(IDictionary<string, string> queryStringParams)
    {
        if (Log.IsEnabled(LogLevel.Debug))
        {
            Log.LogDebug("Attempting to build a url using query string parameters {parameters}", string.Join(";", queryStringParams.Select(k => $"\"{k.Key}\":\"{k.Value}\"")));
        }

        // this will get a specialized namevalue collection for formatting query strings.
        var query = HttpUtility.ParseQueryString(string.Empty);

        foreach (var kvp in queryStringParams)
        {
            query[kvp.Key] = kvp.Value;
        }

        var result = $"?{query}";

        if (Log.IsEnabled(LogLevel.Debug))
        {
            Log.LogDebug("Built url {result} from query string parameters {parameters}", result, string.Join(";", queryStringParams.Select(k => $"\"{k.Key}\":\"{k.Value}\"")));
        }

        return result;
    }

    private async Task<string> MakeRequestAsync(string path, Dictionary<string, string> parameters, Dictionary<string, string>? tags = null)
    {
        using (var activity = Activity.StartActivity())
        {
            var url = BuildUrlWithQueryString(path, parameters);

            Log.LogInformation("Requesting data using url {url}", url);

            if (tags != null)
            {
                foreach (var kvp in tags)
                {
                    activity?.AddTag(kvp.Key, kvp.Value);
                }
            }

            var result = await this.GetAsyncStringWithAuthRetry(url);

            Log.LogDebug("For query {url}, received data {result}", url, result);

            return result;
        }
    }

    private string BuildUrlWithQueryString(string url, IDictionary<string, string> queryStringParams)
    {
        if (Log.IsEnabled(LogLevel.Debug))
        {
            Log.LogDebug("Attempting to build a url using url {url} and query string parameters {parameters}", url, string.Join(";", queryStringParams.Select(k => $"\"{k.Key}\":\"{k.Value}\"")));
        }

        // this will get a specialized namevalue collection for formatting query strings.
        var query = HttpUtility.ParseQueryString(string.Empty);

        foreach (var kvp in queryStringParams)
        {
            query[kvp.Key] = kvp.Value;
        }

        var result = $"{url}?{query}";

        if (Log.IsEnabled(LogLevel.Debug))
        {
            Log.LogDebug("Built url {result} from url {url} and query string parameters {parameters}", result, url, string.Join(";", queryStringParams.Select(k => $"\"{k.Key}\":\"{k.Value}\"")));
        }

        return result;
    }
}
