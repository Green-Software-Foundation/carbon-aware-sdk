
using System.Collections.Generic;
using System.Net;
using CarbonAware.Model;
using CarbonAware.WebApi.Controllers;
using CarbonAware.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace CarbonAware.WepApi.UnitTests;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

/// <summary>
/// Tests that the Web API controller handles and packages various responses from a plugin properly 
/// including empty responses and exceptions.
/// </summary>
[TestFixture]
public class SciScoreControllerTests : TestsBase
{

    /// <summary>
    /// Tests that successfull call to the aggregator with any data returned results in action with OK status.
    /// </summary>
    [TestCase(LocationType.Geoposition)]
    [TestCase(LocationType.CloudProvider)]
    public async Task SuccessfulCallReturnsOk_MarginalCarbonIntensity(LocationType locationType)
    {
        // Arrange
        double data = 0.7;
        var controller = new SciScoreController(this.MockSciScoreLogger.Object, CreateSciScoreAggregator(data).Object);
        var location = new LocationInput() { LocationType = LocationType.Geoposition.ToString(), Latitude = (decimal)1.0, Longitude = (decimal)2.0 };
        string timeInterval = "2007-03-01T13:00:00Z/2007-03-01T15:30:00Z";
        SciScoreInput input = new SciScoreInput()
        {
            Location = location,
            TimeInterval = timeInterval
        };

        // Act
        var carbonIntensityOutput = (await controller.GetCarbonIntensityAsync(input)) as ObjectResult;

        // Assert
        TestHelpers.AssertStatusCode(carbonIntensityOutput, HttpStatusCode.OK);
        var expectedContent = new SciScore() { MarginalCarbonIntensityValue = data };
        var actualContent = (carbonIntensityOutput == null) ? string.Empty : carbonIntensityOutput.Value;
        Assert.AreEqual(expectedContent, actualContent);
    }

    /// <summary>
    /// Tests that invalid locationType inputs respond with a badRequest error
    /// </summary>
    [Test]
    public void InvalidLocationTypeReturnsBadRequest_MarginalCarbonIntensity()
    {
        // Arrange
        var data = 0.7;
        var controller = new SciScoreController(this.MockSciScoreLogger.Object, CreateSciScoreAggregator(data).Object);

        LocationInput locationInput = new LocationInput()
        {
            LocationType = "InvalidType"
        };

        string timeInterval = "";

        SciScoreInput input = new SciScoreInput()
        {
            Location = locationInput,
            TimeInterval = timeInterval
        };

        // Act/Assert
        Assert.ThrowsAsync<ArgumentException>(async () => await controller.GetCarbonIntensityAsync(input));
    }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
