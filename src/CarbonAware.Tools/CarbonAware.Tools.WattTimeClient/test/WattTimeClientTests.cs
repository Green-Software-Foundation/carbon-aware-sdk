using CarbonAware.Tools.WattTimeClient.Configuration;
using CarbonAware.Tools.WattTimeClient.Model;
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

namespace CarbonAware.Tools.WattTimeClient.Tests;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
public class WattTimeClientTests
{
    private MockHttpMessageHandler MessageHandler { get; set; }

    private HttpClient HttpClient { get; set; }

    private IHttpClientFactory HttpClientFactory { get; set; }

    private WattTimeClientConfiguration Configuration { get; set; }

    private Mock<IOptionsMonitor<WattTimeClientConfiguration>> Options { get; set; }

    private Mock<ILogger<WattTimeClient>> Log { get; set; }

    private string BasicAuthValue { get; set; }

    private readonly string DefaultTokenValue = "myDefaultToken123";

    [SetUp]
    public void Initialize()
    {
        this.Configuration = new WattTimeClientConfiguration() { Username = "username", Password = "password" };

        this.Options = new Mock<IOptionsMonitor<WattTimeClientConfiguration>>();
        this.Log = new Mock<ILogger<WattTimeClient>>();

        this.Options.Setup(o => o.CurrentValue).Returns(() => this.Configuration);

        this.BasicAuthValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{this.Configuration.Username}:{this.Configuration.Password}"));
    }

    [Test]
    public void AllPublicMethods_ThrowsWhenInvalidLogin()
    {
        this.CreateHttpClient(m =>
        {
            var response = this.MockWattTimeAuthResponse(m, new StringContent(""), "token");
            return Task.FromResult(response);
        });

        this.BasicAuthValue = "invalid";
        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object);
        
        Assert.ThrowsAsync<WattTimeClientHttpException>(async () => await client.GetDataAsync("ba", new DateTimeOffset(), new DateTimeOffset()));
        Assert.ThrowsAsync<WattTimeClientHttpException>(async () => await client.GetCurrentForecastAsync("ba"));
        Assert.ThrowsAsync<WattTimeClientHttpException>(async () => await client.GetForecastByDateAsync("ba", new DateTimeOffset(), new DateTimeOffset()));
        Assert.ThrowsAsync<WattTimeClientHttpException>(async () => await client.GetBalancingAuthorityAsync("lat", "long"));
        Assert.ThrowsAsync<WattTimeClientHttpException>(async () => await client.GetBalancingAuthorityAbbreviationAsync("lat", "long"));
        Assert.ThrowsAsync<WattTimeClientHttpException>(async () => await client.GetHistoricalDataAsync("ba"));
    }

    [Test]
    public void GetDataAsync_ThrowsWhenBadJsonIsReturned()
    {
        this.CreateHttpClient(m =>
        {
            var response = this.MockWattTimeAuthResponse(m, new StringContent("This is bad json."));
            return Task.FromResult(response);
        });

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object);
        client.SetBearerAuthenticationHeader(this.DefaultTokenValue);

        Assert.ThrowsAsync<JsonException>(async () => await client.GetDataAsync("ba", new DateTimeOffset(), new DateTimeOffset()));
    }


    [Test]
    public async Task GetDataAsync_DeserializesExpectedResponse()
    {
        this.CreateHttpClient(m =>
        {
            Assert.AreEqual("https://api2.watttime.org/v2/data?ba=balauth&starttime=2022-04-22T00%3a00%3a00.0000000%2b00%3a00&endtime=2022-04-22T00%3a00%3a00.0000000%2b00%3a00", m.RequestUri?.ToString());
            Assert.AreEqual(HttpMethod.Get, m.Method);
            var response = this.MockWattTimeAuthResponse(m, new StringContent(TestData.GetGridDataJsonString()));
            return Task.FromResult(response);
        });

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object);
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
        this.CreateHttpClient(m =>
        {
            var content = new StringContent(TestData.GetGridDataJsonString());
            var response = this.MockWattTimeAuthResponse(m, content, "REFRESHTOKEN");
            return Task.FromResult(response);
        });

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object);
        client.SetBearerAuthenticationHeader(this.DefaultTokenValue);

        var data = await client.GetDataAsync("balauth", new DateTimeOffset(), new DateTimeOffset());
        
        Assert.IsTrue(data.Count() > 0);
        var gridDataPoint = data.ToList().First();
        Assert.AreEqual("ba", gridDataPoint.BalancingAuthorityAbbreviation);
    }

    [Test]
    public async Task GetDataAsync_RefreshesTokenWhenNoneSet()
    {
        this.CreateHttpClient(m =>
        {
            var content = new StringContent(TestData.GetGridDataJsonString());
            var response = this.MockWattTimeAuthResponse(m, content, "REFRESHTOKEN");
            return Task.FromResult(response);
        });

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object);

        var data = await client.GetDataAsync("balauth", new DateTimeOffset(), new DateTimeOffset());

        Assert.IsTrue(data.Count() > 0);
        var gridDataPoint = data.ToList().First();
        Assert.AreEqual("ba", gridDataPoint.BalancingAuthorityAbbreviation);
    }

    [Test]
    public void GetCurrentForecastAsync_ThrowsWhenBadJsonIsReturned()
    {
        this.CreateHttpClient(m =>
        {
            var response = this.MockWattTimeAuthResponse(m, new StringContent("This is bad json."));
            return Task.FromResult(response);
        });


        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object);
        client.SetBearerAuthenticationHeader(this.DefaultTokenValue);
        var ba = new BalancingAuthority(){ Abbreviation = "balauth" };

        Assert.ThrowsAsync<JsonException>(async () => await client.GetCurrentForecastAsync(ba.Abbreviation));
        Assert.ThrowsAsync<JsonException>(async () => await client.GetCurrentForecastAsync(ba));
    }

    [Test]
    public void GetCurrentForecastAsync_ThrowsWhenNull()
    {
        this.CreateHttpClient(m =>
        {
            var response = this.MockWattTimeAuthResponse(m, new StringContent("null"));
            return Task.FromResult(response);
        });

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object);
        client.SetBearerAuthenticationHeader(this.DefaultTokenValue);
        var ba = new BalancingAuthority(){ Abbreviation = "balauth" };

        Assert.ThrowsAsync<WattTimeClientException>(async () => await client.GetCurrentForecastAsync(ba.Abbreviation));
        Assert.ThrowsAsync<WattTimeClientException>(async () => await client.GetCurrentForecastAsync(ba));
    }

    [Test]
    public async Task GetCurrentForecastAsync_DeserializesExpectedResponse()
    {
        this.CreateHttpClient(m =>
        {
            Assert.AreEqual("https://api2.watttime.org/v2/forecast?ba=balauth", m.RequestUri?.ToString());
            Assert.AreEqual(HttpMethod.Get, m.Method);
            var response = this.MockWattTimeAuthResponse(m, new StringContent(TestData.GetCurrentForecastJsonString()));
            return Task.FromResult(response);
        });

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object);
        client.SetBearerAuthenticationHeader(this.DefaultTokenValue);

        var ba = new BalancingAuthority(){ Abbreviation = "balauth" };

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
        this.CreateHttpClient(m =>
        {
            var content = new StringContent(TestData.GetCurrentForecastJsonString());
            var response = this.MockWattTimeAuthResponse(m, content, "REFRESHTOKEN");
            return Task.FromResult(response);
        });

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object);
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
        this.CreateHttpClient(m =>
        {
            var content = new StringContent(TestData.GetCurrentForecastJsonString());
            var response = this.MockWattTimeAuthResponse(m, content, "REFRESHTOKEN");
            return Task.FromResult(response);
        });

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object);

        this.HttpClient.DefaultRequestHeaders.Authorization = null;

        var forecast = await client.GetCurrentForecastAsync("balauth");

        Assert.IsNotNull(forecast);
        Assert.AreEqual(new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero), forecast?.GeneratedAt);
        var forecastDataPoint = forecast?.ForecastData.First();
        Assert.AreEqual("ba", forecastDataPoint?.BalancingAuthorityAbbreviation);
    }

    [Test]
    public void GetForecastByDateAsync_ThrowsWhenBadJsonIsReturned()
    {
        this.CreateHttpClient(m =>
        {
            var response = this.MockWattTimeAuthResponse(m, new StringContent("This is bad json."));
            return Task.FromResult(response);
        });

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object);
        client.SetBearerAuthenticationHeader(this.DefaultTokenValue);
        var ba = new BalancingAuthority(){ Abbreviation = "balauth" };

        Assert.ThrowsAsync<JsonException>(async () => await client.GetForecastByDateAsync(ba.Abbreviation, new DateTimeOffset(), new DateTimeOffset()));
        Assert.ThrowsAsync<JsonException>(async () => await client.GetForecastByDateAsync(ba, new DateTimeOffset(), new DateTimeOffset()));
    }

    [Test]
    public void GetForecastByDateAsync_ThrowsWhenNull()
    {
        this.CreateHttpClient(m =>
        {
            var response = this.MockWattTimeAuthResponse(m, new StringContent("null"));
            return Task.FromResult(response);
        });

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object);
        client.SetBearerAuthenticationHeader(this.DefaultTokenValue);
        var ba = new BalancingAuthority(){ Abbreviation = "balauth" };

        Assert.ThrowsAsync<WattTimeClientException>(async () => await client.GetForecastByDateAsync(ba.Abbreviation, new DateTimeOffset(), new DateTimeOffset()));
        Assert.ThrowsAsync<WattTimeClientException>(async () => await client.GetForecastByDateAsync(ba, new DateTimeOffset(), new DateTimeOffset()));
    }

    [Test]
    public async Task GetForecastByDateAsync_DeserializesExpectedResponse()
    {
        this.CreateHttpClient(m =>
        {
            Assert.AreEqual("https://api2.watttime.org/v2/forecast?ba=balauth&starttime=2022-04-22T00%3a00%3a00.0000000%2b00%3a00&endtime=2022-04-22T00%3a00%3a00.0000000%2b00%3a00", m.RequestUri?.ToString());
            Assert.AreEqual(HttpMethod.Get, m.Method);
            var response = this.MockWattTimeAuthResponse(m, new StringContent(TestData.GetForecastByDateJsonString()));
            return Task.FromResult(response);
        });

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object);
        client.SetBearerAuthenticationHeader(this.DefaultTokenValue);
        var ba = new BalancingAuthority(){ Abbreviation = "balauth" };

        var forecasts = await client.GetForecastByDateAsync(ba.Abbreviation, new DateTimeOffset(2022, 4, 22, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(2022, 4, 22, 0, 0, 0, TimeSpan.Zero));
        var overloadedForecasts = await client.GetForecastByDateAsync(ba, new DateTimeOffset(2022, 4, 22, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(2022, 4, 22, 0, 0, 0, TimeSpan.Zero));

        Assert.IsTrue(forecasts.Count() > 0);
        var forecast = forecasts.ToList().First();
        var overloadedForecast = overloadedForecasts.ToList().First();

        Assert.AreEqual(forecast.GeneratedAt, overloadedForecast.GeneratedAt);
        Assert.AreEqual(forecast.ForecastData.First(), overloadedForecast.ForecastData.First());

        Assert.AreEqual(new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero), forecast.GeneratedAt);
        var forecastDataPoint = forecast.ForecastData.ToList().First();
        Assert.AreEqual("ba", forecastDataPoint.BalancingAuthorityAbbreviation);
        Assert.AreEqual(new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero), forecastDataPoint.PointTime);
        Assert.AreEqual("999.99", forecastDataPoint.Value.ToString("0.00")); //Format float to avoid precision issues
        Assert.AreEqual("1.0", forecastDataPoint.Version);
    }

    [Test]
    public async Task GetForecastByDateAsync_RefreshesTokenWhenExpired()
    {
        this.CreateHttpClient(m =>
        {
            var content = new StringContent(TestData.GetForecastByDateJsonString());
            var response = this.MockWattTimeAuthResponse(m, content, "REFRESHTOKEN");
            return Task.FromResult(response);
        });

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object);
        client.SetBearerAuthenticationHeader(this.DefaultTokenValue);

        var forecasts = await client.GetForecastByDateAsync("balauth", new DateTimeOffset(), new DateTimeOffset());

        Assert.IsTrue(forecasts.Count() > 0);
        var forecast = forecasts.ToList().First();
        Assert.AreEqual(new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero), forecast.GeneratedAt);
    }

    [Test]
    public async Task GetForecastByDateAsync_RefreshesTokenWhenNoneSet()
    {
        this.CreateHttpClient(m =>
        {
            var content = new StringContent(TestData.GetForecastByDateJsonString());
            var response = this.MockWattTimeAuthResponse(m, content, "REFRESHTOKEN");
            return Task.FromResult(response);
        });

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object);

        this.HttpClient.DefaultRequestHeaders.Authorization = null;

        var forecasts = await client.GetForecastByDateAsync("balauth", new DateTimeOffset(), new DateTimeOffset());
        
        Assert.IsTrue(forecasts.Count() > 0);
        var forecast = forecasts.ToList().First();
        Assert.AreEqual(new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero), forecast.GeneratedAt);
    }

    [Test]
    public void GetBalancingAuthorityAsync_ThrowsWhenBadJsonIsReturned()
    {
        this.CreateHttpClient(m =>
        {
            var response = this.MockWattTimeAuthResponse(m, new StringContent("This is bad json."));
            return Task.FromResult(response);
        });

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object);
        client.SetBearerAuthenticationHeader(this.DefaultTokenValue);

        Assert.ThrowsAsync<JsonException>(async () => await client.GetBalancingAuthorityAsync("lat", "long"));
    }

    [Test]
    public void GetBalancingAuthorityAsync_ThrowsWhenNull()
    {
        this.CreateHttpClient(m =>
        {
            var response = this.MockWattTimeAuthResponse(m, new StringContent("null"));
            return Task.FromResult(response);
        });

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object);
        client.SetBearerAuthenticationHeader(this.DefaultTokenValue);

        Assert.ThrowsAsync<WattTimeClientException>(async () => await client.GetBalancingAuthorityAsync("lat", "long"));
    }

    [Test]
    public async Task GetBalancingAuthorityAsync_DeserializesExpectedResponse()
    {
        this.CreateHttpClient(m =>
        {
            Assert.AreEqual("https://api2.watttime.org/v2/ba-from-loc?latitude=lat&longitude=long", m.RequestUri?.ToString());
            Assert.AreEqual(HttpMethod.Get, m.Method);
            var response = this.MockWattTimeAuthResponse(m, new StringContent(TestData.GetBalancingAuthorityJsonString()));
            return Task.FromResult(response);
        });

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object);
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
        this.CreateHttpClient(m =>
        {
            var content = new StringContent(TestData.GetBalancingAuthorityJsonString());
            var response = this.MockWattTimeAuthResponse(m, content, "REFRESHTOKEN");
            return Task.FromResult(response);
        });

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object);
        client.SetBearerAuthenticationHeader(this.DefaultTokenValue);

        var ba = await client.GetBalancingAuthorityAsync("lat", "long");

        Assert.IsNotNull(ba);
        Assert.AreEqual(12345, ba?.Id);
    }

    [Test]
    public async Task GetBalancingAuthorityAsync_RefreshesTokenWhenNoneSet()
    {
        this.CreateHttpClient(m =>
        {
            var content = new StringContent(TestData.GetBalancingAuthorityJsonString());
            var response = this.MockWattTimeAuthResponse(m, content, "REFRESHTOKEN");
            return Task.FromResult(response);
        });

        var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object);

        this.HttpClient.DefaultRequestHeaders.Authorization = null;

        var ba = await client.GetBalancingAuthorityAsync("lat", "long");

        Assert.IsNotNull(ba);
        Assert.AreEqual(12345, ba?.Id);
    }

    [Test]
    public async Task GetHistoricalDataAsync_StreamsExpectedContent()
    {
        using (var testStream = new MemoryStream(Encoding.UTF8.GetBytes("myStreamResults")))
        {
            this.CreateHttpClient(m =>
            {
                var response = this.MockWattTimeAuthResponse(m, new StreamContent(testStream));
                return Task.FromResult(response);
            });

            var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object);
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
            this.CreateHttpClient(m =>
            {
                var response = this.MockWattTimeAuthResponse(m, new StreamContent(testStream), "REFRESHTOKEN");
                return Task.FromResult(response);
            });

            this.HttpClient.DefaultRequestHeaders.Authorization = null;
            var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object);

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
            this.CreateHttpClient(m =>
            {
                var response = this.MockWattTimeAuthResponse(m, new StreamContent(testStream), "REFRESHTOKEN");
                return Task.FromResult(response);
            });

            var client = new WattTimeClient(this.HttpClientFactory, this.Options.Object, this.Log.Object);
            client.SetBearerAuthenticationHeader(this.DefaultTokenValue);

            var result = await client.GetHistoricalDataAsync("ba");
            var sr = new StreamReader(result);
            string streamResult = sr.ReadToEnd();

            Assert.AreEqual("myStreamResults", streamResult);
        }
    }

    [Test]
    public void TestClient_With_Proxy_Failure()
    {
        var key1 = $"{CarbonAwareVariablesConfiguration.Key}:Proxy:UseProxy";
        var key2 = $"{CarbonAwareVariablesConfiguration.Key}:Proxy:Url";
        var settings = new Dictionary<string, string> {
                {key1, "true"},
                {key2, "http://fakeproxy:8080"},
            };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
        var serviceCollection = new ServiceCollection();
        serviceCollection.ConfigureWattTimeClient(configuration);
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var client = serviceProvider.GetRequiredService<IWattTimeClient>();
        Assert.ThrowsAsync<HttpRequestException>(async () => await client.GetBalancingAuthorityAsync("lat", "long"));
    }

    [Test]
    public void TestClient_With_Missing_Proxy_URL()
    {
        var key1 = $"{CarbonAwareVariablesConfiguration.Key}:Proxy:UseProxy";
        var settings = new Dictionary<string, string> {
                {key1, "true"},
            };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
        var serviceCollection = new ServiceCollection();
        Assert.Throws<ConfigurationException>(() => serviceCollection.ConfigureWattTimeClient(configuration));
    }

    private void CreateHttpClient(Func<HttpRequestMessage, Task<HttpResponseMessage>> requestDelegate)
    {
        this.MessageHandler = new MockHttpMessageHandler(requestDelegate);
        this.HttpClient = new HttpClient(this.MessageHandler);
        this.HttpClientFactory = Mock.Of<IHttpClientFactory>();
        Mock.Get(this.HttpClientFactory).Setup(h => h.CreateClient(IWattTimeClient.NamedClient)).Returns(this.HttpClient);
        this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.DefaultTokenValue);
    }

    private HttpResponseMessage MockWattTimeAuthResponse(HttpRequestMessage request, HttpContent reponseContent, string? validToken = null)
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

        if ((request.RequestUri == new Uri("https://api2.watttime.org/v2/login") && ($"Basic {this.BasicAuthValue}".Equals(authHeader))))
        {
            response.Content = new StringContent("{\"token\":\""+validToken+"\"}");
        }
        else if (authHeader == $"Bearer {validToken}")
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
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

