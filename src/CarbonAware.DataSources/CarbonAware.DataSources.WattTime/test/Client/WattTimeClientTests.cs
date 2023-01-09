using CarbonAware.DataSources.WattTime.Configuration;
using CarbonAware.DataSources.WattTime.Model;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Contrib.HttpClient;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CarbonAware.DataSources.WattTime.Client.Tests;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
public class WattTimeClientTests
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

        this.BasicAuthValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{this.Configuration.Username}:{this.Configuration.Password}"));

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

        Assert.ThrowsAsync<WattTimeClientHttpException>(async () => await client.GetDataAsync("ba", new DateTimeOffset(), new DateTimeOffset()));
        Assert.ThrowsAsync<WattTimeClientHttpException>(async () => await client.GetCurrentForecastAsync("ba"));
        Assert.ThrowsAsync<WattTimeClientHttpException>(async () => await client.GetForecastOnDateAsync("ba", new DateTimeOffset()));
        Assert.ThrowsAsync<WattTimeClientHttpException>(async () => await client.GetBalancingAuthorityAsync("lat", "long"));
        Assert.ThrowsAsync<WattTimeClientHttpException>(async () => await client.GetBalancingAuthorityAbbreviationAsync("lat", "long"));
        Assert.ThrowsAsync<WattTimeClientHttpException>(async () => await client.GetHistoricalDataAsync("ba"));
    }

    [Test]
    public void AllPublicMethods_ThrowClientException_WhenNull()
    {
        this.SetupBasicHandlers("null");

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object, this.MemoryCache);
        client.SetBearerAuthenticationHeader(this.DefaultTokenValue);
        var ba = new BalancingAuthority() { Abbreviation = "balauth" };

        Assert.ThrowsAsync<WattTimeClientException>(async () => await client.GetBalancingAuthorityAsync("lat", "long"));
        Assert.ThrowsAsync<WattTimeClientException>(async () => await client.GetDataAsync(ba.Abbreviation, new DateTimeOffset(), new DateTimeOffset()));
        Assert.ThrowsAsync<WattTimeClientException>(async () => await client.GetDataAsync(ba, new DateTimeOffset(), new DateTimeOffset()));
        Assert.ThrowsAsync<WattTimeClientException>(async () => await client.GetCurrentForecastAsync(ba.Abbreviation));
        Assert.ThrowsAsync<WattTimeClientException>(async () => await client.GetCurrentForecastAsync(ba));
        Assert.ThrowsAsync<WattTimeClientException>(async () => await client.GetForecastOnDateAsync(ba.Abbreviation, new DateTimeOffset()));
        Assert.ThrowsAsync<WattTimeClientException>(async () => await client.GetForecastOnDateAsync(ba, new DateTimeOffset()));
    }

    [Test]
    public void AllPublicMethods_ThrowJsonException_WhenBadJsonIsReturned()
    {
        this.SetupBasicHandlers("This is bad json");

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object, this.MemoryCache);
        client.SetBearerAuthenticationHeader(this.DefaultTokenValue);
        var ba = new BalancingAuthority() { Abbreviation = "balauth" };

        Assert.ThrowsAsync<JsonException>(async () => await client.GetBalancingAuthorityAsync("lat", "long"));
        Assert.ThrowsAsync<JsonException>(async () => await client.GetDataAsync(ba.Abbreviation, new DateTimeOffset(), new DateTimeOffset()));
        Assert.ThrowsAsync<JsonException>(async () => await client.GetDataAsync(ba, new DateTimeOffset(), new DateTimeOffset()));
        Assert.ThrowsAsync<JsonException>(async () => await client.GetCurrentForecastAsync(ba.Abbreviation));
        Assert.ThrowsAsync<JsonException>(async () => await client.GetCurrentForecastAsync(ba));
        Assert.ThrowsAsync<JsonException>(async () => await client.GetForecastOnDateAsync(ba.Abbreviation, new DateTimeOffset()));
        Assert.ThrowsAsync<JsonException>(async () => await client.GetForecastOnDateAsync(ba, new DateTimeOffset()));
    }

    [Test]
    public async Task GetDataAsync_DeserializesExpectedResponse()
    {
        this.AddHandlers_Auth();
        this.AddHandler_RequestResponse(r =>
        {
            return r.RequestUri!.ToString().Equals("https://api2.watttime.org/v2/data?ba=balauth&starttime=2022-04-22T00%3a00%3a00.0000000%2b00%3a00&endtime=2022-04-22T00%3a00%3a00.0000000%2b00%3a00") && r.Method == HttpMethod.Get;
        }, System.Net.HttpStatusCode.OK, TestData.GetGridDataJsonString());

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object, this.MemoryCache);
        client.SetBearerAuthenticationHeader(this.DefaultTokenValue);

        var data = await client.GetDataAsync("balauth", new DateTimeOffset(2022, 4, 22, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(2022, 4, 22, 0, 0, 0, TimeSpan.Zero));

        Assert.IsTrue(data.Count() > 0);
        var gridDataPoint = data.ToList().First();
        Assert.AreEqual("ba", gridDataPoint.BalancingAuthorityAbbreviation);
        Assert.AreEqual("dt", gridDataPoint.Datatype);
        Assert.AreEqual(300, gridDataPoint.Frequency);
        Assert.AreEqual("mkt", gridDataPoint.Market);
        Assert.AreEqual(new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero), gridDataPoint.PointTime);
        Assert.AreEqual("999.99", gridDataPoint.Value.ToString("0.00")); //Format float to avoid precision issues
        Assert.AreEqual("1.0", gridDataPoint.Version);
    }

    [Test]
    public async Task GetDataAsync_RefreshesTokenWhenExpired()
    {
        this.SetupBasicHandlers(TestData.GetGridDataJsonString(), "REFRESHTOKEN");

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object, this.MemoryCache);
        client.SetBearerAuthenticationHeader(this.DefaultTokenValue);

        var data = await client.GetDataAsync("balauth", new DateTimeOffset(), new DateTimeOffset());

        Assert.IsTrue(data.Count() > 0);
        var gridDataPoint = data.ToList().First();
        Assert.AreEqual("ba", gridDataPoint.BalancingAuthorityAbbreviation);
    }

    [Test]
    public async Task GetDataAsync_RefreshesTokenWhenNoneSet()
    {
        this.SetupBasicHandlers(TestData.GetGridDataJsonString(), "REFRESHTOKEN");

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object, this.MemoryCache);

        var data = await client.GetDataAsync("balauth", new DateTimeOffset(), new DateTimeOffset());

        Assert.IsTrue(data.Count() > 0);
        var gridDataPoint = data.ToList().First();
        Assert.AreEqual("ba", gridDataPoint.BalancingAuthorityAbbreviation);
    }

    [Test]
    public async Task GetCurrentForecastAsync_DeserializesExpectedResponse()
    {
        this.AddHandlers_Auth();
        this.AddHandler_RequestResponse(r =>
        {
            return r.RequestUri!.ToString().Equals("https://api2.watttime.org/v2/forecast?ba=balauth") && r.Method == HttpMethod.Get;
        }, System.Net.HttpStatusCode.OK, TestData.GetCurrentForecastJsonString());

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object, this.MemoryCache);
        client.SetBearerAuthenticationHeader(this.DefaultTokenValue);

        var ba = new BalancingAuthority() { Abbreviation = "balauth" };

        var forecast = await client.GetCurrentForecastAsync(ba.Abbreviation);
        var overloadedForecast = await client.GetCurrentForecastAsync(ba);

        Assert.AreEqual(forecast.GeneratedAt, overloadedForecast.GeneratedAt);
        Assert.AreEqual(forecast.ForecastData.First(), overloadedForecast.ForecastData.First());

        Assert.IsNotNull(forecast);
        Assert.AreEqual(new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero), forecast?.GeneratedAt);
        var forecastDataPoint = forecast?.ForecastData.First();
        Assert.AreEqual("ba", forecastDataPoint?.BalancingAuthorityAbbreviation);
        Assert.AreEqual(new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero), forecastDataPoint?.PointTime);
        Assert.AreEqual("999.99", forecastDataPoint?.Value.ToString("0.00")); //Format float to avoid precision issues
        Assert.AreEqual("1.0", forecastDataPoint?.Version);
    }

    [Test]
    public async Task GetCurrentForecastAsync_RefreshesTokenWhenExpired()
    {
        this.SetupBasicHandlers(TestData.GetCurrentForecastJsonString(), "REFRESHTOKEN");

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object, this.MemoryCache);
        client.SetBearerAuthenticationHeader(this.DefaultTokenValue);

        var forecast = await client.GetCurrentForecastAsync("balauth");

        Assert.IsNotNull(forecast);
        Assert.AreEqual(new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero), forecast?.GeneratedAt);
        var forecastDataPoint = forecast?.ForecastData.First();
        Assert.AreEqual("ba", forecastDataPoint?.BalancingAuthorityAbbreviation);
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

        var forecast = await client.GetCurrentForecastAsync("balauth");

        Assert.IsNotNull(forecast);
        Assert.AreEqual(new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero), forecast?.GeneratedAt);
        var forecastDataPoint = forecast?.ForecastData.First();
        Assert.AreEqual("ba", forecastDataPoint?.BalancingAuthorityAbbreviation);
    }

    [Test]
    public async Task GetForecastOnDateAsync_DeserializesExpectedResponse()
    {
        this.AddHandlers_Auth();
        this.AddHandler_RequestResponse(r =>
        {
            return r.RequestUri!.ToString().Equals("https://api2.watttime.org/v2/forecast?ba=balauth&starttime=2022-04-22T00%3a00%3a00.0000000%2b00%3a00&endtime=2022-04-22T00%3a00%3a00.0000000%2b00%3a00") && r.Method == HttpMethod.Get;
        }, System.Net.HttpStatusCode.OK, TestData.GetForecastByDateJsonString());

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object, this.MemoryCache);
        client.SetBearerAuthenticationHeader(this.DefaultTokenValue);
        var ba = new BalancingAuthority() { Abbreviation = "balauth" };

        var forecast = await client.GetForecastOnDateAsync(ba.Abbreviation, new DateTimeOffset(2022, 4, 22, 0, 0, 0, TimeSpan.Zero));
        var overloadedForecast = await client.GetForecastOnDateAsync(ba, new DateTimeOffset(2022, 4, 22, 0, 0, 0, TimeSpan.Zero));

        Assert.AreEqual(forecast!.GeneratedAt, overloadedForecast!.GeneratedAt);
        Assert.AreEqual(forecast.ForecastData.First(), overloadedForecast.ForecastData.First());

        Assert.AreEqual(new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero), forecast.GeneratedAt);
        var forecastDataPoint = forecast.ForecastData.ToList().First();
        Assert.AreEqual("ba", forecastDataPoint.BalancingAuthorityAbbreviation);
        Assert.AreEqual(new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero), forecastDataPoint.PointTime);
        Assert.AreEqual("999.99", forecastDataPoint.Value.ToString("0.00")); //Format float to avoid precision issues
        Assert.AreEqual("1.0", forecastDataPoint.Version);
    }

    [Test]
    public async Task GetForecastOnDateAsync_RefreshesTokenWhenExpired()
    {
        this.SetupBasicHandlers(TestData.GetForecastByDateJsonString(), "REFRESHTOKEN");

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object, this.MemoryCache);
        client.SetBearerAuthenticationHeader(this.DefaultTokenValue);

        var forecast = await client.GetForecastOnDateAsync("balauth", new DateTimeOffset());
        Assert.AreEqual(new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero), forecast!.GeneratedAt);
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

        this.SetupBasicHandlers(TestData.GetForecastByDateJsonString(), "REFRESHTOKEN");

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object, this.MemoryCache);

        var forecast = await client.GetForecastOnDateAsync("balauth", new DateTimeOffset());

        Assert.AreEqual(new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero), forecast!.GeneratedAt);
    }

    [Test]
    public async Task GetBalancingAuthorityAsync_DeserializesExpectedResponse()
    {
        this.AddHandlers_Auth();
        this.AddHandler_RequestResponse(r =>
        {
            return r.RequestUri!.ToString().Equals("https://api2.watttime.org/v2/ba-from-loc?latitude=lat&longitude=long") && r.Method == HttpMethod.Get;
        }, System.Net.HttpStatusCode.OK, TestData.GetBalancingAuthorityJsonString());

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object, this.MemoryCache);
        client.SetBearerAuthenticationHeader(this.DefaultTokenValue);

        var ba = await client.GetBalancingAuthorityAsync("lat", "long");

        Assert.IsNotNull(ba);
        Assert.AreEqual(12345, ba?.Id);
        Assert.AreEqual("TEST_BA", ba?.Abbreviation);
        Assert.AreEqual("Test Balancing Authority", ba?.Name);
    }

    [Test]
    public async Task GetBalancingAuthorityAsync_RefreshesTokenWhenExpired()
    {
        this.SetupBasicHandlers(TestData.GetBalancingAuthorityJsonString(), "REFRESHTOKEN");

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object, this.MemoryCache);
        client.SetBearerAuthenticationHeader(this.DefaultTokenValue);

        var ba = await client.GetBalancingAuthorityAsync("lat", "long");

        Assert.IsNotNull(ba);
        Assert.AreEqual(12345, ba?.Id);
    }

    [Test]
    public async Task GetBalancingAuthorityAsync_RefreshesTokenWhenNoneSet()
    {
        // Override http client mock to remove authorization header
        Mock.Get(this.HttpClientFactory).Setup(x => x.CreateClient(IWattTimeClient.NamedClient))
            .Returns(() =>
            {
                var client = Handler.CreateClient();
                client.DefaultRequestHeaders.Authorization = null; // Null authorization header
                return client;
            });

        this.SetupBasicHandlers(TestData.GetBalancingAuthorityJsonString(), "REFRESHTOKEN");

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object, this.MemoryCache);

        var ba = await client.GetBalancingAuthorityAsync("lat", "long");

        Assert.IsNotNull(ba);
        Assert.AreEqual(12345, ba?.Id);
    }

    [Test]
    public async Task GetHistoricalDataAsync_StreamsExpectedContent()
    {
        using (var testStream = new MemoryStream(Encoding.UTF8.GetBytes("myStreamResults")))
        {
            this.SetupBasicHandlers(new StreamContent(testStream));

            var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object, this.MemoryCache);
            client.SetBearerAuthenticationHeader(this.DefaultTokenValue);

            var result = await client.GetHistoricalDataAsync("ba");
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

            var result = await client.GetHistoricalDataAsync("ba");
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

            var result = await client.GetHistoricalDataAsync("ba");
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
            return (r.RequestUri == new Uri("https://api2.watttime.org/v2/login") && ($"Basic {this.BasicAuthValue}".Equals(r.Headers.Authorization?.ToString())));
        }, System.Net.HttpStatusCode.OK, "{\"token\":\"" + validToken + "\"}");

        AddHandler_RequestResponse(r =>
        {
            return !(r.RequestUri == new Uri("https://api2.watttime.org/v2/login") && ($"Basic {this.BasicAuthValue}".Equals(r.Headers.Authorization?.ToString()))) && r.Headers.Authorization?.ToString() != $"Bearer {validToken}";
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
            .SetupRequest(r => r.RequestUri != new Uri("https://api2.watttime.org/v2/login") && r.Headers.Authorization?.ToString() == $"Bearer {validToken}")
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
        AddHandler_RequestResponse(r => r.RequestUri != new Uri("https://api2.watttime.org/v2/login") && r.Headers.Authorization?.ToString() == $"Bearer {validToken}", System.Net.HttpStatusCode.OK, responseContent);
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
