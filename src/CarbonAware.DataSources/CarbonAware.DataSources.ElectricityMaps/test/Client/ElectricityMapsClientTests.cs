using CarbonAware.DataSources.ElectricityMaps.Client;
using CarbonAware.DataSources.ElectricityMaps.Configuration;
using CarbonAware.DataSources.ElectricityMaps.Constants;
using CarbonAware.DataSources.ElectricityMaps.Model;
using CarbonAware.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
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

    private MockHttpMessageHandler MessageHandler { get; set; }

    private HttpClient HttpClient { get; set; }

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
    }

    [TestCase(BaseUrls.TrialBaseUrl, TestName = "ClientInstantiation_FailsForInvalidConfig: Trial")]
    [TestCase(BaseUrls.TokenBaseUrl, TestName = "ClientInstantiation_FailsForInvalidConfig: Token")]
    public void ClientInstantiation_FailsForInvalidConfig(string baseUrl)
    {
        // Arrange
        CreateBasicClient(TestData.GetZonesAllowedJsonString(), "{}");
        this.Configuration = new ElectricityMapsClientConfiguration()
        {
            APITokenHeader = null,
            APIToken = null,
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
        CreateBasicClient(TestData.GetZonesAllowedJsonString(), "{}");
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
    public void GetForecastedCarbonIntensityAsync_ThrowsWhenBadJsonIsReturned()
    {
        // Arrange
        CreateBasicClient(TestData.GetZonesAllowedJsonString(), "This is bad json");
        var client = new ElectricityMapsClient(this.HttpClientFactory, this.Options.Object, this.Log.Object);

        // Act & Assert
        Assert.ThrowsAsync<JsonException>(async () => await client.GetForecastedCarbonIntensityAsync(TestLatitude, TestLongitude));
    }

    [Test]
    public void GetForecastedCarbonIntensityAsync_ThrowsWhenNull()
    {
        // Arrange
        CreateBasicClient(TestData.GetZonesAllowedJsonString(), "null");
        var client = new ElectricityMapsClient(this.HttpClientFactory, this.Options.Object, this.Log.Object);

        // Act & Assert
        Assert.ThrowsAsync<ElectricityMapsClientException>(async () => await client.GetForecastedCarbonIntensityAsync(TestLatitude, TestLongitude));
    }

    [Test]
    public void GetForecastedCarbonIntensityAsync_ThrowsWhen_PathNotSupported()
    {
        // Arrange
        CreateBasicClient(TestData.GetNoPathsSupportedJsonString(), string.Empty);
        var client = new ElectricityMapsClient(this.HttpClientFactory, this.Options.Object, this.Log.Object);

        // Act & Assert
        Assert.ThrowsAsync<ElectricityMapsClientException>(async () => await client.GetForecastedCarbonIntensityAsync(TestZone));
    }

    [Test]
    public void GetForecastedCarbonIntensityAsync_ThrowsWhen_ZoneNotSupported()
    {
        // Arrange
        CreateBasicClient(TestData.GetNoZonesSupportedJsonString(), string.Empty);
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
        this.CreateHttpClient(m =>
        {
            if (m.RequestUri!.ToString() == baseUrl + Paths.Zones)
            {
                var response = this.MockElectricityMapsResponse(m, new StringContent(TestData.GetZonesAllowedJsonString()));
                return Task.FromResult(response);
            }
            else if (
                m.RequestUri!.ToString().Contains(baseUrl + Paths.Forecast) &&
                m.Method == HttpMethod.Get)
            {
                var response = this.MockElectricityMapsResponse(m, new StringContent(TestData.GetCurrentForecastJsonString()));
                return Task.FromResult(response);
            }
            return Task.FromResult(this.MockElectricityMapsResponse(m, new StringContent("null")));
        });

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
    public void GetRecentCarbonIntensityHistoryAsync_ThrowsWhenBadJsonIsReturned()
    {
        // Arrange
        CreateBasicClient(TestData.GetZonesAllowedJsonString(), "This is bad json");
        var client = new ElectricityMapsClient(this.HttpClientFactory, this.Options.Object, this.Log.Object);

        // Act & Assert
        Assert.ThrowsAsync<JsonException>(async () => await client.GetRecentCarbonIntensityHistoryAsync(TestLatitude, TestLongitude));
    }

    [Test]
    public void GetRecentCarbonIntensityHistoryAsync_ThrowsWhen_PathNotSupported()
    {
        // Arrange
        CreateBasicClient(TestData.GetNoPathsSupportedJsonString(), string.Empty);
        var client = new ElectricityMapsClient(this.HttpClientFactory, this.Options.Object, this.Log.Object);

        // Act & Assert
        Assert.ThrowsAsync<ElectricityMapsClientException>(async () => await client.GetRecentCarbonIntensityHistoryAsync(TestZone));
    }

    [Test]
    public void GetRecentCarbonIntensityHistoryAsync_ThrowsWhen_ZoneNotSupported()
    {
        // Arrange
        CreateBasicClient(TestData.GetNoZonesSupportedJsonString(), string.Empty);
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
        this.CreateHttpClient(m =>
        {
            if (m.RequestUri!.ToString() == baseUrl + Paths.Zones)
            {
                var response = this.MockElectricityMapsResponse(m, new StringContent(TestData.GetZonesAllowedJsonString()));
                return Task.FromResult(response);
            }
            else if (m.RequestUri!.ToString().Contains(baseUrl + Paths.History) && m.Method == HttpMethod.Get)
            {
                var response = this.MockElectricityMapsResponse(m, new StringContent(TestData.GetHistoryCarbonIntensityDataJsonString()));
                return Task.FromResult(response);
            }
            return Task.FromResult(this.MockElectricityMapsResponse(m, new StringContent("null")));
        });

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
            Assert.That(dataPoint?.CarbonIntensity, Is.EqualTo(999));
            Assert.That(dataPoint?.EmissionFactorType, Is.EqualTo(EmissionsFactor.Lifecycle));
            Assert.That(dataPoint?.IsEstimated, Is.False);
            Assert.That(dataPoint?.EstimationMethod, Is.Null);
        });
    }

    private void CreateBasicClient(string zoneContent, string resultContent)
    {
        this.CreateHttpClient(m =>
        {
            var isTokenAuthValid = m.RequestUri!.ToString().Contains(BaseUrls.TokenBaseUrl) && !m.Headers.Where(x => x.Key == "auth-token").Any();
            var isTrialAuthValid = m.RequestUri!.ToString().Contains(BaseUrls.TrialBaseUrl) && !m.Headers.Where(x => x.Key == "X-BLOBR-KEY").Any();
            // If no auth and token setup, return unauthorized
            if (isTokenAuthValid || isTrialAuthValid)
            {
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized));
            }

            if (m.RequestUri?.ToString() == BaseUrls.TokenBaseUrl + Paths.Zones)
            {
                var response = this.MockElectricityMapsResponse(m, new StringContent(zoneContent));
                return Task.FromResult(response);
            }
            return Task.FromResult(this.MockElectricityMapsResponse(m, new StringContent(resultContent)));
        });
    }

    private HttpResponseMessage MockElectricityMapsResponse(HttpRequestMessage request, HttpContent responseContent)
    {
        var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        response.Content = responseContent;
        return response;
    }

    private void CreateHttpClient(Func<HttpRequestMessage, Task<HttpResponseMessage>> requestDelegate)
    {
        this.MessageHandler = new MockHttpMessageHandler(requestDelegate);
        this.HttpClient = new HttpClient(this.MessageHandler);
        this.HttpClientFactory = Mock.Of<IHttpClientFactory>();
        Mock.Get(this.HttpClientFactory).Setup(h => h.CreateClient(IElectricityMapsClient.NamedClient)).Returns(this.HttpClient);
    }

    private class MockHttpMessageHandler : HttpMessageHandler
    {
        private Func<HttpRequestMessage, Task<HttpResponseMessage>> RequestDelegate { get; set; }

        public MockHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> requestDelegate)
        {
            this.RequestDelegate = requestDelegate;
        }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return await this.RequestDelegate.Invoke(request);
        }
    }
}
