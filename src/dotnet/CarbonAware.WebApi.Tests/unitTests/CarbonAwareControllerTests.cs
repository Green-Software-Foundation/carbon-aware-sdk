namespace CarbonAware.WepApi.UnitTests;

using System.Collections.Generic;
using CarbonAware.Model;
using CarbonAware.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

/// <summary>
/// Testing Web API Controller
/// </summary>
[TestFixture]
public class CarbonAwareControllerTests : TestsBase
{   
    /// <summary>
    /// Tests that successfull call to plugin results in action with 200 status
    /// </summary>
    [Test]
    public async Task SuccessfulCallReturnsOk()
    {
        string location = "Sydney";
        var data = new List<EmissionsData>()
        {
            new EmissionsData()
            {
                Location = location,
                Rating = 0.9,
                Time = DateTime.Now
            }
        };
        var controller = new CarbonAwareController(this.MockLogger.Object, CreatePluginWithData(data).Object);
        
        IActionResult ar1 = await controller.GetEmissionsDataForLocationByTime(location);
        IActionResult ar2 = await controller.GetBestEmissionsDataForLocationsByTime(new string[] { location });
        IActionResult ar3 = await controller.GetEmissionsDataForLocationsByTime(new string[] { location });

        TestHelpers.AssertStatusCode(ar1, 200);
        TestHelpers.AssertStatusCode(ar2, 200);
        TestHelpers.AssertStatusCode(ar3, 200);
    }

    /// <summary>
    /// Tests that empty result from plugin results in action with 204 status
    /// </summary>
    [Test]
    public async Task EmptyResultRetuns204()
    {
        var controller = new CarbonAwareController(this.MockLogger.Object, CreatePluginWithData(new List<EmissionsData>()).Object);
        
        string location = "Sydney";
        IActionResult ar1 = await controller.GetEmissionsDataForLocationByTime(location);
        IActionResult ar2 = await controller.GetBestEmissionsDataForLocationsByTime(new string[] {location});
        IActionResult ar3 = await controller.GetEmissionsDataForLocationsByTime(new string[] { location });

        //Assert
        TestHelpers.AssertStatusCode(ar1, 204);
        TestHelpers.AssertStatusCode(ar2, 204);
        TestHelpers.AssertStatusCode(ar3, 204);
    }

    /// <summary>
    /// Tests that exception in plugin results in action with 400 status
    /// </summary>
    [Test]
    public async Task ExceptionReturns400()
    {
        var controller = new CarbonAwareController(this.MockLogger.Object, CreatePluginWithException().Object);
 
        string location = "Sydney";
        IActionResult ar1 = await controller.GetEmissionsDataForLocationByTime(location);
        IActionResult ar2 = await controller.GetBestEmissionsDataForLocationsByTime(new string[] { location });
        IActionResult ar3 = await controller.GetEmissionsDataForLocationsByTime(new string[] { location });

        // Assert
        TestHelpers.AssertStatusCode(ar1, 400);
        TestHelpers.AssertStatusCode(ar2, 400);
        TestHelpers.AssertStatusCode(ar3, 400);
    }
}
