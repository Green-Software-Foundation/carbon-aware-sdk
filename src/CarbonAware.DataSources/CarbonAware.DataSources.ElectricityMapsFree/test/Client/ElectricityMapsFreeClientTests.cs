using CarbonAware.DataSources.ElectricityMapsFree.Client;
using CarbonAware.DataSources.ElectricityMapsFree.Configuration;
using CarbonAware.DataSources.ElectricityMapsFree.Constants;
using CarbonAware.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Moq.Contrib.HttpClient;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Text.Json;

namespace CarbonAware.DataSources.ElectricityMapsFree.Tests;

[TestFixture]
public class ElectricityMapsFreeClientTests
{
    private readonly string TestLatitude = "37.783";
    private readonly string TestLongitude = "-122.417";
    private readonly string TestCountryCode = "US-CAL-CISO";

    private Mock<HttpMessageHandler> Handler { get; set; }

    private IHttpClientFactory HttpClientFactory { get; set; }

    private ElectricityMapsFreeClientConfiguration Configuration { get; set; }

    private Mock<IOptionsMonitor<ElectricityMapsFreeClientConfiguration>> Options { get; set; }

    private Mock<ILogger<ElectricityMapsFreeClient>> Log { get; set; }

    public ElectricityMapsFreeClientTests()
    {
        this.Configuration = new ElectricityMapsFreeClientConfiguration() { };

        this.Options = new Mock<IOptionsMonitor<ElectricityMapsFreeClientConfiguration>>();
        this.Log = new Mock<ILogger<ElectricityMapsFreeClient>>();

        this.Options.Setup(o => o.CurrentValue).Returns(() => this.Configuration);

        this.Handler = new Mock<HttpMessageHandler>();
        this.HttpClientFactory = Handler.CreateClientFactory();
        Mock.Get(this.HttpClientFactory).Setup(x => x.CreateClient(IElectricityMapsFreeClient.NamedClient))
            .Returns(() =>
            {
                var client = Handler.CreateClient();
                return client;
            });
    }

    [TestCase("ww.ca.u", "mytoken", TestName = "ClientInstantiation_FailsForInvalidConfig: url")]
    [TestCase("https://example.com/", "", TestName = "ClientInstantiation_FailsForInvalidConfig: Token")]
    public void ClientInstantiation_FailsForInvalidConfig(string baseUrl, string token)
    {
        // Arrange
        this.Configuration = new ElectricityMapsFreeClientConfiguration()
        {
            Token = token,
            BaseUrl = baseUrl,
        };

        // Act & Assert
        Assert.Throws<ConfigurationException>(() => new ElectricityMapsFreeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object));
    }

    [Test]
    public void AllPublicMethods_DoNotSwallowBadProxyExceptions()
    {
        // Arrange
        var mockHttpClientFactory = Mock.Of<IHttpClientFactory>();
        var mockHandler = new Mock<HttpClientHandler>();
        this.Configuration = GetValidConfiguration();

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

        var client = new ElectricityMapsFreeClient(mockHttpClientFactory, this.Options.Object, this.Log.Object);

        // Act & Assert
        Assert.ThrowsAsync<HttpRequestException>(async () => await client.GetCurrentEmissionsAsync(TestCountryCode));
        Assert.ThrowsAsync<HttpRequestException>(async () => await client.GetCurrentEmissionsAsync(TestLatitude, TestLongitude));
    }

    [Test]
    public void AllPublicMethods_ThrowJsonException_WhenBadJsonIsReturned()
    {
        // Arrange
        this.Configuration = GetValidConfiguration();
        AddHandler_RequestResponse(r =>
        {
            return r.RequestUri!.ToString().Contains(Paths.Latest) && r.Method == HttpMethod.Get;
        }, System.Net.HttpStatusCode.OK, "This is bad json");

        var client = new ElectricityMapsFreeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object);

        // Act & Assert
        Assert.ThrowsAsync<JsonException>(async () => await client.GetCurrentEmissionsAsync(TestLatitude, TestLongitude));
    }

    [Test]
    public async Task GetLatestCarbonIntensityAsync_DeserializesExpectedResponse()
    {
        // Arrange
        this.Configuration = GetValidConfiguration();
        AddHandler_RequestResponse(r =>
        {
            return r.RequestUri!.ToString().Contains(Paths.Latest) && r.Method == HttpMethod.Get;
        }, System.Net.HttpStatusCode.OK, TestData.GetLatestCarbonIntensityDataJsonString());

        var client = new ElectricityMapsFreeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object);

        // Act
        var latestData = await client.GetCurrentEmissionsAsync(TestLatitude, TestLongitude);
        var dataPoint = latestData?.Data;

        // Assert
        Assert.That(latestData, Is.Not.Null);
        Assert.That(latestData?.CountryCodeAbbreviation, Is.EqualTo(TestCountryCode));
        Assert.Multiple(() =>
        {
            Assert.That(dataPoint?.Datetime, Is.EqualTo(new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero)));
            Assert.That(dataPoint?.CarbonIntensity, Is.EqualTo(999));
        });
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
                .ReturnsResponse(statusCode, new StringContent(responseContent));
        }
        else
        {
            this.Handler
                .SetupRequest(requestPredicate)
                .ReturnsResponse(statusCode);
        }
    }

    /**
     * Provide valid values for token and baseurl
     */
    static private ElectricityMapsFreeClientConfiguration GetValidConfiguration()
    {
        return new ElectricityMapsFreeClientConfiguration()
        {
            Token = "mytoken",
            BaseUrl = "https://example.com/",
        };
    }
}