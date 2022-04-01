// <copyright file="SubstrateServiceClientTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace CarbonAware.WepApi.UnitTests
{
    using System.Collections.Generic;
    using CarbonAware.Model;
    using CarbonAware.WebApi.Controllers;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Testing substrate service client.
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

            var or1 = ar1 as OkObjectResult;
            var or2 = ar2 as OkObjectResult;
            var or3 = ar3 as OkObjectResult;

            //Assert
            Assert.IsNotNull(or1);
            Assert.IsNotNull(or2);
            Assert.IsNotNull(or3);
        }

        [TestMethod]
        public async Task EmptyResultRetuns204()
        {
            this.SetupPluginWithData(new List<EmissionsData>());
            string location = "Sydney";

            IActionResult ar1 = await this.controller!.GetEmissionsDataForLocationByTime(location);
            IActionResult ar2 = await this.controller!.GetBestEmissionsDataForLocationsByTime(new string[] {location});
            IActionResult ar3 = await this.controller!.GetEmissionsDataForLocationsByTime(new string[] { location });

            var or1 = ar1 as ObjectResult;
            var or2 = ar2 as ObjectResult;
            var or3 = ar3 as ObjectResult;

            //Assert
            Assert.IsNotNull(or1);
            Assert.IsTrue(or1.StatusCode == 204);

            Assert.IsNotNull(or2);
            Assert.IsTrue(or2.StatusCode == 204);

            Assert.IsNotNull(or3);
            Assert.IsTrue(or3.StatusCode == 204);
        }

        [TestMethod]
        public async Task ExceptionReturns400()
        {
            this.SetupPluginWithException();
            string location = "Sydney";

            IActionResult ar1 = await this.controller!.GetEmissionsDataForLocationByTime(location);
            IActionResult ar2 = await this.controller!.GetBestEmissionsDataForLocationsByTime(new string[] { location });
            IActionResult ar3 = await this.controller!.GetEmissionsDataForLocationsByTime(new string[] { location });

            var or1 = ar1 as ObjectResult;
            var or2 = ar2 as ObjectResult;
            var or3 = ar3 as ObjectResult;

            //Assert
            Assert.IsNotNull(or1);
            Assert.IsTrue(or1.StatusCode == 400);

            Assert.IsNotNull(or2);
            Assert.IsTrue(or2.StatusCode == 400);

            Assert.IsNotNull(or3);
            Assert.IsTrue(or3.StatusCode == 400);
        }
    }
}
