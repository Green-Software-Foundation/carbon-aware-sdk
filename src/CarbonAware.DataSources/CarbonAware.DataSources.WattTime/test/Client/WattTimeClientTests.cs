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

    private readonly string DefaultTokenValue = "myDefaultToken123";

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
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.DefaultTokenValue);
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

        Assert.ThrowsAsync<WattTimeClientHttpException>(async () => await client.GetDataAsync(TestData.TestDataConstants.Region, new DateTimeOffset(), new DateTimeOffset()));
        Assert.ThrowsAsync<WattTimeClientHttpException>(async () => await client.GetCurrentForecastAsync(TestData.TestDataConstants.Region));
        Assert.ThrowsAsync<WattTimeClientHttpException>(async () => await client.GetForecastOnDateAsync(TestData.TestDataConstants.Region, new DateTimeOffset()));
        Assert.ThrowsAsync<WattTimeClientHttpException>(async () => await client.GetRegionAsync("lat", "long"));
        Assert.ThrowsAsync<WattTimeClientHttpException>(async () => await client.GetRegionAbbreviationAsync("lat", "long"));
        Assert.ThrowsAsync<WattTimeClientHttpException>(async () => await client.GetHistoricalDataAsync(TestData.TestDataConstants.Region));
    }

    [Test]
    public void AllPublicMethods_ThrowClientException_WhenNull()
    {
        this.SetupBasicHandlers("null");

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object, this.MemoryCache);
        client.SetBearerAuthenticationHeader(this.DefaultTokenValue);
        var region = new RegionResponse() { Region = "balauth" };

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
        client.SetBearerAuthenticationHeader(this.DefaultTokenValue);
        var region = new RegionResponse() { Region = TestData.TestDataConstants.Region };

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
            return r.RequestUri!.ToString().Equals("https://api.watttime.org/v3/historical?region=region&start=2022-04-22T00%3a00%3a00.0000000%2b00%3a00&end=2022-04-22T00%3a00%3a00.0000000%2b00%3a00&signal_type=co2_moer") && r.Method == HttpMethod.Get;
        }, System.Net.HttpStatusCode.OK, TestData.GetGridDataResponseJsonString());

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object, this.MemoryCache);
        client.SetBearerAuthenticationHeader(this.DefaultTokenValue);

        var emissionsResponse = await client.GetDataAsync("region", new DateTimeOffset(2022, 4, 22, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(2022, 4, 22, 0, 0, 0, TimeSpan.Zero));

        Assert.IsTrue(emissionsResponse.Data.Count() > 0);
        var meta = emissionsResponse.Meta;
        Assert.AreEqual(TestData.TestDataConstants.Region, meta.Region);
        Assert.AreEqual(TestData.TestDataConstants.SignalType, meta.SignalType);
        var gridDataPoint = emissionsResponse.Data.ToList().First();
        Assert.AreEqual(300, gridDataPoint.Frequency);
        Assert.AreEqual(TestData.TestDataConstants.Market, gridDataPoint.Market);
        Assert.AreEqual(TestData.TestDataConstants.PointTime, gridDataPoint.PointTime);
        Assert.AreEqual("999.99", gridDataPoint.Value.ToString("0.00", CultureInfo.InvariantCulture)); //Format float to avoid precision issues
        Assert.AreEqual("1.0", gridDataPoint.Version);
    }

    [Test]
    public async Task GetDataAsync_RefreshesTokenWhenExpired()
    {
        this.SetupBasicHandlers(TestData.GetGridDataResponseJsonString(), "REFRESHTOKEN");

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object, this.MemoryCache);
        client.SetBearerAuthenticationHeader(this.DefaultTokenValue);

        var emissionsResponse = await client.GetDataAsync("region", new DateTimeOffset(), new DateTimeOffset());

        Assert.IsTrue(emissionsResponse.Data.Count() > 0);
        Assert.AreEqual(TestData.TestDataConstants.Region, emissionsResponse.Meta.Region);
    }

    [Test]
    public async Task GetDataAsync_RefreshesTokenWhenNoneSet()
    {
        this.SetupBasicHandlers(TestData.GetGridDataResponseJsonString(), "REFRESHTOKEN");

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object, this.MemoryCache);

        var gridEmissionsResponse = await client.GetDataAsync("region", new DateTimeOffset(), new DateTimeOffset());

        Assert.IsTrue(gridEmissionsResponse.Data.Count() > 0);
        Assert.AreEqual(TestData.TestDataConstants.Region, gridEmissionsResponse.Meta.Region);
    }

    [Test]
    public async Task GetCurrentForecastAsync_DeserializesExpectedResponse()
    {
        this.AddHandlers_Auth();
        this.AddHandler_RequestResponse(r =>
        {
            return r.RequestUri!.ToString().Equals(@"https://api.watttime.org/v3/forecast?region=region&signal_type=co2_moer") && r.Method == HttpMethod.Get;
        }, System.Net.HttpStatusCode.OK, TestData.GetCurrentForecastJsonString());

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object, this.MemoryCache);
        client.SetBearerAuthenticationHeader(this.DefaultTokenValue);

        const string REGION = "region";

        var forecastResponse = await client.GetCurrentForecastAsync(REGION);
        var overloadedForecast = await client.GetCurrentForecastAsync(REGION);

        Assert.AreEqual(forecastResponse.Meta.GeneratedAt, overloadedForecast.Meta.GeneratedAt);
        Assert.AreEqual(forecastResponse.Data.First(), overloadedForecast.Data.First());

        Assert.IsNotNull(forecastResponse);
        Assert.AreEqual(new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero), forecastResponse?.Meta.GeneratedAt);
        Assert.AreEqual(TestData.TestDataConstants.Region, forecastResponse?.Meta.Region);
        var forecastDataPoint = forecastResponse?.Data.First();

        Assert.AreEqual(new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero), forecastDataPoint?.PointTime);
        Assert.AreEqual("999.99", forecastDataPoint?.Value.ToString("0.00", CultureInfo.InvariantCulture)); //Format float to avoid precision issues
        Assert.AreEqual("1.0", forecastDataPoint?.Version);
    }

    [Test]
    public async Task GetCurrentForecastAsync_RefreshesTokenWhenExpired()
    {
        this.SetupBasicHandlers(TestData.GetCurrentForecastJsonString(), "REFRESHTOKEN");

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object, this.MemoryCache);
        client.SetBearerAuthenticationHeader(this.DefaultTokenValue);

        var forecastResponse = await client.GetCurrentForecastAsync("region");

        Assert.IsNotNull(forecastResponse);
        Assert.AreEqual(new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero), forecastResponse?.Meta.GeneratedAt);
        Assert.AreEqual(TestData.TestDataConstants.Region, forecastResponse?.Meta.Region);
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
        this.SetupBasicHandlers(TestData.GetCurrentForecastJsonString(), "REFRESHTOKEN");

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object, this.MemoryCache);

        var forecastResponse = await client.GetCurrentForecastAsync("region");

        Assert.IsNotNull(forecastResponse);
        Assert.AreEqual(new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero), forecastResponse?.Meta.GeneratedAt);
        Assert.AreEqual(TestData.TestDataConstants.Region, forecastResponse?.Meta.Region);
    }

    [Test]
    public async Task GetForecastOnDateAsync_DeserializesExpectedResponse()
    {
        this.AddHandlers_Auth();
        this.AddHandler_RequestResponse(r =>
        {
            return r.RequestUri!.ToString().Equals("https://api.watttime.org/v3/forecast/historical?region=region&start=2022-04-22T00%3a00%3a00.0000000%2b00%3a00&end=2022-04-22T00%3a00%3a00.0000000%2b00%3a00&signal_type=co2_moer") && r.Method == HttpMethod.Get;
        }, System.Net.HttpStatusCode.OK, TestData.GetHistoricalForecastDataJsonString());

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object, this.MemoryCache);
        client.SetBearerAuthenticationHeader(this.DefaultTokenValue);
        var region = new RegionResponse() { Region = "region" };

        var forecastResponse = await client.GetForecastOnDateAsync(region.Region, new DateTimeOffset(2022, 4, 22, 0, 0, 0, TimeSpan.Zero));
        var overloadedForecast = await client.GetForecastOnDateAsync(region, new DateTimeOffset(2022, 4, 22, 0, 0, 0, TimeSpan.Zero));

        Assert.AreEqual(forecastResponse!.Meta.GeneratedAt, overloadedForecast!.Meta.GeneratedAt);
        Assert.AreEqual(forecastResponse.Data[0].Forecast.First(), overloadedForecast.Data[0].Forecast.First());

        Assert.AreEqual(new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero), forecastResponse.Meta.GeneratedAt);
        Assert.AreEqual(TestData.TestDataConstants.Region, forecastResponse.Meta.Region);

        var forecastDataPoint = forecastResponse.Data[0].Forecast.ToList().First();
        Assert.AreEqual(TestData.TestDataConstants.PointTime, forecastDataPoint.PointTime);
        Assert.AreEqual(TestData.TestDataConstants.Value.ToString("0.00", CultureInfo.InvariantCulture), forecastDataPoint.Value.ToString("0.00", CultureInfo.InvariantCulture)); //Format float to avoid precision issues
        Assert.AreEqual("1.0", forecastDataPoint.Version);
    }

    [Test]
    public async Task GetForecastOnDateAsync_RefreshesTokenWhenExpired()
    {
        this.SetupBasicHandlers(TestData.GetHistoricalForecastDataJsonString(), "REFRESHTOKEN");

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object, this.MemoryCache);
        client.SetBearerAuthenticationHeader(this.DefaultTokenValue);

        var forecastResponse = await client.GetForecastOnDateAsync("region", new DateTimeOffset());
        Assert.AreEqual(new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero), forecastResponse!.Meta.GeneratedAt);
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

        this.SetupBasicHandlers(TestData.GetHistoricalForecastDataJsonString(), "REFRESHTOKEN");

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object, this.MemoryCache);

        var forecastResponse = await client.GetForecastOnDateAsync("region", new DateTimeOffset());

        Assert.AreEqual(new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero), forecastResponse!.Meta.GeneratedAt);
    }

    [Test]
    public async Task GetRegionAsync_DeserializesExpectedResponse()
    {
        this.AddHandlers_Auth();
        this.AddHandler_RequestResponse(r =>
        {
            return r.RequestUri!.ToString().Equals("https://api.watttime.org/v3/region-from-loc?latitude=lat&longitude=long&signal_type=co2_moer") && r.Method == HttpMethod.Get;
        }, System.Net.HttpStatusCode.OK, TestData.GetRegionJsonString());


        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object, this.MemoryCache);
        client.SetBearerAuthenticationHeader(this.DefaultTokenValue);

        var regionResponse = await client.GetRegionAsync("lat", "long");

        Assert.IsNotNull(regionResponse);
        Assert.AreEqual(TestData.TestDataConstants.Region, regionResponse?.Region);
        Assert.AreEqual(TestData.TestDataConstants.RegionFullName, regionResponse?.RegionFullName);
        Assert.AreEqual(SignalTypes.co2_moer, regionResponse?.SignalType);
    }

    [Test]
    public async Task GetRegionAsync_RefreshesTokenWhenExpired()
    {
        this.SetupBasicHandlers(TestData.GetRegionJsonString(), "REFRESHTOKEN");

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object, this.MemoryCache);
        client.SetBearerAuthenticationHeader(this.DefaultTokenValue);

        var regionResponse = await client.GetRegionAsync("lat", "long");

        Assert.IsNotNull(regionResponse);
        Assert.AreEqual(SignalTypes.co2_moer, regionResponse?.SignalType);
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

        this.SetupBasicHandlers(TestData.GetRegionJsonString(), "REFRESHTOKEN");

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object, this.MemoryCache);

        var regionResponse = await client.GetRegionAsync("lat", "long");

        Assert.IsNotNull(regionResponse);
        Assert.AreEqual(SignalTypes.co2_moer, regionResponse?.SignalType);
    }

    [Test]
    public async Task GetHistoricalDataAsync_StreamsExpectedContent()
    {
        using (var testStream = new MemoryStream(Encoding.UTF8.GetBytes("myStreamResults")))
        {
            this.SetupBasicHandlers(new StreamContent(testStream));

            var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object, this.MemoryCache);
            client.SetBearerAuthenticationHeader(this.DefaultTokenValue);

            var result = await client.GetHistoricalDataAsync(TestData.TestDataConstants.Region);
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

            var result = await client.GetHistoricalDataAsync(TestData.TestDataConstants.Region);
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
            client.SetBearerAuthenticationHeader(this.DefaultTokenValue);

            var result = await client.GetHistoricalDataAsync(TestData.TestDataConstants.Region);
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
        validToken ??= this.DefaultTokenValue;

        AddHandler_RequestResponse(r =>
        {
            return r.Headers.Authorization == null;
        }, System.Net.HttpStatusCode.Unauthorized);

        AddHandler_RequestResponse(r =>
        {
            return (r.RequestUri == new Uri("https://api.watttime.org/login") && ($"Basic {this.BasicAuthValue}".Equals(r.Headers.Authorization?.ToString())));
        }, System.Net.HttpStatusCode.OK, "{\"token\":\"" + validToken + "\"}");

        AddHandler_RequestResponse(r =>
        {
            return !(r.RequestUri == new Uri("https://api.watttime.org/login") && ($"Basic {this.BasicAuthValue}".Equals(r.Headers.Authorization?.ToString()))) && r.Headers.Authorization?.ToString() != $"Bearer {validToken}";
        }, System.Net.HttpStatusCode.Forbidden);
    }

    /**
    * Helper to add client handlers for auth and basic content return
    */
    private void SetupBasicHandlers(StreamContent responseContent, string? validToken = null)
    {
        validToken ??= this.DefaultTokenValue;

        AddHandlers_Auth(validToken);

        // Catch-all for "requesting url that is not login and has valid token"
        this.Handler
            .SetupRequest(r => r.RequestUri != new Uri("https://api.watttime.org/login") && r.Headers.Authorization?.ToString() == $"Bearer {validToken}")
            .ReturnsResponse(System.Net.HttpStatusCode.OK, responseContent);
    }

    /**
    * Helper to add client handlers for auth and basic content return
    */
    private void SetupBasicHandlers(string responseContent, string? validToken = null)
    {
        validToken ??= this.DefaultTokenValue;

        AddHandlers_Auth(validToken);

        // Catch-all for "requesting url that is not login and has valid token"
        AddHandler_RequestResponse(r => r.RequestUri != new Uri("https://api.watttime.org/login") && r.Headers.Authorization?.ToString() == $"Bearer {validToken}", System.Net.HttpStatusCode.OK, responseContent);
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
