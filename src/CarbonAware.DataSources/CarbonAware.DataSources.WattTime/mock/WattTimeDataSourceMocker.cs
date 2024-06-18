using CarbonAware.DataSources.WattTime.Constants;
using CarbonAware.DataSources.WattTime.Model;
using CarbonAware.Interfaces;
using System.Net;
using System.Net.Mime;
using System.Text.Json;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace CarbonAware.DataSources.WattTime.Mocks;
internal class WattTimeDataSourceMocker : IDataSourceMocker
{
    protected WireMockServer _server;

    private static readonly RegionResponse defaultRegion = new()
    {
        Region = "TEST_REGION",
        RegionFullName = "Test Region Full Name",
        SignalType = SignalTypes.co2_moer
    };

    private static readonly LoginResult defaultLoginResult = new() { Token = "myDefaultToken123" };

    public WattTimeDataSourceMocker()
    {
        _server = WireMockServer.Start();
        Environment.SetEnvironmentVariable("DataSources__Configurations__WattTime__BaseURL", _server.Url!);
        Environment.SetEnvironmentVariable("DataSources__Configurations__WattTime__AuthenticationBaseUrl", _server.Url!);

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
                PointTime = pointTime,
                Value = 999.99F,
                Version = "1.0",
                Frequency = 300,
                Market = "mkt",
            };


            data.Add(newDataPoint);
            pointTime = newDataPoint.PointTime + duration;
        }

        var meta = new GridEmissionsMetaData()
        {
            Region = defaultRegion.Region,
            SignalType = SignalTypes.co2_moer
        };

        var gridEmissionsResponse = new GridEmissionsDataResponse()
        {
            Data = data,
            Meta = meta
        };

        SetupResponseGivenGetRequest(Paths.Data, JsonSerializer.Serialize(gridEmissionsResponse));
    }

    public void SetupForecastMock()
    {
        var curr = DateTimeOffset.UtcNow;
        var d = TimeSpan.FromMinutes(5);
        // Calculate nearest 5 minute increment from now to match expected WattTime data points.
        var start = new DateTimeOffset(((curr.Ticks + d.Ticks - 1) / d.Ticks) * d.Ticks, TimeSpan.Zero);
        var end = start + TimeSpan.FromDays(1.0);
        var pointTime = start;
        var forecastData = new List<GridEmissionDataPoint>();
        var currValue = 200.0F;

        while (pointTime < end)
        {
            var newForecastPoint = new GridEmissionDataPoint()
            {
                Frequency = 300,
                Market = "mkt",
                PointTime = start,
                Value = currValue,
                Version = "1.0"
            };
            newForecastPoint.PointTime = pointTime;
            newForecastPoint.Value = currValue;
            forecastData.Add(newForecastPoint);
            pointTime = pointTime + TimeSpan.FromMinutes(5);
            currValue = currValue + 5.0F;
        }

        var meta = new GridEmissionsMetaData()
        {
            Region = defaultRegion.Region,
            SignalType = SignalTypes.co2_moer,
            GeneratedAt = new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero)
        };

        var forecastResponse = new ForecastEmissionsDataResponse()
        {
            Data = forecastData,
            Meta = meta
        };


        SetupResponseGivenGetRequest(Paths.Forecast, JsonSerializer.Serialize(forecastResponse));
    }

    public void SetupHistoricalBatchForecastMock()
    {
        var start = new DateTimeOffset(2021, 9, 1, 8, 30, 0, TimeSpan.Zero);
        var end = start + TimeSpan.FromDays(1.0);
        var pointTime = start;
        var forecastData = new List<GridEmissionDataPoint>();
        var currValue = 200.0F;
        while (pointTime < end)
        {
            var newForecastPoint = new GridEmissionDataPoint()
            {
                Frequency = 300,
                Market = "mkt",
                PointTime = start,
                Value = currValue,
                Version = "1.0"
            };
            newForecastPoint.PointTime = pointTime;
            newForecastPoint.Value = currValue;
            forecastData.Add(newForecastPoint);
            pointTime = pointTime + TimeSpan.FromMinutes(5);
            currValue = currValue + 5.0F;
        }

        var meta = new GridEmissionsMetaData()
        {
            Region = defaultRegion.Region,
            SignalType = SignalTypes.co2_moer,
            GeneratedAt = new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero)
        };

        //var forecastBatchData = new List<ForecastEmissionsDataResponse> {
        //    new ForecastEmissionsDataResponse()
        //    {
        //        Data = forecastData,
        //        Meta = meta
        //    }
        //};

        var historicalForecastResponse = new HistoricalForecastEmissionsDataResponse()
        {
            Data = new List<HistoricalEmissionsData>()
            {
                new HistoricalEmissionsData()
                {
                    Forecast = forecastData,
                    GeneratedAt = new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero)
                }
            },
            Meta = meta
        };


        SetupResponseGivenGetRequest(Paths.ForecastHistorical, JsonSerializer.Serialize(historicalForecastResponse));
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
    private void SetupBaMock(RegionResponse? content = null) =>
        SetupResponseGivenGetRequest(Paths.RegionFromLocation, JsonSerializer.Serialize(content ?? defaultRegion));

    private void SetupLoginMock(LoginResult? content = null) =>
        SetupResponseGivenGetRequest(Paths.Login, JsonSerializer.Serialize(content ?? defaultLoginResult));

}