using CarbonAware.DataSources.Configuration;
using CarbonAware.WebApi.IntegrationTests;
using NUnit.Framework;
using System.Net;
using System.Text.Json;
using CarbonAware.WebApi.Models;

namespace CarbonAware.WepApi.IntegrationTests;

/// <summary>
/// Tests that the Web API controller handles and packages various responses from a plugin properly 
/// including empty responses and exceptions.
/// </summary>
[TestFixture(DataSourceType.JSON)]
[TestFixture(DataSourceType.WattTime)]
public class CarbonAwareControllerTests : IntegrationTestingBase
{
    private string healthURI = "/health";
    private string fakeURI = "/fake-endpoint";
    private string bestLocationsURI = "/emissions/bylocations/best";
    private string currentForecastURI = "/emissions/forecasts/current";
    private string batchForecastURI = "/emissions/forecasts/batch";
    private string actualHistoricalURI = "/emissions/average-carbon-intensity";
    private string batchActualURI = "/emissions/average-carbon-intensity/batch";

    private JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

    public CarbonAwareControllerTests(DataSourceType dataSource) : base(dataSource) { }

    [Test]
    public async Task HealthCheck_ReturnsOK()
    {
        //Use client to get endpoint
        var result = await _client.GetAsync(healthURI);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task FakeEndPoint_ReturnsNotFound()
    {
        var result = await _client.GetAsync(fakeURI);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    //ISO8601: YYYY-MM-DD
    [TestCase("2022-1-1T04:05:06Z", "2022-1-2T04:05:06Z", "eastus")]
    [TestCase("2021-12-25", "2021-12-26", "westus")]
    public async Task BestLocations_ReturnsOK(DateTimeOffset start, DateTimeOffset end, string location)
    {
        //Sets up any data endpoints needed for mocking purposes
        _dataSourceMocker.SetupDataMock(start, end, location);

        //Call the private method to construct with parameters
        var queryStrings = new Dictionary<string, string>();
        queryStrings["location"] = location;
        queryStrings["time"] = $"{start:O}";
        queryStrings["toTime"] = $"{end:O}";

        var endpointURI = ConstructUriWithQueryString(bestLocationsURI,queryStrings);

        //Get response and response content
        var result = await _client.GetAsync(endpointURI);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [TestCase("location", "", TestName = "empty location query string")]
    [TestCase("non-location-param", "", TestName = "location param not present")]
    public async Task BestLocations_EmptyLocationQueryString_ReturnsBadRequest(string queryString, string value)
    {
        //Call the private method to construct with parameters
        var queryStrings = new Dictionary<string, string>();
        queryStrings[queryString] = value;

        var endpointURI = ConstructUriWithQueryString(bestLocationsURI, queryStrings);

        //Get response and response content
        var result = await _client.GetAsync(endpointURI);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task EmissionsForecastsCurrent_UnsupportedDataSources_ReturnsNotImplemented()
    {
        IgnoreTestForDataSource("data source does implement '/emissions/forecasts/current'.", DataSourceType.WattTime);

        var queryStrings = new Dictionary<string, string>();
        queryStrings["location"] = "fakeLocation";

        var endpointURI = ConstructUriWithQueryString(currentForecastURI, queryStrings);

        var result = await _client.GetAsync(endpointURI);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.NotImplemented));
    }

    [Test]
    public async Task EmissionsForecastsCurrent_SupportedDataSources_ReturnsOk()
    {
        IgnoreTestForDataSource("data source does not implement '/emissions/forecasts/current'", DataSourceType.JSON);

        _dataSourceMocker.SetupForecastMock();

        var queryStrings = new Dictionary<string, string>();
        // A valid region name is required: 'location' is not specifically under test.
        queryStrings["location"] = "westus";

        var endpointURI = ConstructUriWithQueryString(currentForecastURI, queryStrings);

        var result = await _client.GetAsync(endpointURI);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task EmissionsForecastsCurrent_StartAndEndOutsideWindow_ReturnsBadRequest()
    {
        IgnoreTestForDataSource("data source does not implement '/emissions/forecasts/current'", DataSourceType.JSON);

        _dataSourceMocker.SetupForecastMock();

        var queryStrings = new Dictionary<string, string>();
        // A valid region name is required: 'location' is not specifically under test.
        queryStrings["location"] = "westus";
        // Mock data setup is set to current date.  This date will always be in the past.
        queryStrings["dataStartAt"] = "1999-01-01T00:00:00Z";
        queryStrings["dataEndAt"] = "1999-01-02T00:00:00Z";

        var endpointURI = ConstructUriWithQueryString(currentForecastURI, queryStrings);

        var result = await _client.GetAsync(endpointURI);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [TestCase("location", "", TestName = "empty location query string")]
    [TestCase("non-location-param", "", TestName = "location param not present")]
    public async Task EmissionsForecastsCurrent_InvalidLocationQueryString_ReturnsBadRequest(string queryString, string value)
    {

        IgnoreTestForDataSource("data source does not implement '/emissions/forecasts/current'", DataSourceType.JSON);

        _dataSourceMocker.SetupForecastMock();

        var queryStrings = new Dictionary<string, string>();
        queryStrings[queryString] = value;

        var endpointURI = ConstructUriWithQueryString(currentForecastURI, queryStrings);

        var result = await _client.GetAsync(endpointURI);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [TestCase(true, false, TestName = "Use location, Not use requestedAt")]
    [TestCase(false, true, TestName = "Not use location, Use requestedAt")]
    [TestCase(false, false, TestName = "Not use location, Not use requestedAt")]
     public async Task EmissionsForecastsBatch_MissingRequiredParams_ReturnsBadRequest(bool useLocation, bool useRequestedAt)
    {
        if (useLocation && useRequestedAt)
        {
            Assert.Fail("Invalid test");
        }
        IgnoreTestForDataSource("data source does not implement '/emissions/forecasts/batch'", DataSourceType.JSON);

        _dataSourceMocker.SetupForecastMock();
        var efb = new EmissionsForecastBatchDTO();
        if (useLocation)
        {
            efb.Location = "eastus";
        }
        if (useRequestedAt)
        {
            efb.RequestedAt = new DateTimeOffset(2021,9,1,8,30,0, TimeSpan.Zero);
        }
        var forecastData = new List<EmissionsForecastBatchDTO>() { efb };

        var result = await PostJSONBodyToURI(forecastData, batchForecastURI);

        Assert.That(result, Is.Not.Null);
        Assert.That(result?.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [TestCase("2021-09-01T08:30:00Z", "2021-09-01T09:00:00Z", "2021-09-01T12:00:00Z", "eastus", 1, TestName = "EmissionsForecastsBatch expects OK for eastus 1 element")]
    [TestCase("2021-09-01T08:30:00Z", "2021-09-01T08:30:00Z", "2021-09-02T08:30:00Z", "westus", 3, TestName = "EmissionsForecastsBatch expects OK for westus 3 element")]
    public async Task EmissionsForecastsBatch_SupportedDataSources_ReturnsOk(DateTimeOffset reqAt, DateTimeOffset start, DateTimeOffset end, string location, int nelems)
    {
        IgnoreTestForDataSource("data source does not implement '/emissions/forecasts/batch'", DataSourceType.JSON);

        _dataSourceMocker.SetupBatchForecastMock();
        var inputData = Enumerable.Range(0, nelems).Select(x => new EmissionsForecastBatchDTO() 
        {
            RequestedAt = reqAt,
            DataStartAt = start,
            DataEndAt = end,
            Location = location
        });

        using (var result = await PostJSONBodyToURI(inputData, batchForecastURI))
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result!.Content, Is.Not.Null);
            using (var data = await result!.Content.ReadAsStreamAsync())
            {
                Assert.That(data, Is.Not.Null);
                var forecasts = await JsonSerializer.DeserializeAsync<IEnumerable<EmissionsForecastDTO>>(data);
                Assert.That(forecasts, Is.Not.Null);
                Assert.That(forecasts!.Count, Is.EqualTo(inputData.Count()));
                foreach (var forecast in forecasts!)
                {
                    Assert.That(forecast.Location, Is.EqualTo(location));
                    Assert.That(forecast.DataStartAt, Is.EqualTo(start));
                    Assert.That(forecast.DataEndAt, Is.EqualTo(end));
                    Assert.That(forecast.RequestedAt, Is.EqualTo(reqAt));
                    Assert.That(forecast.GeneratedAt, Is.Not.Null);
                    Assert.That(forecast.OptimalDataPoint, Is.Not.Null);
                    Assert.That(forecast.ForecastData, Is.Not.Null);
                }
            }
        }
    }

    [TestCase("2022-1-1T04:05:06Z", "2022-1-2T04:05:06Z", "eastus", TestName = "EmissionsActual expects OK for eastus")]
    [TestCase("2021-12-25", "2021-12-26", "westus", TestName = "EmissionsActual expects OK for westus")]
    public async Task EmissionsActual_ReturnsOk(DateTimeOffset start, DateTimeOffset end, string location)
    {
        _dataSourceMocker.SetupDataMock(start, end, location);

        var queryStrings = new Dictionary<string, string>();
        queryStrings["location"] = location;
        queryStrings["startTime"] = $"{start:O}";
        queryStrings["endTime"] = $"{end:O}";

        var endpointURI = ConstructUriWithQueryString(actualHistoricalURI, queryStrings);
        using (var result = await _client.GetAsync(endpointURI))
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result!.Content, Is.Not.Null);
            using (var data = await result!.Content.ReadAsStreamAsync())
            {
                Assert.That(data, Is.Not.Null);
                var value = await JsonSerializer.DeserializeAsync<CarbonIntensityDTO>(data);
                Assert.That(value, Is.Not.Null);
                Assert.That(value!.CarbonIntensity, Is.Not.EqualTo(0));
                Assert.That(value!.StartTime, Is.EqualTo(start));
                Assert.That(value!.EndTime, Is.EqualTo(end));
            }
        }
    }

    [TestCase("location", "", TestName = "EmissionsActual empty location query string expects BadRequest")]
    [TestCase("non-location-param", "", TestName = "EmissionsActual location param not present expects BadRequest")]
    public async Task EmissionsActual_EmptyLocationQueryString_ReturnsBadRequest(string queryString, string value)
    {
        var queryStrings = new Dictionary<string, string>();
        queryStrings[queryString] = value;

        var endpointURI = ConstructUriWithQueryString(actualHistoricalURI, queryStrings);
        var result = await _client.GetAsync(endpointURI);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [TestCase(false, false, false, TestName = "EmissionsBatchActual Not use location, Not use startTime, Not use endTime expects BadRequest")]
    [TestCase(true, false, false, TestName = "EmissionsBatchActual Use location, Not use startTime, Not use endTime expects BadRequest")]
    [TestCase(true, true, false, TestName = "EmissionsBatchActual Use location, Use startTime, Not use endTime expects BadRequest")]
    public async Task EmissionsBatchActual_MissingRequiredParams_ReturnsBadRequest(bool useLocation, bool useStart, bool useEnd)
    {
        if (useLocation && useStart && useEnd)
        {
            Assert.Fail("Invalid test");
        }

        var intensityBatch = new CarbonIntensityBatchDTO();
        intensityBatch.Location = useLocation ? "eastus" : null;
        intensityBatch.StartTime = useStart ? DateTimeOffset.Parse("2022-03-01T15:30:00Z") : null;
        intensityBatch.EndTime = useEnd ? DateTimeOffset.Parse("2022-03-01T18:30:00Z") : null;
       
        var intesityData = new List<CarbonIntensityBatchDTO>() { intensityBatch };

        var result = await PostJSONBodyToURI(intesityData, batchActualURI);

        Assert.That(result, Is.Not.Null);
        Assert.That(result?.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [TestCase("2022-1-1T04:05:06Z", "2022-1-2T04:05:06Z", "eastus", 1, TestName = "EmissionsBatchActual expects OK for eastus 1 element")]
    [TestCase("2021-12-25", "2021-12-26", "westus", 3, TestName = "EmissionsBatchActual expects OK for westus 3 elements")]
    public async Task EmissionsBatchActual_SupportedDataSources_ReturnsOk(DateTimeOffset start, DateTimeOffset end, string location, int nelems)
    {
        _dataSourceMocker.SetupDataMock(start, end, location);
        var intesityData = Enumerable.Range(0, nelems).Select(x => new CarbonIntensityBatchDTO() 
        {
            Location = location,
            StartTime = start,
            EndTime = end
        });
        using (var result = await PostJSONBodyToURI(intesityData, batchActualURI))
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result?.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result!.Content, Is.Not.Null);
            using (var data = await result!.Content.ReadAsStreamAsync())
            {
                Assert.That(data, Is.Not.Null);
                var values = await JsonSerializer.DeserializeAsync<IEnumerable<CarbonIntensityDTO>>(data);
                Assert.That(values, Is.Not.Null);
                Assert.That(values!.Count, Is.EqualTo(intesityData.Count()));
                foreach (var val in values!)
                {
                    Assert.That(val.CarbonIntensity, Is.Not.EqualTo(0));
                    Assert.That(val.Location, Is.EqualTo(location));
                    Assert.That(val.StartTime, Is.EqualTo(start));
                    Assert.That(val.EndTime, Is.EqualTo(end));
                }
            }
        }
    }

    private void IgnoreTestForDataSource(string reasonMessage, params DataSourceType[] ignoredDataSources)
    {
        if (ignoredDataSources.Contains(_dataSource))
        {
            Assert.Ignore($"Ignoring test: {reasonMessage}");
        }
    }
}
