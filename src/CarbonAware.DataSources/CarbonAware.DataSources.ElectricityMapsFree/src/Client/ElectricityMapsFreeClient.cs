using CarbonAware.DataSources.ElectricityMapsFree.Configuration;
using CarbonAware.DataSources.ElectricityMapsFree.Constants;
using CarbonAware.DataSources.ElectricityMapsFree.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Net;
using System.Text.Json;
using System.Web;

namespace CarbonAware.DataSources.ElectricityMapsFree.Client;

public class ElectricityMapsFreeClient : IElectricityMapsFreeClient
{
    private static readonly JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

    private static readonly HttpStatusCode[] RetriableStatusCodes = new HttpStatusCode[]
    {
        HttpStatusCode.Unauthorized,
        HttpStatusCode.Forbidden
    };

    private readonly HttpClient client;

    private IOptionsMonitor<ElectricityMapsFreeClientConfiguration> ConfigurationMonitor { get; }

    private ElectricityMapsFreeClientConfiguration Configuration => this.ConfigurationMonitor.CurrentValue;

    private static readonly ActivitySource Activity = new ActivitySource(nameof(ElectricityMapsFreeClient));

    private ILogger<ElectricityMapsFreeClient> Log { get; }

    public ElectricityMapsFreeClient(IHttpClientFactory factory, IOptionsMonitor<ElectricityMapsFreeClientConfiguration> configurationMonitor, ILogger<ElectricityMapsFreeClient> log)
    {
        this.client = factory.CreateClient(IElectricityMapsFreeClient.NamedClient);
        this.ConfigurationMonitor = configurationMonitor;
        this.Log = log;
        Configuration.Validate();
        this.client.BaseAddress = new Uri(this.Configuration.BaseUrl);
        this.client.DefaultRequestHeaders.Accept.Clear();
        this.client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
    }

    /// <inheritdoc/>
    public async Task<GridEmissionDataPoint> GetCurrentEmissionsAsync(string countryCodeAbbreviation)
    {
        Log.LogInformation("Requesting latest carbon intensity from zone {countryCode}", countryCodeAbbreviation);

        var parameters = new Dictionary<string, string>()
        {
            { QueryStrings.countryCodeAbbreviation, countryCodeAbbreviation }
        };

        var tags = new Dictionary<string, string>()
        {
            { QueryStrings.countryCodeAbbreviation, countryCodeAbbreviation }
        };

        var result = await this.MakeRequestAsync(Paths.Latest, parameters, tags);
        var emissionData = JsonSerializer.Deserialize<GridEmissionDataPoint>(result, options) ?? throw new ElectricityMapsFreeClientException($"Error getting latest carbon intensity for countryCode {countryCodeAbbreviation}");
        if (emissionData == null || emissionData.Data.CarbonIntensity == null || emissionData.Data.Datetime == null)
        {
            Log.LogError("Region {countryCode} is not known", countryCodeAbbreviation);
            throw new ElectricityMapsFreeClientException($"Region {countryCodeAbbreviation} is not known");
        }
        return emissionData;
    }

    /// <inheritdoc/>
    public async Task<GridEmissionDataPoint> GetCurrentEmissionsAsync(string latitude, string longitude)
    {
        Log.LogDebug("Requesting latest carbon intensity using latitude {latitude} longitude {longitude}",
            latitude, longitude);

        var parameters = new Dictionary<string, string>()
        {
            { QueryStrings.Latitude, latitude },
            { QueryStrings.Longitude, longitude }
        };

        var result = await this.MakeRequestAsync(Paths.Latest, parameters);

        var emissionData = JsonSerializer.Deserialize<GridEmissionDataPoint>(result, options) ?? throw new ElectricityMapsFreeClientException($"Error getting forecast for latitude {latitude} longitude {longitude}");
        return emissionData;
    }

    private async Task<HttpResponseMessage> GetAsyncWithAuthRetry(string uriPath)
    {
        this.EnsureToken();

        var response = await this.client.GetAsync(uriPath);

        if (RetriableStatusCodes.Contains(response.StatusCode))
        {
            Log.LogDebug("Failed to get url {url} with status code {statusCode}.  Attempting to log in again.", uriPath, response.StatusCode);
            this.UpdateAuthToken();
            response = await this.client.GetAsync(uriPath);
        }

        if (!response.IsSuccessStatusCode)
        {
            Log.LogError("Error getting data from electricityMapsFree.  StatusCode: {statusCode}. Response: {response}", response.StatusCode, response);
            throw new ElectricityMapsFreeClientHttpException($"Error getting data from electricityMapsFree: {response.StatusCode}", response);
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

    private void EnsureToken()
    {
        if (!this.client.DefaultRequestHeaders.Contains("auth-token"))
        {
            this.UpdateAuthToken();
        }
    }

    private void UpdateAuthToken()
    {
        using (var activity = Activity.StartActivity())
        {
            Log.LogInformation("Attempting to log using token {token}", this.Configuration.Token);
            this.SetAuthTokenAuthenticationHeader(this.Configuration.Token!);
        }
    }

    internal void SetAuthTokenAuthenticationHeader(string token)
    {
        this.client.DefaultRequestHeaders.Add("auth-token", token);
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


