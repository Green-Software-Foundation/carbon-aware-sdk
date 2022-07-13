using CarbonAware.DataSources.Configuration;
using CarbonAware.Tools.WattTimeClient.Configuration;
using CarbonAware.Tools.WattTimeClient.Constants;
using CarbonAware.Tools.WattTimeClient.Model;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Mime;
using System.Text.Json;
using WireMock.Server;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace CarbonAware.WebApi.IntegrationTests;
public class WattTimeDataSourceMocker : IDataSourceMocker
{
    protected WireMockServer _server;
    private readonly object _dataSource = DataSourceType.WattTime;

    private static readonly BalancingAuthority defaultBalancingAuthority = new()
    {
        Id = 12345,
        Abbreviation = "TEST_BA",
        Name = "Test Balancing Authority"
    };

    private static readonly LoginResult defaultLoginResult = new() { Token = "myDefaultToken123" };

    internal WattTimeDataSourceMocker()
    {
        _server = WireMockServer.Start();
        Initialize();
    }

    public void SetupDataMock(DateTimeOffset start, DateTimeOffset end, string location)
    {
        var data = new List<GridEmissionDataPoint>();
        DateTimeOffset pointTime = start;
        TimeSpan duration = TimeSpan.FromSeconds(300);

        while (pointTime < end)
        {
            var newDataPoint = new GridEmissionDataPoint()
            {
                BalancingAuthorityAbbreviation = defaultBalancingAuthority.Abbreviation,
                PointTime = pointTime,
                Value = 999.99F,
                Version = "1.0",
                Datatype = "dt",
                Frequency = 300,
                Market = "mkt",
            };

            data.Add(newDataPoint);
            pointTime = newDataPoint.PointTime + duration;
        }

        SetupResponseGivenGetRequest(Paths.Data, JsonSerializer.Serialize(data));
    }

    public void SetupForecastMock()
    {
        var curr = DateTimeOffset.UtcNow;
        var d = TimeSpan.FromMinutes(5);
        // Calculate nearest 5 minute increment from now to match expected WattTime data points.
        var start = new DateTimeOffset(((curr.Ticks + d.Ticks - 1) / d.Ticks) * d.Ticks, TimeSpan.Zero);
        var end = start + TimeSpan.FromDays(1.0);
        var pointTime = start;
        var ForecastData = new List<GridEmissionDataPoint>();
        var currValue = 200.0F;

        while (pointTime < end)
        {
            var newForecastPoint = new GridEmissionDataPoint()
            {
                BalancingAuthorityAbbreviation = defaultBalancingAuthority.Abbreviation,
                Datatype = "dt",
                Frequency = 300,
                Market = "mkt",
                PointTime = start,
                Value = currValue,
                Version = "1.0"
            };
            newForecastPoint.PointTime = pointTime;
            newForecastPoint.Value = currValue;
            ForecastData.Add(newForecastPoint);
            pointTime = pointTime + TimeSpan.FromMinutes(5);
            currValue = currValue + 5.0F;
        }

        var forecast = new Forecast()
        {
            ForecastData = ForecastData,
            GeneratedAt = new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero)
        };
        SetupResponseGivenGetRequest(Paths.Forecast, JsonSerializer.Serialize(forecast));
    }

    public WebApplicationFactory<Program> OverrideWebAppFactory(WebApplicationFactory<Program> factory)
    {
        return factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.Configure<WattTimeClientConfiguration>(configOpt =>
                {
                    configOpt.BaseUrl = _server.Url!;
                });
            });
        });
    }

    public void Initialize()
    {
        SetupBaMock();
        SetupLoginMock();
    }

    public void Reset()
    {
        _server.Reset();
    }

    public void Dispose()
    {
        _server.Dispose();
    }

    private void SetupResponseGivenGetRequest(string path, string body)
    {
        _server
            .Given(Request.Create().WithPath("/" + path).UsingGet())
            .RespondWith(
                Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", MediaTypeNames.Application.Json)
                    .WithBody(body)
        );
    }

    private void SetupBaMock(BalancingAuthority? content = null) =>
        SetupResponseGivenGetRequest(Paths.BalancingAuthorityFromLocation, JsonSerializer.Serialize(content ?? defaultBalancingAuthority));

    private void SetupLoginMock(LoginResult? content = null) =>
        SetupResponseGivenGetRequest(Paths.Login, JsonSerializer.Serialize(content ?? defaultLoginResult));

}