namespace CarbonAware.WepApi.UnitTests;

using CarbonAware.Model;
using CarbonAware.WebApi.Controllers;
using CarbonAware.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

/// <summary>
/// Tests that the Web API controller handles and packages various responses from a plugin properly 
/// including empty responses and exceptions.
/// </summary>
[TestFixture]
public class CarbonAwareControllerTests : TestsBase
{
    /// <summary>
    /// Tests that successful emissions call to an aggregator with any data returned results in action with OK status.
    /// </summary>
    [TestCase(new object?[] { null, "Sydney" }, TestName = "GetEmissions simulates 'location=&location=Sydney'")]
    [TestCase(new object?[] { "Sydney", null }, TestName = "GetEmissions simulates 'location=Sydney&location='")]
    [TestCase(new object?[] { "Sydney" }, TestName = "GetEmissions simulates 'location=Sydney'")]
    public async Task GetEmissionsByMultipleLocations_SuccessfulCallReturnsOk(params string[] locations)
    {
        var data = new List<EmissionsData>()
        {
            new EmissionsData()
            {
                Location = "Sydney",
                Rating = 0.9,
                Time = DateTime.Now
            }
        };
        var controller = new CarbonAwareController(this.MockCarbonAwareLogger.Object, CreateAggregatorWithEmissionsData(data).Object);

        IActionResult result = await controller.GetEmissionsDataForLocationsByTime(locations);

        TestHelpers.AssertStatusCode(result, HttpStatusCode.OK);
    }

    /// <summary>
    /// Tests that successful emissions call to an aggregator with any data returned results in action with OK status.
    /// </summary>
    [Test]
    public async Task GetEmissionsBySingleLocation_SuccessfulCallReturnsOk()
    {
        var location = "Sydney";
        var data = new List<EmissionsData>()
        {
            new EmissionsData()
            {
                Location = location,
                Rating = 0.9,
                Time = DateTime.Now
            }
        };
        var controller = new CarbonAwareController(this.MockCarbonAwareLogger.Object, CreateAggregatorWithEmissionsData(data).Object);

        IActionResult result = await controller.GetEmissionsDataForLocationByTime(location);

        TestHelpers.AssertStatusCode(result, HttpStatusCode.OK);
    }

    /// <summary>
    /// Tests that successful best emissions call to an aggregator with any data returned results in action with OK status.
    /// </summary>
    [TestCase(new object?[] { null, "Sydney" }, TestName = "GetBestEmissions simulates 'location=&location=Sydney'")]
    [TestCase(new object?[] { "Sydney", null }, TestName = "GetBestEmissions simulates 'location=Sydney&location='")]
    [TestCase(new object?[] { "Sydney" }, TestName = "GetBestEmissions simulates 'location=Sydney'")]
    public async Task GetBestEmissions_SuccessfulCallReturnsOk(params string[] locations)
    {
        var data = new EmissionsData()
        {
            Location = "Sydney",
            Rating = 0.9,
            Time = DateTime.Now
        };

        var controller = new CarbonAwareController(this.MockCarbonAwareLogger.Object, CreateAggregatorWithBestEmissionsData(data).Object);

        var result = await controller.GetBestEmissionsDataForLocationsByTime(locations);

        TestHelpers.AssertStatusCode(result, HttpStatusCode.OK);
    }

    /// <summary>
    /// Tests that successful forecast call to an aggregator with any data returned results in action with OK status.
    /// </summary>
    [TestCase(new object?[] { null, "Sydney" }, TestName = "GetForecast simulates 'location=&location=Sydney'")]
    [TestCase(new object?[] { "Sydney", null }, TestName = "GetForecast simulates 'location=Sydney&location='")]
    [TestCase(new object?[] { "Sydney" }, TestName = "GetForecast simulates 'location=Sydney'")]
    public async Task GetForecast_SuccessfulCallReturnsOk(params string[] locations)
    {
        var data = new List<EmissionsData>()
        {
            new EmissionsData()
            {
                Location = "Sydney",
                Rating = 0.9,
                Time = DateTime.Now
            }
        };
        var aggregator = CreateAggregatorWithForecastData(data);
        var controller = new CarbonAwareController(this.MockCarbonAwareLogger.Object, aggregator.Object);

        IActionResult result = await controller.GetCurrentForecastData(locations);

        TestHelpers.AssertStatusCode(result, HttpStatusCode.OK);
        aggregator.Verify(a => a.GetCurrentForecastDataAsync(It.IsAny<Dictionary<string, object>>()), Times.Once);
    }

    /// <summary>
    /// Tests that successfull call to the aggregator with any data returned results in action with OK status.
    /// </summary>
    [TestCase("Sydney", "2022-03-07T01:00:00", "2022-03-07T03:30:00", TestName = "GetAverageCarbonIntensity Success ReturnsOk")]
    public async Task GetAverageCarbonIntensity_SuccessfulCallReturnsOk(string location, DateTimeOffset start, DateTimeOffset end)
    {
        // Arrange
        double data = 0.7;
        var controller = new CarbonAwareController(this.MockCarbonAwareLogger.Object, CreateCarbonAwareAggregatorWithAverageCI(data).Object);

        // Act
        var carbonIntensityOutput = (await controller.GetAverageCarbonIntensity(location, start, end)) as ObjectResult;

        // Assert
        TestHelpers.AssertStatusCode(carbonIntensityOutput, HttpStatusCode.OK);
        var expectedContent = new CarbonIntensityDTO { Location = location, StartTime = start, EndTime = end, CarbonIntensity = data };
        var actualContent = (carbonIntensityOutput == null) ? string.Empty : carbonIntensityOutput.Value;
        Assert.AreEqual(expectedContent, actualContent);
    }

    /// <summary>
    /// Tests that successfull call the average carbon intensity for a batch of requests on valid input
    /// </summary>
    [Test]
    public async Task CalculateAverageCarbonIntensityBatch_ValidInput()
    {
        // Arrange
        string location = "Sydney";
        var start1 = new DateTimeOffset(2022, 3, 1, 0, 0, 0, TimeSpan.Zero);
        var end1 = new DateTimeOffset(2022, 3, 1, 1, 0, 0, TimeSpan.Zero);

        var start2 = new DateTimeOffset(2022, 3, 2, 0, 0, 0, TimeSpan.Zero);
        var end2 = new DateTimeOffset(2022, 3, 2, 1, 0, 0, TimeSpan.Zero);
        double data = 0.7;
        var controller = new CarbonAwareController(this.MockCarbonAwareLogger.Object, CreateCarbonAwareAggregatorWithAverageCI(data).Object);

        var request1 = new CarbonIntensityBatchDTO { Location = location, StartTime = start1, EndTime = end1 };
        var request2 = new CarbonIntensityBatchDTO { Location = location, StartTime = start2, EndTime = end2 };
        var requestList = new List<CarbonIntensityBatchDTO> { request1, request2 };
        // Act
        var actualContent = new List<CarbonIntensityDTO> { };
        await foreach (var item in controller.GetAverageCarbonIntensityBatch(requestList))
        {
            actualContent.Add(item);
        }

        // Assert
        var expectedContent1 = new CarbonIntensityDTO { Location = location, StartTime = start1, EndTime = end1, CarbonIntensity = data };
        var expectedContent2 = new CarbonIntensityDTO { Location = location, StartTime = start2, EndTime = end2, CarbonIntensity = data };
        var expectedContent = new List<CarbonIntensityDTO> { expectedContent1, expectedContent2 };
        CollectionAssert.AreEqual(expectedContent, actualContent);
    }

    /// <summary>
    /// Tests that are missing a location and thus throw an ArgumentException error
    /// </summary>
    [Test]
    public void CalculateAverageCarbonIntensityBatch_NoLocations_ThrowsException()
    {
        //Arrange
        var batchRequestData = new List<CarbonIntensityBatchDTO>()
        {
            new CarbonIntensityBatchDTO
            {
                StartTime = new DateTimeOffset(2021,9,1,8,30,0, TimeSpan.Zero),
                EndTime = new DateTimeOffset(2021,9,2,8,30,0, TimeSpan.Zero)
            }
        };
        var data = 0.7d;
        var controller = new CarbonAwareController(this.MockCarbonAwareLogger.Object, CreateCarbonAwareAggregatorWithAverageCI(data).Object);
        Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await foreach (var _ in controller.GetAverageCarbonIntensityBatch(batchRequestData)) ;
        });
    }

    /// <summary>
    /// Tests that a success call to aggregator with no data returned results in action with No Content status.
    /// </summary>
    [Test]
    public async Task GetEmissions_EmptyResultReturnsNoContent()
    {
        var controller = new CarbonAwareController(this.MockCarbonAwareLogger.Object, CreateAggregatorWithEmissionsData(new List<EmissionsData>()).Object);

        string location = "Sydney";
        IActionResult singleLocationResult = await controller.GetEmissionsDataForLocationByTime(location);
        IActionResult multipleLocationsResult = await controller.GetEmissionsDataForLocationsByTime(new string[] { location });

        //Assert
        TestHelpers.AssertStatusCode(singleLocationResult, HttpStatusCode.NoContent);
        TestHelpers.AssertStatusCode(multipleLocationsResult, HttpStatusCode.NoContent);
    }

    /// <summary>
    /// Tests that a success call to aggregator with no data returned results in action with No Content status.
    /// </summary>
    [Test]
    public async Task GetBestEmissions_EmptyResultReturnsNoContent()
    {
        var controller = new CarbonAwareController(this.MockCarbonAwareLogger.Object, CreateAggregatorWithEmissionsData(new List<EmissionsData>()).Object);

        IActionResult result = await controller.GetBestEmissionsDataForLocationsByTime(new string[] { "Sydney" });

        //Assert
        TestHelpers.AssertStatusCode(result, HttpStatusCode.NoContent);
    }

    /// <summary>
    /// Tests empty or null location arrays throw ArgumentException.
    /// </summary>
    [TestCase(new object?[] { null, null }, TestName = "array of nulls: simulates 'location=&location=' empty value input")]
    [TestCase(new object?[] { null, }, TestName = "array of nulls: simulates 'location=' empty value input")]
    [TestCase(new object?[] { }, TestName = "empty array: simulates no 'location' query string")]
    public void GetEmissions_NoLocations_ThrowsException(params string[] locations)
    {
        var controller = new CarbonAwareController(this.MockCarbonAwareLogger.Object, CreateAggregatorWithEmissionsData(new List<EmissionsData>()).Object);

        Assert.ThrowsAsync<ArgumentException>(async () => await controller.GetBestEmissionsDataForLocationsByTime(locations));
        Assert.ThrowsAsync<ArgumentException>(async () => await controller.GetEmissionsDataForLocationsByTime(locations));
        Assert.ThrowsAsync<ArgumentException>(async () => await controller.GetCurrentForecastData(locations));
    }

    /// <summary>
    /// Tests empty location arrays throw ArgumentException.
    /// </summary>
    [Test]
    public void BatchForecast_NoLocations_ThrowsException()
    {
        var controller = new CarbonAwareController(this.MockCarbonAwareLogger.Object, CreateAggregatorWithEmissionsData(new List<EmissionsData>()).Object);
        var forecastData = new List<EmissionsForecastBatchDTO>()
        {
            new EmissionsForecastBatchDTO
            {
                DataStartAt = new DateTimeOffset(2021,9,1,8,30,0, TimeSpan.Zero),
                DataEndAt = new DateTimeOffset(2021,9,2,8,30,0, TimeSpan.Zero),
                RequestedAt = new DateTimeOffset(2021,9,1,8,30,0, TimeSpan.Zero)
            }
        };
        Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await foreach (var _ in controller.BatchForecastDataAsync(forecastData)) ;
        });
    }
}
