namespace CarbonAware.WepApi.IntegrationTests;

using CarbonAware.DataSources.Configuration;
using CarbonAware.WebApi.IntegrationTests;
using CarbonAware.Model;
using NUnit.Framework;
using System.Net;
using System.Text.Json;

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

    private void IgnoreTestForDataSource(string reasonMessage, params DataSourceType[] ignoredDataSources)
    {
        if (ignoredDataSources.Contains(_dataSource))
        {
            Assert.Ignore($"Ignoring test: {reasonMessage}");
        }
    }
}