using CarbonAware.Tools.electricityMapClient.Configuration;
using CarbonAware.Tools.electricityMapClient.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CarbonAware.Tools.electricityMapClient.Tests;

public class electricityMapClientTests
{
    private MockHttpMessageHandler MessageHandler { get; set; }

    private HttpClient HttpClient { get; set; }

    private IHttpClientFactory HttpClientFactory { get; set; }

    private electricityMapClientConfiguration Configuration { get; set; }

    private Mock<IOptionsMonitor<electricityMapClientConfiguration>> Options { get; set; }

    private Mock<ILogger<electricityMapClient>> Log { get; set; }

    private string BasicAuthValue { get; set; }

    private readonly string DefaultTokenValue = "myDefaultToken123";

    [SetUp]
    public void Initialize()
    {
        this.Configuration = new electricityMapClientConfiguration() { token = "myDefaultToken123" };

        this.Options = new Mock<IOptionsMonitor<electricityMapClientConfiguration>>();
        this.Log = new Mock<ILogger<electricityMapClient>>();

        this.Options.Setup(o => o.CurrentValue).Returns(() => this.Configuration);

        this.BasicAuthValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{this.Configuration.token}"));
    }

    [Test]
    public void AllPublicMethods_ThrowsWhenInvalidLogin()
    {
        Debug.WriteLine("This is Test");
        this.CreateHttpClient(m =>
        {
            var response = this.MockelectricityMapAuthResponse(m, new StringContent(""), "token");
            return Task.FromResult(response);
        });

        this.BasicAuthValue = "invalid";
        var client = new electricityMapClient(this.HttpClientFactory, this.Options.Object, this.Log.Object);
        Assert.ThrowsAsync<electricityMapClientHttpException>(async () => await client.GetCurrentForecastAsync("AUS-NSW"));
    }

    [Test]
    public void GetCurrentForecastAsync_ThrowsWhenBadJsonIsReturned()
    {
        this.CreateHttpClient(m =>
        {
            var response = this.MockelectricityMapAuthResponse(m, new StringContent("This is bad json."));
            return Task.FromResult(response);
        });


        var client = new electricityMapClient(this.HttpClientFactory, this.Options.Object, this.Log.Object);
        client.SetBearerAuthenticationHeader(this.DefaultTokenValue);
        var zone = new Zone() { countryCode = "AUS-NSW" };

        Assert.ThrowsAsync<JsonException>(async () => await client.GetCurrentForecastAsync(zone.countryCode));
        Assert.ThrowsAsync<JsonException>(async () => await client.GetCurrentForecastAsync(zone));
    }

    [Test]
    public void GetCurrentForecastAsync_ThrowsWhenNull()
    {
        this.CreateHttpClient(m =>
        {
            var response = this.MockelectricityMapAuthResponse(m, new StringContent("null"));
            return Task.FromResult(response);
        });

        var client = new electricityMapClient(this.HttpClientFactory, this.Options.Object, this.Log.Object);
        client.SetBearerAuthenticationHeader(this.DefaultTokenValue);
        var zone = new Zone() { countryCode = "AUS-NSW" };

        Assert.ThrowsAsync<electricityMapClientException>(async () => await client.GetCurrentForecastAsync(zone.countryCode));
        Assert.ThrowsAsync<electricityMapClientException>(async () => await client.GetCurrentForecastAsync(zone));
    }

    [Test]
    public async Task GetCurrentForecastAsync_DeserializesExpectedResponse()
    {
        this.CreateHttpClient(m =>
        {
            Assert.AreEqual("https://api.co2signal.com/v1/latest?countryCode=AUS-NSW", m.RequestUri?.ToString());
            Assert.AreEqual(HttpMethod.Get, m.Method);
            var response = this.MockelectricityMapAuthResponse(m, new StringContent(TestData.GetCurrentForecastJsonString()));
            return Task.FromResult(response);
        });

        var client = new electricityMapClient(this.HttpClientFactory, this.Options.Object, this.Log.Object);
        client.SetBearerAuthenticationHeader(this.DefaultTokenValue);

        var zone = new Zone() { countryCode = "AUS-NSW" };

        var forecast = await client.GetCurrentForecastAsync(zone.countryCode);
        var overloadedForecast = await client.GetCurrentForecastAsync(zone);

        Assert.AreEqual(forecast.GeneratedAt, overloadedForecast.GeneratedAt);
        Assert.AreEqual(forecast.ForecastData.First(), overloadedForecast.ForecastData.First());

        Assert.IsNotNull(forecast);
        Assert.AreEqual(new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero), forecast?.GeneratedAt);
        var forecastDataPoint = forecast?.ForecastData.First();
        Assert.AreEqual("ok", forecastDataPoint?.Status);
        Assert.AreEqual("AUS-NSW", forecastDataPoint?.CountryCodeAbbreviation);
        Assert.AreEqual(new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero), forecastDataPoint?.Data.Datetime);
        Assert.AreEqual("999.99", forecastDataPoint?.Data.CarbonIntensity.ToString("0.00"));
        Assert.AreEqual("99.99", forecastDataPoint?.Data.FossilFuelPercentage.ToString("0.00"));
        Assert.AreEqual("gCO2eq/kWh", forecastDataPoint?.Units.CarbonIntensity);
    }

    private void CreateHttpClient(Func<HttpRequestMessage, Task<HttpResponseMessage>> requestDelegate)
    {
        this.MessageHandler = new MockHttpMessageHandler(requestDelegate);
        this.HttpClient = new HttpClient(this.MessageHandler);
        this.HttpClientFactory = Mock.Of<IHttpClientFactory>();
        Mock.Get(this.HttpClientFactory).Setup(h => h.CreateClient(IelectricityMapClient.NamedClient)).Returns(this.HttpClient);
        this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("auth-token", this.DefaultTokenValue);
    }


    // Need Change to electricityMap Authentication

    private HttpResponseMessage MockelectricityMapAuthResponse(HttpRequestMessage request, HttpContent reponseContent, string? validToken = null)
    {
        if (validToken == null)
        {
            validToken = this.DefaultTokenValue;
        }
        var auth = this.HttpClient.DefaultRequestHeaders.Authorization;
        if (auth == null)
        {
            return new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
        }

        var authHeader = auth.ToString();
        var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);

        if ((request.RequestUri == new Uri("https://api.co2signal.com/v1/latest/") && ($"Basic {this.BasicAuthValue}".Equals(authHeader))))
        {
            response.Content = new StringContent("{\"token\":\"" + validToken + "\"}");
        }
        else if (authHeader == $"auth-token {validToken}")
        {
            response.Content = reponseContent;
        }
        else
        {
            response.StatusCode = System.Net.HttpStatusCode.Forbidden;
        }
        return response;
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