using CarbonAware.DataSources.ElectricityMaps.Client;
using CarbonAware.DataSources.ElectricityMaps.Configuration;
using CarbonAware.DataSources.ElectricityMaps.Constants;
using CarbonAware.DataSources.ElectricityMaps.Model;
using CarbonAware.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Moq.Contrib.HttpClient;
using System.Text.Json;

namespace CarbonAware.DataSources.ElectricityMaps.Tests;

[TestFixture]
public class ElectricityMapsClientTests
{
    private readonly string AuthHeader = "auth-token";
    private readonly string DefaultTokenValue = "myDefaultToken123";
    private readonly string TestLatitude = "36.6681";
    private readonly string TestLongitude = "-78.3889";
    private readonly string TestZone = "NL";

    private Mock<HttpMessageHandler> Handler { get; set; }

    private IHttpClientFactory HttpClientFactory { get; set; }

    private ElectricityMapsClientConfiguration Configuration { get; set; }

    private Mock<IOptionsMonitor<ElectricityMapsClientConfiguration>> Options { get; set; }

    private Mock<ILogger<ElectricityMapsClient>> Log { get; set; }

    [SetUp]
    public void Setup()
    {
        this.Configuration = new ElectricityMapsClientConfiguration() { APITokenHeader = AuthHeader, APIToken = DefaultTokenValue };

        this.Options = new Mock<IOptionsMonitor<ElectricityMapsClientConfiguration>>();
        this.Log = new Mock<ILogger<ElectricityMapsClient>>();

        this.Options.Setup(o => o.CurrentValue).Returns(() => this.Configuration);

        this.Handler = new Mock<HttpMessageHandler>();
        this.HttpClientFactory = Handler.CreateClientFactory();
        Mock.Get(this.HttpClientFactory).Setup(x => x.CreateClient(IElectricityMapsClient.NamedClient))
            .Returns(() =>
            {
                var client = Handler.CreateClient();
                return client;
            });
    }

    [TestCase(BaseUrls.TrialBaseUrl, TestName = "ClientInstantiation_FailsForInvalidConfig: Trial")]
    [TestCase(BaseUrls.TokenBaseUrl, TestName = "ClientInstantiation_FailsForInvalidConfig: Token")]
    public void ClientInstantiation_FailsForInvalidConfig(string baseUrl)
    {
        // Arrange
        AddHandlers_Auth_Zones(TestData.GetZonesAllowedJsonString());
        this.Configuration = new ElectricityMapsClientConfiguration()
        {
            APITokenHeader = string.Empty,
            APIToken = string.Empty,
            BaseUrl = baseUrl,
        };

        // Act & Assert
        Assert.Throws<ConfigurationException>(() => new ElectricityMapsClient(this.HttpClientFactory, this.Options.Object, this.Log.Object));
    }

    [TestCase(BaseUrls.TrialBaseUrl, TestName = "ClientInstantiation_SucceedsForValidConfig: Trial")]
    [TestCase(BaseUrls.TokenBaseUrl, TestName = "ClientInstantiation_SucceedsForValidConfig: Token")]
    public void ClientInstantiation_SucceedsForValidConfig(string baseUrl)
    {
        // Arrange
        AddHandlers_Auth_Zones(TestData.GetZonesAllowedJsonString());
        this.Configuration.BaseUrl = baseUrl;

        // Act & Assert
        Assert.DoesNotThrow(() => new ElectricityMapsClient(this.HttpClientFactory, this.Options.Object, this.Log.Object));
    }

    [Test]
    public void AllPublicMethods_DoNotSwallowBadProxyExceptions()
    {
        // Arrange
        var mockHttpClientFactory = Mock.Of<IHttpClientFactory>();
        var mockHandler = new Mock<HttpClientHandler>();

        // A bad proxy will throw HttpRequestException when used so we mock that here.
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new HttpRequestException());

        Mock.Get(mockHttpClientFactory)
            .Setup(h => h.CreateClient(It.IsAny<string>()))
            .Returns(new HttpClient(mockHandler.Object));

        var client = new ElectricityMapsClient(mockHttpClientFactory, this.Options.Object, this.Log.Object);

        // Act & Assert
        Assert.ThrowsAsync<HttpRequestException>(async () => await client.GetForecastedCarbonIntensityAsync(TestZone));
        Assert.ThrowsAsync<HttpRequestException>(async () => await client.GetForecastedCarbonIntensityAsync(TestLatitude, TestLongitude));
        Assert.ThrowsAsync<HttpRequestException>(async () => await client.GetRecentCarbonIntensityHistoryAsync(TestZone));
        Assert.ThrowsAsync<HttpRequestException>(async () => await client.GetRecentCarbonIntensityHistoryAsync(TestLatitude, TestLongitude));
    }

    [Test]
    public void AllPublicMethods_ThrowsClientException_WhenNull()
    {
        // Arrange
        AddHandlers_Auth_Zones(TestData.GetZonesAllowedJsonString());
        AddHandler_RequestResponse(r =>
        {
            return r.RequestUri!.ToString().Contains(Paths.Forecast) && r.Method == HttpMethod.Get;
        }, System.Net.HttpStatusCode.OK, "null");

        AddHandler_RequestResponse(r =>
        {
            return r.RequestUri!.ToString().Contains(Paths.History) && r.Method == HttpMethod.Get;
        }, System.Net.HttpStatusCode.OK, "null");

        var client = new ElectricityMapsClient(this.HttpClientFactory, this.Options.Object, this.Log.Object);

        // Act & Assert
        Assert.ThrowsAsync<ElectricityMapsClientException>(async () => await client.GetForecastedCarbonIntensityAsync(TestLatitude, TestLongitude));
        Assert.ThrowsAsync<ElectricityMapsClientException>(async () => await client.GetRecentCarbonIntensityHistoryAsync(TestLatitude, TestLongitude));
    }

    [Test]
    public void AllPublicMethods_ThrowJsonException_WhenBadJsonIsReturned()
    {
        // Arrange
        AddHandlers_Auth_Zones(TestData.GetZonesAllowedJsonString());
        AddHandler_RequestResponse(r =>
        {
            return r.RequestUri!.ToString().Contains(Paths.Forecast) && r.Method == HttpMethod.Get;
        }, System.Net.HttpStatusCode.OK, "This is bad json");

        AddHandler_RequestResponse(r =>
        {
            return r.RequestUri!.ToString().Contains(Paths.History) && r.Method == HttpMethod.Get;
        }, System.Net.HttpStatusCode.OK, "This is bad json");

        var client = new ElectricityMapsClient(this.HttpClientFactory, this.Options.Object, this.Log.Object);

        // Act & Assert
        Assert.ThrowsAsync<JsonException>(async () => await client.GetForecastedCarbonIntensityAsync(TestLatitude, TestLongitude));
        Assert.ThrowsAsync<JsonException>(async () => await client.GetRecentCarbonIntensityHistoryAsync(TestLatitude, TestLongitude));
    }

    [Test]
    public void GetForecastedCarbonIntensityAsync_ThrowsWhen_PathNotSupported()
    {
        // Arrange
        AddHandlers_Auth_Zones(TestData.GetNoPathsSupportedJsonString());
        var client = new ElectricityMapsClient(this.HttpClientFactory, this.Options.Object, this.Log.Object);

        // Act & Assert
        Assert.ThrowsAsync<ElectricityMapsClientException>(async () => await client.GetForecastedCarbonIntensityAsync(TestZone));
    }

    [Test]
    public void GetForecastedCarbonIntensityAsync_ThrowsWhen_ZoneNotSupported()
    {
        // Arrange
        AddHandlers_Auth_Zones(TestData.GetNoZonesSupportedJsonString());
        var client = new ElectricityMapsClient(this.HttpClientFactory, this.Options.Object, this.Log.Object);

        // Act & Assert
        Assert.ThrowsAsync<ElectricityMapsClientException>(async () => await client.GetForecastedCarbonIntensityAsync(TestZone));
    }


    [TestCase(BaseUrls.TrialBaseUrl, TestName = "GetForecastedCarbonIntensityAsync_DeserializesExpectedResponse: Trial")]
    [TestCase(BaseUrls.TokenBaseUrl, TestName = "GetForecastedCarbonIntensityAsync_DeserializesExpectedResponse: Token")]
    public async Task GetForecastedCarbonIntensityAsync_DeserializesExpectedResponse(string baseUrl)
    {
        // Arrange
        this.Configuration.BaseUrl = baseUrl;
        AddHandlers_Auth_Zones(TestData.GetZonesAllowedJsonString());
        AddHandler_RequestResponse(r =>
            {
                return r.RequestUri!.ToString().Contains(baseUrl + Paths.Forecast) && r.Method == HttpMethod.Get;
            }, System.Net.HttpStatusCode.OK, TestData.GetCurrentForecastJsonString());

        var client = new ElectricityMapsClient(this.HttpClientFactory, this.Options.Object, this.Log.Object);

        // Act
        var forecast = await client.GetForecastedCarbonIntensityAsync(TestLatitude, TestLongitude);

        // Assert
        var forecastDataPoint = forecast?.ForecastData.First();
        Assert.That(forecast, Is.Not.Null);
        Assert.That(forecast?.UpdatedAt, Is.EqualTo(new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero)));
        Assert.That(forecast?.Zone, Is.EqualTo(TestZone));
        Assert.That(forecastDataPoint?.DateTime, Is.EqualTo(new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero)));
        Assert.That(forecastDataPoint?.CarbonIntensity, Is.EqualTo(999));
    }

    [Test]
    public void GetRecentCarbonIntensityHistoryAsync_ThrowsWhen_PathNotSupported()
    {
        // Arrange
        AddHandlers_Auth_Zones(TestData.GetNoPathsSupportedJsonString());
        var client = new ElectricityMapsClient(this.HttpClientFactory, this.Options.Object, this.Log.Object);

        // Act & Assert
        Assert.ThrowsAsync<ElectricityMapsClientException>(async () => await client.GetRecentCarbonIntensityHistoryAsync(TestZone));
    }

    [Test]
    public void GetRecentCarbonIntensityHistoryAsync_ThrowsWhen_ZoneNotSupported()
    {
        // Arrange
        AddHandlers_Auth_Zones(TestData.GetNoZonesSupportedJsonString());
        var client = new ElectricityMapsClient(this.HttpClientFactory, this.Options.Object, this.Log.Object);

        // Act & Assert
        Assert.ThrowsAsync<ElectricityMapsClientException>(async () => await client.GetRecentCarbonIntensityHistoryAsync(TestZone));
    }

    [TestCase(BaseUrls.TrialBaseUrl, TestName = "GetRecentCarbonIntensityHistoryAsync_DeserializesExpectedResponse: Trial")]
    [TestCase(BaseUrls.TokenBaseUrl, TestName = "GetRecentCarbonIntensityHistoryAsync_DeserializesExpectedResponse: Token")]
    public async Task GetRecentCarbonIntensityHistoryAsync_DeserializesExpectedResponse(string baseUrl)
    {
        // Arrange
        this.Configuration.BaseUrl = baseUrl;

        AddHandlers_Auth_Zones(TestData.GetZonesAllowedJsonString());
        AddHandler_RequestResponse(r =>
            {
                return r.RequestUri!.ToString().Contains(baseUrl + Paths.History) && r.Method == HttpMethod.Get;
            }, System.Net.HttpStatusCode.OK, TestData.GetHistoryCarbonIntensityDataJsonString());

        var client = new ElectricityMapsClient(this.HttpClientFactory, this.Options.Object, this.Log.Object);

        // Act
        var data = await client.GetRecentCarbonIntensityHistoryAsync(TestLatitude, TestLongitude);
        var dataPoint = data?.HistoryData.First();

        // Assert
        Assert.That(data, Is.Not.Null);
        Assert.That(data?.Zone, Is.EqualTo(TestZone));
        Assert.That(data?.HistoryData.Count(), Is.GreaterThan(0));
        Assert.Multiple(() =>
        {
            Assert.That(dataPoint?.DateTime, Is.EqualTo(new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero)));
            Assert.That(dataPoint?.UpdatedAt, Is.EqualTo(new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero)));
            Assert.That(dataPoint?.CreatedAt, Is.EqualTo(new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero)));
            Assert.That(dataPoint?.Value, Is.EqualTo(999));
            Assert.That(dataPoint?.EmissionFactorType, Is.EqualTo("lifecycle"));
            Assert.That(dataPoint?.IsEstimated, Is.False);
            Assert.That(dataPoint?.EstimationMethod, Is.Null);
        });
    }


    /**
     * Helper to add client handlers for auth checking and for zone content response
     */
    private void AddHandlers_Auth_Zones(string zoneContent)
    {
        AddHandler_RequestResponse(r =>
            {
                var isTokenAuthInvalid = r.RequestUri!.ToString().Contains(BaseUrls.TokenBaseUrl) && !r.Headers.Where(x => x.Key == "auth-token").Any();
                var isTrialAuthInvalid = r.RequestUri!.ToString().Contains(BaseUrls.TrialBaseUrl) && !r.Headers.Where(x => x.Key == "X-BLOBR-KEY").Any();
                // If no auth and token setup, return unauthorized
                return isTokenAuthInvalid || isTrialAuthInvalid;
            }, System.Net.HttpStatusCode.Unauthorized);

        AddHandler_RequestResponse(r =>
            {
                return r.RequestUri?.ToString() == BaseUrls.TokenBaseUrl + Paths.Zones;
            }, System.Net.HttpStatusCode.OK, zoneContent);
    }

    /**
     * Helper to add client handler for request predicate and corresponding status code and response content
     */
    private void AddHandler_RequestResponse(Predicate<HttpRequestMessage> requestPredicate, System.Net.HttpStatusCode statusCode, string? responseContent = null) {
        if (responseContent != null) {
            this.Handler
                .SetupRequest(requestPredicate)
                .ReturnsResponse(statusCode, new StringContent(responseContent));
        } else {
            this.Handler
                .SetupRequest(requestPredicate)
                .ReturnsResponse(statusCode);
        }    
    }
}
