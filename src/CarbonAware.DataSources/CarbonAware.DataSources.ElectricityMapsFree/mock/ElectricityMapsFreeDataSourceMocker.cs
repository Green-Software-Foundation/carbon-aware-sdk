using CarbonAware.Interfaces;
using CarbonAware.DataSources.ElectricityMapsFree.Model;
using CarbonAware.DataSources.ElectricityMapsFree.Constants;
using System.Net;
using System.Net.Mime;
using System.Text.Json;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace CarbonAware.DataSources.ElectricityMapsFree.Mocks;

internal class ElectricityMapsFreeDataSourceMocker : IDataSourceMocker
{
    private readonly WireMockServer _server;
    private static readonly JsonSerializerOptions _options = new JsonSerializerOptions(JsonSerializerDefaults.Web);


    public ElectricityMapsFreeDataSourceMocker()
    {
        _server = WireMockServer.Start();
        Environment.SetEnvironmentVariable("DataSources__Configurations__ElectricityMapsFree__BaseURL", _server.Url!);
        Initialize();
    }

    public void SetupDataMock(DateTimeOffset start, DateTimeOffset end, string location)
    {
        var data = new GridEmissionDataPoint
        {
            Disclaimer = string.Empty,
            Status = "ok",
            CountryCodeAbbreviation = location,
            Data = new Data() {
                Datetime = start,
                CarbonIntensity = 100,
                FossilFuelPercentage = 12.03F
            },
            Units = new Units()
            {
                CarbonIntensity = "gCO2eq/kWh"
            }
        };

        SetupResponseGivenGetRequest(Paths.Latest, data);
    }

    public void SetupForecastMock()
    {
        throw new NotImplementedException();
    }

    public void SetupHistoricalBatchForecastMock()
    {
        throw new NotImplementedException();
    }

    public void Initialize()
    {
        // No initialization needed
        return;
    }

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
