using CarbonAware.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using System.Net;

namespace CarbonAware.WepApi.UnitTests;

/// <summary>
/// Tests that the Web API controller handles request for Location instances 
/// </summary>
[TestFixture]
public class LocationsControllerTests : TestsBase
{
    /// <summary>
    /// Test result with content
    /// </summary>
    [Test]
    public async Task GetLocations_ResultsOk()
    {
        var controller = new LocationsController(this.MockCarbonAwareLogger.Object, CreateLocations().Object);

        IActionResult result = await controller.GetAllLocations();

        //Assert
        TestHelpers.AssertStatusCode(result, HttpStatusCode.OK);
    }

    /// <summary>
    /// Test result without content
    /// </summary>
    [Test]
    public async Task GetLocations_ResultsNoContent()
    {
        var controller = new LocationsController(this.MockCarbonAwareLogger.Object, CreateLocations(false).Object);

        IActionResult result = await controller.GetAllLocations();

        //Assert
        TestHelpers.AssertStatusCode(result, HttpStatusCode.NoContent);
    }
}
