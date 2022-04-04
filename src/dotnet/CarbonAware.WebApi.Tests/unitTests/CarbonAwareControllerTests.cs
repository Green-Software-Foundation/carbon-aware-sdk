namespace CarbonAware.WepApi.UnitTests
{
    using System.Collections.Generic;
    using CarbonAware.Model;
    using CarbonAware.WebApi.Controllers;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Testing Web API Controller
    /// </summary>
    [TestClass]
    public class CarbonAwareControllerTests : TestsBase
    {   
        private CarbonAwareController? controller;

        [TestInitialize]
        public void TestInitialize()
        {
            this.controller = new CarbonAwareController(this.MockLogger.Object, this.MockPlugin.Object);
        }

        /// <summary>
        /// Tests that successfull call to plugin results in action with 200 status
        /// </summary>
        [TestMethod]
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
            this.SetupPluginWithData(data);
            

            IActionResult ar1 = await this.controller!.GetEmissionsDataForLocationByTime(location);
            IActionResult ar2 = await this.controller!.GetBestEmissionsDataForLocationsByTime(new string[] { location });
            IActionResult ar3 = await this.controller!.GetEmissionsDataForLocationsByTime(new string[] { location });

            TestHelpers.AssertStatusCode(ar1, 200);
            TestHelpers.AssertStatusCode(ar2, 200);
            TestHelpers.AssertStatusCode(ar3, 200);
        }

        /// <summary>
        /// Tests that empty result from plugin results in action with 204 status
        /// </summary>
        [TestMethod]
        public async Task EmptyResultRetuns204()
        {
            this.SetupPluginWithData(new List<EmissionsData>());
            string location = "Sydney";

            IActionResult ar1 = await this.controller!.GetEmissionsDataForLocationByTime(location);
            IActionResult ar2 = await this.controller!.GetBestEmissionsDataForLocationsByTime(new string[] {location});
            IActionResult ar3 = await this.controller!.GetEmissionsDataForLocationsByTime(new string[] { location });

            //Assert
            TestHelpers.AssertStatusCode(ar1, 204);
            TestHelpers.AssertStatusCode(ar2, 204);
            TestHelpers.AssertStatusCode(ar3, 204);
        }

        /// <summary>
        /// Tests that exception in plugin results in action with 400 status
        /// </summary>
        [TestMethod]
        public async Task ExceptionReturns400()
        {
            this.SetupPluginWithException();
            string location = "Sydney";

            IActionResult ar1 = await this.controller!.GetEmissionsDataForLocationByTime(location);
            IActionResult ar2 = await this.controller!.GetBestEmissionsDataForLocationsByTime(new string[] { location });
            IActionResult ar3 = await this.controller!.GetEmissionsDataForLocationsByTime(new string[] { location });

            // Assert
            TestHelpers.AssertStatusCode(ar1, 400);
            TestHelpers.AssertStatusCode(ar2, 400);
            TestHelpers.AssertStatusCode(ar3, 400);
        }
    }
}
