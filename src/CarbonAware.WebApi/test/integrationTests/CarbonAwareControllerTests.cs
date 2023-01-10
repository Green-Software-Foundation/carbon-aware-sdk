using CarbonAware.DataSources.Configuration;
using CarbonAware.WebApi.IntegrationTests;
using CarbonAware.WebApi.Models;
using NUnit.Framework;
using System.Net;
using System.Text.Json;

namespace CarbonAware.WepApi.IntegrationTests;

/// <summary>
/// Tests that the Web API controller handles and packages various responses from a plugin properly 
/// including empty responses and exceptions.
/// </summary>
[TestFixture(DataSourceType.JSON)]
[TestFixture(DataSourceType.WattTime)]
[TestFixture(DataSourceType.ElectricityMaps)]
public class CarbonAwareControllerTests : IntegrationTestingBase
{
    private readonly string healthURI = "/health";
    private readonly string fakeURI = "/fake-endpoint";
    private readonly string bestLocationsURI = "/emissions/bylocations/best";
    private readonly string currentForecastURI = "/emissions/forecasts/current";
    private readonly string batchForecastURI = "/emissions/forecasts/batch";
    private readonly string averageCarbonIntensityURI = "/emissions/average-carbon-intensity";
    private readonly string batchAverageCarbonIntensityURI = "/emissions/average-carbon-intensity/batch";

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
        _dataSourceMocker?.SetupDataMock(start, end, location);

        //Call the private method to construct with parameters
        var queryStrings = new Dictionary<string, string>();
        queryStrings["location"] = location;
        queryStrings["time"] = $"{start:O}";
        queryStrings["toTime"] = $"{end:O}";

        var endpointURI = ConstructUriWithQueryString(bestLocationsURI, queryStrings);

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
    public async Task EmissionsForecastsCurrent_SupportedDataSources_ReturnsOk()
    {
        IgnoreTestForDataSource("data source does not implement '/emissions/forecasts/current'", DataSourceType.JSON);

        _dataSourceMocker?.SetupForecastMock();

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

        _dataSourceMocker?.SetupForecastMock();

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

        _dataSourceMocker?.SetupForecastMock();

        var queryStrings = new Dictionary<string, string>();
        queryStrings[queryString] = value;

        var endpointURI = ConstructUriWithQueryString(currentForecastURI, queryStrings);

        var result = await _client.GetAsync(endpointURI);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [TestCase(null, null, TestName = "EmissionsForecastsBatch returns BadRequest for missing params: location, requestedAt")]
    [TestCase("eastus", null, TestName = "EmissionsForecastsBatch returns BadRequest for missing param: requestedAt")]
    [TestCase(null, "2021-09-01T08:30:00Z", TestName = "EmissionsForecastsBatch returns BadRequest for missing param: location")]
    [TestCase("eastus", "2021-9-1T08:30:00Z", TestName = "EmissionsForecastsBatch returns BadRequest for wrong date format")]
    public async Task EmissionsForecastsBatch_MissingRequiredParams_ReturnsBadRequest(string location, string requestedAt)
    {
        IgnoreTestForDataSource("data source does not implement '/emissions/forecasts/batch'", DataSourceType.JSON);

        _dataSourceMocker?.SetupForecastMock();
        var forecastData = Enumerable.Range(0, 1).Select(x => new
        {
            location = location,
            requestedAt = requestedAt
        });

        var result = await PostJSONBodyToURI(forecastData, batchForecastURI);

        Assert.That(result, Is.Not.Null);
        Assert.That(result?.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [TestCase("2021-09-01T08:30:00Z", "2021-09-01T09:00:00Z", "2021-09-01T12:00:00Z", "eastus", 1, TestName = "EmissionsForecastsBatch expects OK for single element")]
    [TestCase("2021-09-01T08:30:00Z", "2021-09-01T08:30:00Z", "2021-09-02T08:30:00Z", "westus", 3, TestName = "EmissionsForecastsBatch expects OK for multiple elements")]
    public async Task EmissionsForecastsBatch_SupportedDataSources_ReturnsOk(string reqAt, string start, string end, string location, int nelems)
    {
        IgnoreTestForDataSource("data source does not implement '/emissions/forecasts/batch'", DataSourceType.JSON, DataSourceType.ElectricityMaps);

        var expectedRequestedAt = DateTimeOffset.Parse(reqAt);
        var expectedDataStartAt = DateTimeOffset.Parse(start);
        var expectedDataEndAt = DateTimeOffset.Parse(end);
        _dataSourceMocker?.SetupBatchForecastMock();
        var inputData = Enumerable.Range(0, nelems).Select(x => new
        {
            requestedAt = reqAt,
            dataStartAt = start,
            dataEndAt = end,
            location = location
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
                    Assert.That(forecast.DataStartAt, Is.EqualTo(expectedDataStartAt));
                    Assert.That(forecast.DataEndAt, Is.EqualTo(expectedDataEndAt));
                    Assert.That(forecast.RequestedAt, Is.EqualTo(expectedRequestedAt));
                    Assert.That(forecast.GeneratedAt, Is.Not.Null);
                    Assert.That(forecast.OptimalDataPoints, Is.Not.Null);
                    Assert.That(forecast.ForecastData, Is.Not.Null);
                }
            }
        }
    }

    [TestCase("2022-1-1T04:05:06Z", "2022-1-2T04:05:06Z", "eastus", TestName = "EmissionsMarginalCarbonIntensity expects OK for full datetime")]
    [TestCase("2021-12-25", "2021-12-26", "westus", TestName = "EmissionsMarginalCarbonIntensity expects OK date only, no time")]
    public async Task EmissionsMarginalCarbonIntensity_ReturnsOk(string start, string end, string location)
    {
        var startDate = DateTimeOffset.Parse(start);
        var endDate = DateTimeOffset.Parse(end);
        _dataSourceMocker?.SetupDataMock(startDate, endDate, location);

        var queryStrings = new Dictionary<string, string>();
        queryStrings["location"] = location;
        queryStrings["startTime"] = start;
        queryStrings["endTime"] = end;

        var endpointURI = ConstructUriWithQueryString(averageCarbonIntensityURI, queryStrings);
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
                Assert.That(value!.CarbonIntensity, Is.Not.Null);
                Assert.That(value!.StartTime, Is.EqualTo(startDate));
                Assert.That(value!.EndTime, Is.EqualTo(endDate));
            }
        }
    }

    [TestCase("location", "", TestName = "EmissionsMarginalCarbonIntensity returns BadRequest for missing value for location")]
    [TestCase("non-location-param", "", TestName = "EmissionsMarginalCarbonIntensity returns BadRequest for location not present")]
    public async Task EmissionsMarginalCarbonIntensity_EmptyLocationQueryString_ReturnsBadRequest(string queryString, string value)
    {
        var queryStrings = new Dictionary<string, string>();
        queryStrings[queryString] = value;

        var endpointURI = ConstructUriWithQueryString(averageCarbonIntensityURI, queryStrings);
        var result = await _client.GetAsync(endpointURI);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [TestCase(null, null, null, TestName = "EmissionsMarginalCarbonIntensityBatch returns BadRequest for missing params: location, startTime, endTime")]
    [TestCase("eastus", null, null, TestName = "EmissionsMarginalCarbonIntensityBatch returns BadRequest for missing params: startTime, endTime")]
    [TestCase("eastus", "2022-03-01T15:30:00Z", null, TestName = "EmissionsMarginalCarbonIntensityBatch returns BadRequest for missing params: endTime")]
    [TestCase("eastus", null, "2022-03-01T18:00:00Z", TestName = "EmissionsMarginalCarbonIntensityBatch returns BadRequest for missing params: startTime")]
    [TestCase("westus", "2022-3-1T15:30:00Z", "2022-3-1T18:00:00Z", TestName = "EmissionsMarginalCarbonIntensityBatch returns BadRequest for wrong date format")]
    public async Task EmissionsMarginalCarbonIntensityBatch_MissingRequiredParams_ReturnsBadRequest(string location, string startTime, string endTime)
    {
        var intesityData = Enumerable.Range(0, 1).Select(x => new
        {
            location = location,
            startTime = startTime,
            endTime = endTime
        });
        var result = await PostJSONBodyToURI(intesityData, batchAverageCarbonIntensityURI);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [TestCase("2022-01-01T04:05:06Z", "2022-01-02T04:05:06Z", "eastus", 1, TestName = "EmissionsMarginalCarbonIntensityBatch expects OK for single element batch")]
    [TestCase("2021-12-25", "2021-12-26", "westus", 3, TestName = "EmissionsMarginalCarbonIntensityBatch expects OK for multiple element batch")]
    public async Task EmissionsMarginalCarbonIntensityBatch_SupportedDataSources_ReturnsOk(string start, string end, string location, int nelems)
    {
        var startDate = DateTimeOffset.Parse(start);
        var endDate = DateTimeOffset.Parse(end);
        _dataSourceMocker?.SetupDataMock(startDate, endDate, location);
        var intesityData = Enumerable.Range(0, nelems).Select(x => new
        {
            location = location,
            startTime = start,
            endTime = end
        });
        using (var result = await PostJSONBodyToURI(intesityData, batchAverageCarbonIntensityURI))
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.StatusCode, Is.EqualTo(HttpStatusCode.OK));
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
                    Assert.That(val.StartTime, Is.EqualTo(startDate));
                    Assert.That(val.EndTime, Is.EqualTo(endDate));
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
