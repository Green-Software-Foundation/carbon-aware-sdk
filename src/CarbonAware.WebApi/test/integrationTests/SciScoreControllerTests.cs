namespace CarbonAware.WepApi.IntegrationTests;

using CarbonAware.DataSources.Configuration;
using CarbonAware.WebApi.IntegrationTests;
using CarbonAware.WebApi.Models;
using NUnit.Framework;
using System.Net;
using System.Text.Json;

/// <summary>
/// Tests that the Web API controller handles and packages various responses from a plugin properly 
/// including empty responses and exceptions.
/// </summary>
[TestFixture(DataSourceType.JSON)]
[TestFixture(DataSourceType.WattTime)]
public class SciScoreControllerTests : IntegrationTestingBase
{
    private string marginalCarbonIntensityURI = "/sci-scores/marginal-carbon-intensity";
    private JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

    public SciScoreControllerTests(DataSourceType dataSource) : base(dataSource) { }


    [TestCase("2022-1-1T04:05:06Z", "2022-1-2T04:05:06Z", "eastus", HttpStatusCode.OK)]
    [TestCase("2021-11-18", "2022-1-2", "westus", HttpStatusCode.OK)]
    public async Task SCI_AcceptsValidData_ReturnsContent(string start, string end, string location, HttpStatusCode expectedCode)
    {
        var startTime = DateTimeOffset.Parse(start);
        var endTime = DateTimeOffset.Parse(end);
        _dataSourceMocker.SetupDataMock(startTime, endTime, location);
        string timeInterval = $"{start}/{end}";

        object body = new
        {
            location = new
            {
                locationType = "CloudProvider",
                providerName = "Azure",
                regionName = location
            },
            timeInterval = timeInterval
        };

        var result = await PostJSONBodyToURI(body, marginalCarbonIntensityURI);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.StatusCode, Is.EqualTo(expectedCode));
    }

    [TestCase("2022-1-1T04:05:06Z", "2022-1-2T04:05:06Z", "eastus", HttpStatusCode.BadRequest)]
    [TestCase("2021-1-1", "2022-1-2", "westus", HttpStatusCode.BadRequest)]
    public async Task SCI_RejectsInvalidData_ReturnsNotFound(DateTimeOffset start, DateTimeOffset end, string location, HttpStatusCode expectedCode)
    {
        _dataSourceMocker.SetupDataMock(start, end, location);
        string timeInterval = start.ToUniversalTime().ToString("O") + "/" + end.ToUniversalTime().ToString("O");

        object body = new
        {
            location = new {},
            timeInterval = timeInterval
        };

        var result = await PostJSONBodyToURI(body, marginalCarbonIntensityURI);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.StatusCode, Is.EqualTo(expectedCode));

        var resultContent = JsonSerializer.Deserialize<SciScore>(await result.Content.ReadAsStringAsync(), options)!;
        Assert.That(resultContent, Is.Not.Null);
        Assert.That(resultContent.MarginalCarbonIntensityValue, Is.Null);
    }

}
