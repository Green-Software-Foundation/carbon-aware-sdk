namespace CarbonAware.WepApi.UnitTests;

using GSF.CarbonAware.Handlers;
using GSF.CarbonAware.Models;
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
    readonly IForecastHandler forecastHandler = Mock.Of<IForecastHandler>();
    
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
        var controller = new CarbonAwareController(this.MockCarbonAwareLogger.Object, CreateEmissionsHandler(data).Object, forecastHandler, CreateLocations(false).Object);
        var parametersDTO = new EmissionsDataForLocationsParametersDTO() { MultipleLocations = locations };

        IActionResult result = await controller.GetEmissionsDataForLocationsByTime(parametersDTO);

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
        var controller = new CarbonAwareController(this.MockCarbonAwareLogger.Object, CreateEmissionsHandler(data).Object, forecastHandler, CreateLocations(false).Object);

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
        var data = new List<EmissionsData>()
        {
            new EmissionsData(){
                Location = "Sydney",
                Rating = 0.9,
                Time = DateTime.Now
            }
        };
        var controller = new CarbonAwareController(this.MockCarbonAwareLogger.Object, CreateHandlerWithBestEmissionsData(data).Object, forecastHandler, CreateLocations(false).Object);
        var parametersDTO = new EmissionsDataForLocationsParametersDTO(){ MultipleLocations = locations };

        var result = await controller.GetBestEmissionsDataForLocationsByTime(parametersDTO);

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
        var emissionsHandler = Mock.Of<IEmissionsHandler>();
        var forecastHandler = CreateForecastHandler(data);
        var controller = new CarbonAwareController(this.MockCarbonAwareLogger.Object, emissionsHandler, forecastHandler.Object, CreateLocations(false).Object);
        var parametersDTO = new EmissionsForecastCurrentParametersDTO() { MultipleLocations = locations };

        IActionResult result = await controller.GetCurrentForecastData(parametersDTO);

        TestHelpers.AssertStatusCode(result, HttpStatusCode.OK);
        forecastHandler.Verify(a => a.GetCurrentForecastAsync(It.IsAny<string[]>(), null, null, null), Times.Once);
    }

    /// <summary>
    /// Tests that successfull call to the aggregator with any data returned results in action with OK status.
    /// </summary>
    [TestCase("Sydney", "2022-03-07T01:00:00", "2022-03-07T03:30:00", TestName = "GetAverageCarbonIntensity Success ReturnsOk")]
    public async Task GetAverageCarbonIntensity_SuccessfulCallReturnsOk(string location, DateTimeOffset start, DateTimeOffset end)
    {
        // Arrange
        double data = 0.7;
        var controller = new CarbonAwareController(this.MockCarbonAwareLogger.Object, CreateCarbonAwareHandlerWithAverageCI(data).Object, forecastHandler, CreateLocations(false).Object);

        var parametersDTO = new CarbonIntensityParametersDTO
        {
            SingleLocation = location,
            Start = start,
            End = end
        };

        // Act
        var carbonIntensityOutput = (await controller.GetAverageCarbonIntensity(parametersDTO)) as ObjectResult;

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
        var data = 0.7;

        var handler = CreateCarbonAwareHandlerWithAverageCI(data);
        var controller = new CarbonAwareController(this.MockCarbonAwareLogger.Object, handler.Object, forecastHandler, CreateLocations(false).Object);

        var request1 = new CarbonIntensityBatchParametersDTO { SingleLocation = location, Start = start1, End = end1 };
        var request2 = new CarbonIntensityBatchParametersDTO { SingleLocation = location, Start = start2, End = end2 };
        var requestList = new List<CarbonIntensityBatchParametersDTO> { request1, request2 };
        // Act
        var result = await controller.GetAverageCarbonIntensityBatch(requestList);

        // Assert
        TestHelpers.AssertStatusCode(result, HttpStatusCode.OK);
        handler.Verify(a => a.GetAverageCarbonIntensityAsync(
            It.IsAny<string>(),
            It.IsAny<DateTimeOffset>(),
            It.IsAny<DateTimeOffset>()
        ), Times.Exactly(2));
    }

    /// <summary>
    /// GetEmissionsDataForLocationByTime: Tests that a success call to aggregator with no data returned results in action with No Content status.
    /// </summary>
    [Test]
    public async Task GetEmissionsDataForLocationByTime_EmptyResultReturnsNoContent()
    {
        var controller = new CarbonAwareController(this.MockCarbonAwareLogger.Object, CreateEmissionsHandler(new List<EmissionsData>()).Object, forecastHandler, CreateLocations(false).Object);

        string location = "Sydney";
        IActionResult result = await controller.GetEmissionsDataForLocationByTime(location);

        //Assert
        TestHelpers.AssertStatusCode(result, HttpStatusCode.NoContent);
    }

    /// <summary>
    /// GetEmissionsDataForLocationsByTime: Tests that a success call to aggregator with no data returned results in action with No Content status.
    /// </summary>
    [Test]
    public async Task GetEmissionsDataForLocationsByTime_EmptyResultReturnsNoContent()
    {
        var controller = new CarbonAwareController(this.MockCarbonAwareLogger.Object, CreateEmissionsHandler(new List<EmissionsData>()).Object, forecastHandler, CreateLocations(false).Object);

        string location = "Sydney";
        var parametersDTO = new EmissionsDataForLocationsParametersDTO() { MultipleLocations = new string[] { location } };

        IActionResult result = await controller.GetEmissionsDataForLocationsByTime(parametersDTO);

        //Assert
        TestHelpers.AssertStatusCode(result, HttpStatusCode.NoContent);
    }

    /// <summary>
    /// Tests that a success call to aggregator with no data returned results in action with No Content status.
    /// </summary>
    [Test]
    public async Task GetBestEmissions_EmptyResultReturnsNoContent()
    {
        var controller = new CarbonAwareController(this.MockCarbonAwareLogger.Object, CreateEmissionsHandler(new List<EmissionsData>()).Object, forecastHandler, CreateLocations(false).Object);
        var parametersDTO = new EmissionsDataForLocationsParametersDTO() { MultipleLocations = new string[] { "Sydney" } };

        IActionResult result = await controller.GetBestEmissionsDataForLocationsByTime(parametersDTO);

        //Assert
        TestHelpers.AssertStatusCode(result, HttpStatusCode.NoContent);
    }

    /// <summary>
    /// GetEmissionsDataForLocationsByTime: Tests empty or null location arrays throw ArgumentException.
    /// </summary>
    [TestCase(new object?[] { null, null }, TestName = "array of nulls: simulates 'location=&location=' empty value input")]
    [TestCase(new object?[] { null, }, TestName = "array of nulls: simulates 'location=' empty value input")]
    [TestCase(new object?[] { }, TestName = "empty array: simulates no 'location' query string")]
    public async Task GetEmissionsDataForLocationsByTime_NoLocations_ThrowsException(params string[] locations)
    {
        var controller = new CarbonAwareController(this.MockCarbonAwareLogger.Object, CreateEmissionsHandler(new List<EmissionsData>()).Object, forecastHandler, CreateLocations(false).Object);
        var parametersDTO = new EmissionsDataForLocationsParametersDTO() { MultipleLocations = locations };

        IActionResult result = await controller.GetBestEmissionsDataForLocationsByTime(parametersDTO);

        //Assert
        TestHelpers.AssertStatusCode(result, HttpStatusCode.NoContent);
    }
}
