using CarbonAware.DataSources.Configuration;
using CarbonAware.Tools.ElectricityMapClient.Configuration;
using CarbonAware.Tools.ElectricityMapClient.Constants;
using CarbonAware.Tools.ElectricityMapClient.Model;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Mime;
using System.Text.Json;
using WireMock.Server;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace CarbonAware.WebApi.IntegrationTests;
public class ElectricityMapDataSourceMocker : IDataSourceMocker
{
    protected WireMockServer _server;
    private readonly object _dataSource = DataSourceType.WattTime;

    private static readonly Zone defaultZone = new()
    {
        countryCode = "Test-CC",
        countryName = "Test-CN",
        zoneName = "Test-ZN"
    };

    private static readonly LoginResult defaultLoginResult = new() { Token = "myDefaultToken123" };

    internal ElectricityMapDataSourceMocker()
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
            var em_data = new Data()
            {
                Datetime = new DateTimeOffset(2022, 8, 16, 3, 0, 0, new TimeSpan(0, 0, 0)),
                CarbonIntensity = 300,
                FossilFuelPercentage = 70.59F
            };

            var units = new Units()
            {
                CarbonIntensity = "gCO2eq/kWh"
            };

            var newDataPoint = new GridEmissionDataPoint()
            {
                CountryCodeAbbreviation = defaultZone.countryCode,
                Status = "ok",
                Data = em_data,
                Units = units
            };

            data.Add(newDataPoint);
            pointTime = newDataPoint.Data.Datetime + duration;
        }

        SetupResponseGivenGetRequest(Paths.Data, JsonSerializer.Serialize(data));
    }

    public void SetupForecastMock()
    {
        var start = DateTimeOffset.Now.ToUniversalTime();
        var end = start + TimeSpan.FromDays(1.0);
        var pointTime = start;
        var ForecastData = new List<GridEmissionDataPoint>();
        var currValue = 200.0F;

        while (pointTime < end)
        {

            var em_data = new Data()
            {
                Datetime = new DateTimeOffset(2022, 8, 16, 3, 0, 0, new TimeSpan(0, 0, 0)),
                CarbonIntensity = 300,
                FossilFuelPercentage = 70.59F
            };

            var units = new Units()
            {
                CarbonIntensity = "gCO2eq/kWh"
            };

            var newForecastPoint = new GridEmissionDataPoint()
            {
                CountryCodeAbbreviation = defaultZone.countryCode,
                Status = "ok",
                Data = em_data,
                Units = units
            };
            
            newForecastPoint.Data.Datetime = pointTime;
            newForecastPoint.Data.CarbonIntensity = currValue;
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
                services.Configure<ElectricityMapClientConfiguration>(configOpt =>
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

    // No sure caz electricity map no need ba mapper
    private void SetupBaMock(Zone? content = null) =>
        SetupResponseGivenGetRequest("", JsonSerializer.Serialize(content ?? defaultZone));

    private void SetupLoginMock(LoginResult? content = null) =>
        SetupResponseGivenGetRequest(Paths.Login, JsonSerializer.Serialize(content ?? defaultLoginResult));

}