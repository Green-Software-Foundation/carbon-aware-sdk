using CarbonAware.DataSources.WattTime.Configuration;
using CarbonAware.DataSources.WattTime.Constants;
using CarbonAware.DataSources.WattTime.Model;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Contrib.HttpClient;
using NUnit.Framework;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CarbonAware.DataSources.WattTime.Client.Tests;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
class WattTimeClientTests
{
    private Mock<HttpMessageHandler> Handler { get; set; }

    private IHttpClientFactory HttpClientFactory { get; set; }

    private WattTimeClientConfiguration Configuration { get; set; }

    private Mock<IOptionsMonitor<WattTimeClientConfiguration>> Options { get; set; }

    private Mock<ILogger<WattTimeClient>> Log { get; set; }

    private string BasicAuthValue { get; set; }

    private readonly string _DEFAULT_TOKEN_VALUE = "myDefaultToken123";
    private readonly string _BASE_WATTTIME_LOGIN_URL = "https://api.watttime.org/login";

    private IMemoryCache MemoryCache { get; set; }

    [SetUp]
    public void Initialize()
    {
        this.Configuration = new WattTimeClientConfiguration() { Username = "username", Password = "password" };

        this.Options = new Mock<IOptionsMonitor<WattTimeClientConfiguration>>();
        this.Log = new Mock<ILogger<WattTimeClient>>();

        this.Options.Setup(o => o.CurrentValue).Returns(() => this.Configuration);

        this.BasicAuthValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{this.Configuration.Username}:{this.Configuration.Password}"));

        this.Handler = new Mock<HttpMessageHandler>();
        this.HttpClientFactory = Handler.CreateClientFactory();
        Mock.Get(this.HttpClientFactory).Setup(x => x.CreateClient(IWattTimeClient.NamedClient))
            .Returns(() =>
            {
                var client = Handler.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _DEFAULT_TOKEN_VALUE);
                return client;
            });

        this.MemoryCache = new MemoryCache(new MemoryCacheOptions());
    }

    [Test]
    public void AllPublicMethods_ThrowsWhenInvalidLogin()
    {
        this.AddHandlers_Auth("token");

        this.BasicAuthValue = "invalid";
        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object, this.MemoryCache);

        Assert.ThrowsAsync<WattTimeClientHttpException>(async () => await client.GetDataAsync(WattTimeTestData.Constants.Region, new DateTimeOffset(), new DateTimeOffset()));
        Assert.ThrowsAsync<WattTimeClientHttpException>(async () => await client.GetCurrentForecastAsync(WattTimeTestData.Constants.Region));
        Assert.ThrowsAsync<WattTimeClientHttpException>(async () => await client.GetForecastOnDateAsync(WattTimeTestData.Constants.Region, new DateTimeOffset()));
        Assert.ThrowsAsync<WattTimeClientHttpException>(async () => await client.GetRegionAsync("lat", "long"));
        Assert.ThrowsAsync<WattTimeClientHttpException>(async () => await client.GetRegionAbbreviationAsync("lat", "long"));
        Assert.ThrowsAsync<WattTimeClientHttpException>(async () => await client.GetHistoricalDataAsync(WattTimeTestData.Constants.Region));
    }

    [Test]
    public void AllPublicMethods_ThrowClientException_WhenNull()
    {
        this.SetupBasicHandlers("null");

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object, this.MemoryCache);
        client.SetBearerAuthenticationHeader(_DEFAULT_TOKEN_VALUE);
        var region = new RegionResponse() { Region = WattTimeTestData.Constants.Region };

        Assert.ThrowsAsync<WattTimeClientException>(async () => await client.GetRegionAsync("lat", "long"));
        Assert.ThrowsAsync<WattTimeClientException>(async () => await client.GetDataAsync(region.Region, new DateTimeOffset(), new DateTimeOffset()));
        Assert.ThrowsAsync<WattTimeClientException>(async () => await client.GetDataAsync(region, new DateTimeOffset(), new DateTimeOffset()));
        Assert.ThrowsAsync<WattTimeClientException>(async () => await client.GetCurrentForecastAsync(region.Region));
        Assert.ThrowsAsync<WattTimeClientException>(async () => await client.GetCurrentForecastAsync(region));
        Assert.ThrowsAsync<WattTimeClientException>(async () => await client.GetForecastOnDateAsync(region.Region, new DateTimeOffset()));
        Assert.ThrowsAsync<WattTimeClientException>(async () => await client.GetForecastOnDateAsync(region, new DateTimeOffset()));
    }

    [Test]
    public void AllPublicMethods_ThrowJsonException_WhenBadJsonIsReturned()
    {
        this.SetupBasicHandlers("This is bad json");

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object, this.MemoryCache);
        client.SetBearerAuthenticationHeader(_DEFAULT_TOKEN_VALUE);
        var region = new RegionResponse() { Region = WattTimeTestData.Constants.Region };

        Assert.ThrowsAsync<JsonException>(async () => await client.GetRegionAsync("lat", "long"));
        Assert.ThrowsAsync<JsonException>(async () => await client.GetDataAsync(region.Region, new DateTimeOffset(), new DateTimeOffset()));
        Assert.ThrowsAsync<JsonException>(async () => await client.GetDataAsync(region, new DateTimeOffset(), new DateTimeOffset()));
        Assert.ThrowsAsync<JsonException>(async () => await client.GetCurrentForecastAsync(region.Region));
        Assert.ThrowsAsync<JsonException>(async () => await client.GetCurrentForecastAsync(region));
        Assert.ThrowsAsync<JsonException>(async () => await client.GetForecastOnDateAsync(region.Region, new DateTimeOffset()));
        Assert.ThrowsAsync<JsonException>(async () => await client.GetForecastOnDateAsync(region, new DateTimeOffset()));
    }

    [Test]
    public async Task GetDataAsync_DeserializesExpectedResponse()
    {
        this.AddHandlers_Auth();
        this.AddHandler_RequestResponse(r =>
        {
            return r.RequestUri!.ToString().Equals($"https://api.watttime.org/v3/historical?region={WattTimeTestData.Constants.Region}&start=2022-04-22T00%3a00%3a00.0000000%2b00%3a00&end=2022-04-22T00%3a00%3a00.0000000%2b00%3a00&signal_type=co2_moer") && r.Method == HttpMethod.Get;
        }, System.Net.HttpStatusCode.OK, WattTimeTestData.GetGridDataResponseJsonString());

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object, this.MemoryCache);
        client.SetBearerAuthenticationHeader(_DEFAULT_TOKEN_VALUE);

        var emissionsResponse = await client.GetDataAsync(WattTimeTestData.Constants.Region, new DateTimeOffset(2022, 4, 22, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(2022, 4, 22, 0, 0, 0, TimeSpan.Zero));

        Assert.IsTrue(emissionsResponse.Data.Count() > 0);
        var meta = emissionsResponse.Meta;
        Assert.AreEqual(WattTimeTestData.Constants.Region, meta.Region);
        Assert.AreEqual(WattTimeTestData.Constants.SignalType, meta.SignalType);
        var gridDataPoint = emissionsResponse.Data.ToList().First();
        Assert.AreEqual(WattTimeTestData.Constants.Frequency, gridDataPoint.Frequency);
        Assert.AreEqual(WattTimeTestData.Constants.Market, gridDataPoint.Market);
        Assert.AreEqual(WattTimeTestData.Constants.PointTime, gridDataPoint.PointTime);
        Assert.AreEqual(WattTimeTestData.Constants.Value.ToString("0.00", CultureInfo.InvariantCulture), gridDataPoint.Value.ToString("0.00", CultureInfo.InvariantCulture)); //Format float to avoid precision issues
        Assert.AreEqual(WattTimeTestData.Constants.Version, gridDataPoint.Version);
    }

    [Test]
    public async Task GetDataAsync_RefreshesTokenWhenExpired()
    {
        this.SetupBasicHandlers(WattTimeTestData.GetGridDataResponseJsonString(), "REFRESHTOKEN");

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object, this.MemoryCache);
        client.SetBearerAuthenticationHeader(_DEFAULT_TOKEN_VALUE);

        var emissionsResponse = await client.GetDataAsync(WattTimeTestData.Constants.Region, new DateTimeOffset(), new DateTimeOffset());

        Assert.IsTrue(emissionsResponse.Data.Count() > 0);
        Assert.AreEqual(WattTimeTestData.Constants.Region, emissionsResponse.Meta.Region);
    }

    [Test]
    public async Task GetDataAsync_RefreshesTokenWhenNoneSet()
    {
        this.SetupBasicHandlers(WattTimeTestData.GetGridDataResponseJsonString(), "REFRESHTOKEN");

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object, this.MemoryCache);

        var gridEmissionsResponse = await client.GetDataAsync(WattTimeTestData.Constants.Region, new DateTimeOffset(), new DateTimeOffset());

        Assert.IsTrue(gridEmissionsResponse.Data.Count() > 0);
        Assert.AreEqual(WattTimeTestData.Constants.Region, gridEmissionsResponse.Meta.Region);
    }

    [Test]
    public async Task GetCurrentForecastAsync_DeserializesExpectedResponse()
    {
        this.AddHandlers_Auth();
        this.AddHandler_RequestResponse(r =>
        {
            return r.RequestUri!.ToString().Equals($"https://api.watttime.org/v3/forecast?region={WattTimeTestData.Constants.Region}&signal_type=co2_moer") && r.Method == HttpMethod.Get;
        }, System.Net.HttpStatusCode.OK, WattTimeTestData.GetCurrentForecastJsonString());

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object, this.MemoryCache);
        client.SetBearerAuthenticationHeader(_DEFAULT_TOKEN_VALUE);

        var forecastResponse = await client.GetCurrentForecastAsync(WattTimeTestData.Constants.Region);
        var overloadedForecast = await client.GetCurrentForecastAsync(WattTimeTestData.Constants.Region);

        Assert.AreEqual(forecastResponse.Meta.GeneratedAt, overloadedForecast.Meta.GeneratedAt);
        Assert.AreEqual(forecastResponse.Data.First(), overloadedForecast.Data.First());

        Assert.IsNotNull(forecastResponse);
        Assert.AreEqual(WattTimeTestData.Constants.GeneratedAt, forecastResponse?.Meta.GeneratedAt);
        Assert.AreEqual(WattTimeTestData.Constants.Region, forecastResponse?.Meta.Region);
        var forecastDataPoint = forecastResponse?.Data.First();

        Assert.AreEqual(WattTimeTestData.Constants.PointTime, forecastDataPoint?.PointTime);
        Assert.AreEqual(WattTimeTestData.Constants.Value.ToString("0.00", CultureInfo.InvariantCulture), forecastDataPoint?.Value.ToString("0.00", CultureInfo.InvariantCulture)); //Format float to avoid precision issues
        Assert.AreEqual(WattTimeTestData.Constants.Version, forecastDataPoint?.Version);
    }

    [Test]
    public async Task GetCurrentForecastAsync_RefreshesTokenWhenExpired()
    {
        this.SetupBasicHandlers(WattTimeTestData.GetCurrentForecastJsonString(), "REFRESHTOKEN");

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object, this.MemoryCache);
        client.SetBearerAuthenticationHeader(_DEFAULT_TOKEN_VALUE);

        var forecastResponse = await client.GetCurrentForecastAsync(WattTimeTestData.Constants.Region);

        Assert.IsNotNull(forecastResponse);
        Assert.AreEqual(WattTimeTestData.Constants.GeneratedAt, forecastResponse?.Meta.GeneratedAt);
        Assert.AreEqual(WattTimeTestData.Constants.Region, forecastResponse?.Meta.Region);
    }

    [Test]
    public async Task GetCurrentForecastAsync_RefreshesTokenWhenNoneSet()
    {
        // Override http client mock to remove authorization header
        Mock.Get(this.HttpClientFactory).Setup(x => x.CreateClient(IWattTimeClient.NamedClient))
            .Returns(() =>
            {
                var client = Handler.CreateClient();
                client.DefaultRequestHeaders.Authorization = null; // Null authorization header
                return client;
            });
        this.SetupBasicHandlers(WattTimeTestData.GetCurrentForecastJsonString(), "REFRESHTOKEN");

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object, this.MemoryCache);

        var forecastResponse = await client.GetCurrentForecastAsync(WattTimeTestData.Constants.Region);

        Assert.IsNotNull(forecastResponse);
        Assert.AreEqual(WattTimeTestData.Constants.GeneratedAt, forecastResponse?.Meta.GeneratedAt);
        Assert.AreEqual(WattTimeTestData.Constants.Region, forecastResponse?.Meta.Region);
    }

    [Test]
    public async Task GetForecastOnDateAsync_DeserializesExpectedResponse()
    {
        this.AddHandlers_Auth();
        this.AddHandler_RequestResponse(r =>
        {
            return r.RequestUri!.ToString().Equals($"https://api.watttime.org/v3/forecast/historical?region={WattTimeTestData.Constants.Region}&start=2022-04-22T00%3a00%3a00.0000000%2b00%3a00&end=2022-04-22T00%3a00%3a00.0000000%2b00%3a00&signal_type=co2_moer") && r.Method == HttpMethod.Get;
        }, System.Net.HttpStatusCode.OK, WattTimeTestData.GetHistoricalForecastDataJsonString());

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object, this.MemoryCache);
        client.SetBearerAuthenticationHeader(_DEFAULT_TOKEN_VALUE);
        var region = new RegionResponse() { Region = WattTimeTestData.Constants.Region };

        var forecastResponse = await client.GetForecastOnDateAsync(region.Region, new DateTimeOffset(2022, 4, 22, 0, 0, 0, TimeSpan.Zero));
        var overloadedForecast = await client.GetForecastOnDateAsync(region, new DateTimeOffset(2022, 4, 22, 0, 0, 0, TimeSpan.Zero));

        Assert.AreEqual(forecastResponse!.Meta.GeneratedAt, overloadedForecast!.Meta.GeneratedAt);
        Assert.AreEqual(forecastResponse.Data[0].Forecast.First(), overloadedForecast.Data[0].Forecast.First());

        Assert.AreEqual(WattTimeTestData.Constants.GeneratedAt, forecastResponse.Meta.GeneratedAt);
        Assert.AreEqual(WattTimeTestData.Constants.Region, forecastResponse.Meta.Region);

        var forecastDataPoint = forecastResponse.Data[0].Forecast.ToList().First();
        Assert.AreEqual(WattTimeTestData.Constants.PointTime, forecastDataPoint.PointTime);
        Assert.AreEqual(WattTimeTestData.Constants.Value.ToString("0.00", CultureInfo.InvariantCulture), forecastDataPoint.Value.ToString("0.00", CultureInfo.InvariantCulture)); //Format float to avoid precision issues
        Assert.AreEqual(WattTimeTestData.Constants.Version, forecastDataPoint.Version);
    }

    [Test]
    public async Task GetForecastOnDateAsync_RefreshesTokenWhenExpired()
    {
        this.SetupBasicHandlers(WattTimeTestData.GetHistoricalForecastDataJsonString(), "REFRESHTOKEN");

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object, this.MemoryCache);
        client.SetBearerAuthenticationHeader(_DEFAULT_TOKEN_VALUE);

        var forecastResponse = await client.GetForecastOnDateAsync(WattTimeTestData.Constants.Region, new DateTimeOffset());
        Assert.AreEqual(WattTimeTestData.Constants.GeneratedAt, forecastResponse!.Meta.GeneratedAt);
    }

    [Test]
    public async Task GetForecastOnDateAsync_RefreshesTokenWhenNoneSet()
    {
        // Override http client mock to remove authorization header
        Mock.Get(this.HttpClientFactory).Setup(x => x.CreateClient(IWattTimeClient.NamedClient))
            .Returns(() =>
            {
                var client = Handler.CreateClient();
                client.DefaultRequestHeaders.Authorization = null; // Null authorization header
                return client;
            });

        this.SetupBasicHandlers(WattTimeTestData.GetHistoricalForecastDataJsonString(), "REFRESHTOKEN");

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object, this.MemoryCache);

        var forecastResponse = await client.GetForecastOnDateAsync(WattTimeTestData.Constants.Region, new DateTimeOffset());

        Assert.AreEqual(WattTimeTestData.Constants.GeneratedAt, forecastResponse!.Meta.GeneratedAt);
    }

    [Test]
    public async Task GetRegionAsync_DeserializesExpectedResponse()
    {
        this.AddHandlers_Auth();
        this.AddHandler_RequestResponse(r =>
        {
            return r.RequestUri!.ToString().Equals("https://api.watttime.org/v3/region-from-loc?latitude=lat&longitude=long&signal_type=co2_moer") && r.Method == HttpMethod.Get;
        }, System.Net.HttpStatusCode.OK, WattTimeTestData.GetRegionJsonString());


        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object, this.MemoryCache);
        client.SetBearerAuthenticationHeader(_DEFAULT_TOKEN_VALUE);

        var regionResponse = await client.GetRegionAsync("lat", "long");

        Assert.IsNotNull(regionResponse);
        Assert.AreEqual(WattTimeTestData.Constants.Region, regionResponse?.Region);
        Assert.AreEqual(WattTimeTestData.Constants.RegionFullName, regionResponse?.RegionFullName);
        Assert.AreEqual(SignalTypes.co2_moer, regionResponse?.SignalType);
    }

    [Test]
    public async Task GetRegionAsync_RefreshesTokenWhenExpired()
    {
        this.SetupBasicHandlers(WattTimeTestData.GetRegionJsonString(), "REFRESHTOKEN");

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object, this.MemoryCache);
        client.SetBearerAuthenticationHeader(_DEFAULT_TOKEN_VALUE);

        var regionResponse = await client.GetRegionAsync("lat", "long");

        Assert.IsNotNull(regionResponse);
        Assert.AreEqual(WattTimeTestData.Constants.SignalType, regionResponse?.SignalType);
    }

    [Test]
    public async Task GetRegionAsync_RefreshesTokenWhenNoneSet()
    {
        // Override http client mock to remove authorization header
        Mock.Get(this.HttpClientFactory).Setup(x => x.CreateClient(IWattTimeClient.NamedClient))
            .Returns(() =>
            {
                var client = Handler.CreateClient();
                client.DefaultRequestHeaders.Authorization = null; // Null authorization header
                return client;
            });

        this.SetupBasicHandlers(WattTimeTestData.GetRegionJsonString(), "REFRESHTOKEN");

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object, this.MemoryCache);

        var regionResponse = await client.GetRegionAsync("lat", "long");

        Assert.IsNotNull(regionResponse);
        Assert.AreEqual(WattTimeTestData.Constants.SignalType, regionResponse?.SignalType);
    }

    [Test]
    public async Task GetHistoricalDataAsync_StreamsExpectedContent()
    {
        using (var testStream = new MemoryStream(Encoding.UTF8.GetBytes("myStreamResults")))
        {
            this.SetupBasicHandlers(new StreamContent(testStream));

            var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object, this.MemoryCache);
            client.SetBearerAuthenticationHeader(_DEFAULT_TOKEN_VALUE);

            var result = await client.GetHistoricalDataAsync(WattTimeTestData.Constants.Region);
            var sr = new StreamReader(result);
            string streamResult = sr.ReadToEnd();

            Assert.AreEqual("myStreamResults", streamResult);
        }
    }

    [Test]
    public async Task GetHistoricalDataAsync_RefreshesTokenWhenExpired()
    {
        using (var testStream = new MemoryStream(Encoding.UTF8.GetBytes("myStreamResults")))
        {
            // Override http client mock to remove authorization header
            Mock.Get(this.HttpClientFactory).Setup(x => x.CreateClient(IWattTimeClient.NamedClient))
                .Returns(() =>
                {
                    var client = Handler.CreateClient();
                    client.DefaultRequestHeaders.Authorization = null; // Null authorization header
                    return client;
                });

            this.SetupBasicHandlers(new StreamContent(testStream), "REFRESHTOKEN");

            var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object, this.MemoryCache);
            var result = await client.GetHistoricalDataAsync(WattTimeTestData.Constants.Region);
            var sr = new StreamReader(result);
            string streamResult = sr.ReadToEnd();

            Assert.AreEqual("myStreamResults", streamResult);
        }
    }

    [Test]
    public async Task GetHistoricalDataAsync_RefreshesTokenWhenNoneSet()
    {
        using (var testStream = new MemoryStream(Encoding.UTF8.GetBytes("myStreamResults")))
        {
            this.SetupBasicHandlers(new StreamContent(testStream), "REFRESHTOKEN");

            var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object, this.MemoryCache);
            client.SetBearerAuthenticationHeader(_DEFAULT_TOKEN_VALUE);

            var result = await client.GetHistoricalDataAsync(WattTimeTestData.Constants.Region);
            var sr = new StreamReader(result);
            string streamResult = sr.ReadToEnd();

            Assert.AreEqual("myStreamResults", streamResult);
        }
    }

    /**
    * Helper to add client handlers for auth checking
    */
    private void AddHandlers_Auth(string? validToken = null)
    {
        validToken ??= _DEFAULT_TOKEN_VALUE;

        AddHandler_RequestResponse(r =>
        {
            return r.Headers.Authorization == null;
        }, System.Net.HttpStatusCode.Unauthorized);

        AddHandler_RequestResponse(r =>
        {
            return (r.RequestUri == new Uri(_BASE_WATTTIME_LOGIN_URL) && ($"Basic {this.BasicAuthValue}".Equals(r.Headers.Authorization?.ToString())));
        }, System.Net.HttpStatusCode.OK, "{\"token\":\"" + validToken + "\"}");

        AddHandler_RequestResponse(r =>
        {
            return !(r.RequestUri == new Uri(_BASE_WATTTIME_LOGIN_URL) && ($"Basic {this.BasicAuthValue}".Equals(r.Headers.Authorization?.ToString()))) && r.Headers.Authorization?.ToString() != $"Bearer {validToken}";
        }, System.Net.HttpStatusCode.Forbidden);
    }


    /**
    * Helper to add client handlers for auth and basic content return
    */
    private void SetupBasicHandlers(StreamContent responseContent, string? validToken = null)
    {
        validToken ??= _DEFAULT_TOKEN_VALUE;

        AddHandlers_Auth(validToken);

        // Catch-all for "requesting url that is not login and has valid token"
        this.Handler
            .SetupRequest(r => r.RequestUri != new Uri(_BASE_WATTTIME_LOGIN_URL) && r.Headers.Authorization?.ToString() == $"Bearer {validToken}")
            .ReturnsResponse(System.Net.HttpStatusCode.OK, responseContent);
    }

    /**
    * Helper to add client handlers for auth and basic content return
    */
    private void SetupBasicHandlers(string responseContent, string? validToken = null)
    {
        validToken ??= _DEFAULT_TOKEN_VALUE;

        AddHandlers_Auth(validToken);

        // Catch-all for "requesting url that is not login and has valid token"
        AddHandler_RequestResponse(r => r.RequestUri != new Uri(_BASE_WATTTIME_LOGIN_URL) && r.Headers.Authorization?.ToString() == $"Bearer {validToken}", System.Net.HttpStatusCode.OK, responseContent);
    }

    /**
     * Helper to add client handler for request predicate and corresponding status code and response content
     */
    private void AddHandler_RequestResponse(Predicate<HttpRequestMessage> requestPredicate, System.Net.HttpStatusCode statusCode, string? responseContent = null)
    {
        if (responseContent != null)
        {
            this.Handler
                .SetupRequest(requestPredicate)
                .ReturnsResponse(statusCode, responseContent);
        }
        else
        {
            this.Handler
                .SetupRequest(requestPredicate)
                .ReturnsResponse(statusCode);
        }
    }
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
