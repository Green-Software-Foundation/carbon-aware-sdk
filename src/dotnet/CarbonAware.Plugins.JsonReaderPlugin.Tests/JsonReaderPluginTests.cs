using NUnit.Framework;
using CarbonAware.Plugins.JsonReaderPlugin;
using CarbonAware;
using Moq;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using CarbonAware.Model;
using System;
using System.Linq;
using Moq.Protected;

namespace CarbonAware.Plugins.JsonReaderPlugin.Tests;

public class JsonReaderPluginTests
{

    [Test]
    public async Task TestDataByLocation_WhenLocationProvided()
    {
        var mockPlugin = SetupMockPlugin();

        Dictionary<string, object> props = new Dictionary<string, object>();
        props[CarbonAwareConstants.Locations] = new List<string> { "eastus"};
        var plugin = mockPlugin.Object;
        var result = await plugin.GetEmissionsDataAsync(props);
        
        Assert.AreEqual(2, result.Count());
        foreach (var e in result) {
             Assert.AreEqual("eastus", e.Location);
        }
    }

    [Test]
    public async Task TestDataByLocation_WhenLocationNotProvided()
    {
        var mockPlugin = SetupMockPlugin();

        var plugin = mockPlugin.Object;
        var result = await plugin.GetEmissionsDataAsync(new Dictionary<string, object>());
        
        Assert.AreEqual(4, result.Count());
    }

    [Test]
    public async Task TestDataByLocation_WhenMultipleLocationsProvided()
    {
        var mockPlugin = SetupMockPlugin();

        Dictionary<string, object> props = new Dictionary<string, object>();
        props[CarbonAwareConstants.Locations] = new List<string> { "eastus", "westus"};
        var plugin = mockPlugin.Object;
        var result = await plugin.GetEmissionsDataAsync(props);
        List<string> locations = (List<string>)props[CarbonAwareConstants.Locations];
        Assert.AreEqual(3, result.Count());

        foreach (var r in result) {
             Assert.IsTrue(locations.Contains(r.Location));
        }
    }

    [Test]
    public async Task TestDataByLocationAndTimePeriod()
    {
        var mockPlugin = SetupMockPlugin();
        
        Dictionary<string, object> props = new Dictionary<string, object>();
        props[CarbonAwareConstants.Locations] = new List<string> { "eastus"};
        props[CarbonAwareConstants.Start] = "2021-08-09";
        props[CarbonAwareConstants.End] = "2021-12-09";

        var plugin = mockPlugin.Object;
        var result = await plugin.GetEmissionsDataAsync(props);
        
        Assert.AreEqual(1, result.Count());
        Assert.AreEqual("eastus", result.First().Location);
    }

    [Test]
    public async Task TestData_WhenNoEndTimeSpecified()
    {
        var mockPlugin = SetupMockPlugin();
        
        Dictionary<string, object> props = new Dictionary<string, object>();
        props[CarbonAwareConstants.Locations] = new List<string> { "eastus"};
        props[CarbonAwareConstants.Start] = "2021-12-09";

        var plugin = mockPlugin.Object;
        var result = await plugin.GetEmissionsDataAsync(props);
        
        Assert.AreEqual(1, result.Count());
        Assert.AreEqual("eastus", result.First().Location);
        Assert.AreEqual(DateTime.Parse("2021-09-01"), result.First().Time);
    }

    [Test]
    public async Task TestData_WhenNoMatchedCriteria()
    {
        var mockPlugin = SetupMockPlugin();
        
        Dictionary<string, object> props = new Dictionary<string, object>();
        props[CarbonAwareConstants.Locations] = new List<string> { "xyz"};
        
        var plugin = mockPlugin.Object;
        var result = await plugin.GetEmissionsDataAsync(props);
        
        Assert.AreEqual(0, result.Count());
    }

    private Mock<CarbonAwareJsonReaderPlugin> SetupMockPlugin() {
        var logger = Mock.Of<ILogger<CarbonAwareJsonReaderPlugin>>();
        var mockPlugin = new Mock<CarbonAwareJsonReaderPlugin>(logger);
        
        mockPlugin.Protected()
            .Setup<List<EmissionsData>>("GetSampleJson")
            .Returns(GetTestEmissionData())
            .Verifiable();

        return mockPlugin;
    }
    private List<EmissionsData> GetTestEmissionData() {
        return new List<EmissionsData>() {
                new EmissionsData {
                    Location = "eastus",
                    Time = DateTime.Parse("2021-09-01")
                },
                new EmissionsData {
                    Location = "westus",
                    Time = DateTime.Parse("2021-12-01")
                },
                new EmissionsData {
                    Location = "eastus",
                    Time = DateTime.Parse("2022-02-01")
                },
                new EmissionsData {
                    Location = "midwest",
                    Time = DateTime.Parse("2021-05-01")
                }
            };
    }
}