using CarbonAware.DataSources.ElectricityMaps.Model;
using CarbonAware.DataSources.ElectricityMaps.Constants;
using CarbonAware.DataSources.Mocks;
using System.Net;
using System.Net.Mime;
using System.Text.Json;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace CarbonAware.DataSources.ElectricityMaps.Mocks;

public class ElectricityMapsDataSourceMocker : IDataSourceMocker
{
    private WireMockServer _server;
    private const string ZONE_NAME = "eastus";
    private const string ZONE_KEY = "US-NE-ISNE";
    private static readonly JsonSerializerOptions _options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
    
    public ElectricityMapsDataSourceMocker()
    {
        _server = WireMockServer.Start();
        Environment.SetEnvironmentVariable("DataSources__Configurations__ElectricityMaps__BaseURL", _server.Url!);
        Initialize();
    }

    public void SetupHistoryMock(decimal latitude, decimal longitude)
    {
        var data = new List<CarbonIntensity>();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset past24 = now.AddHours(-24);

        while (past24 < now)
        {
            var newDataPoint = new CarbonIntensity()
            {
                Value = 999,
                DateTime = past24,
                UpdatedAt = now,
                CreatedAt = now,
                EmissionFactorType = "lifecycle",
                IsEstimated = false,
                EstimationMethod = null
            };

            data.Add(newDataPoint);
            past24 = past24.AddHours(1);
        }
        var result = new HistoryCarbonIntensityData
        {
            HistoryData = data,
            Zone = ZONE_NAME
        };

        SetupResponseGivenGetRequest(Paths.History, result);
    }

    public void SetupForecastMock()
    {
        var data = new List<Forecast>();
        DateTimeOffset baseTime = DateTimeOffset.UtcNow;
        DateTimeOffset baseTimeHour = new (baseTime.Year, baseTime.Month, baseTime.Day, baseTime.Hour, 0,0,baseTime.Offset);
        DateTimeOffset future24 = baseTimeHour.AddHours(24);

        while (baseTimeHour < future24)
        {
            var newDataPoint = new Forecast()
            {
                CarbonIntensity = 999,
                DateTime = baseTimeHour,
            };

            data.Add(newDataPoint);
            baseTimeHour = baseTimeHour.AddHours(1);
        }
        var result = new ForecastedCarbonIntensityData
        {
            ForecastData = data,
            Zone = ZONE_NAME,
            UpdatedAt = baseTime
        };

        SetupResponseGivenGetRequest(Paths.Forecast, result);
    }

    private void SetupZonesMock()
    {
        var zoneData = new ZoneData()
        {
            ZoneName = ZONE_NAME,
            Access = new string[]
            {
                Paths.History,
                Paths.Forecast
            }
        };
        var result = new Dictionary<string, ZoneData>() {
            { ZONE_KEY, zoneData },
        };

        SetupResponseGivenGetRequest(Paths.Zones, result);
    }

    public void SetupDataMock(DateTimeOffset start, DateTimeOffset end, string location)
    {
        var data = new List<CarbonIntensity>();
        DateTimeOffset pointTime = start;
        TimeSpan duration = TimeSpan.FromHours(1);

        while (pointTime < end)
        {
            var newDataPoint = new CarbonIntensity()
            {
                Value = 100,
                DateTime = pointTime,
            };

            data.Add(newDataPoint);
            pointTime = newDataPoint.DateTime + duration;
        }

        HistoryCarbonIntensityData history = new() { HistoryData = data };
        PastRangeData pastRange = new() { HistoryData = data };

        SetupResponseGivenGetRequest(Paths.History, history);
        SetupResponseGivenGetRequest(Paths.PastRange, pastRange);
    }

    public void SetupBatchForecastMock()
    {
        throw new NotImplementedException();
    }

    public void Initialize() => SetupZonesMock();

    public void Reset() => _server.Reset();

    public void Dispose() => _server.Dispose();

    private void SetupResponseGivenGetRequest(string path, object body)
    {
        var jsonBody = JsonSerializer.Serialize(body, _options);
        _server
            .Given(Request.Create().WithPath("/" + path).UsingGet())
            .RespondWith(
                Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", MediaTypeNames.Application.Json)
                    .WithBody(jsonBody)
        );
    }
}