using NUnit.Framework;
using Moq;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using CarbonAware.Model;
using System;
using System.Linq;
using Moq.Protected;
using System.Diagnostics;

namespace CarbonAware.DataSources.Json.Tests;

public class JsonDataSourceTests
{
    [Test]
    public async Task TestDataByLocation_WhenMultipleLocationsProvided()
    {
        var mockDataSource = SetupMockDataSource();

        var location1 = new Location() { RegionName = "eastus"};
        var location2 = new Location() { RegionName = "westus"};
        IEnumerable<Location> locations = new List<Location>() { location1, location2 };
        var start = new DateTimeOffset(new DateTime(2021,8,9));
        var end = new DateTimeOffset(new DateTime(2022,4,9));
        var dataSource = mockDataSource.Object;
        var result = await dataSource.GetCarbonIntensityAsync(locations, start, end);
        Assert.AreEqual(3, result.Count());

        foreach (var r in result) {
            Assert.IsTrue(locations.Where(loc => loc.RegionName == r.Location).Any());
        }
    }

    [Test]
    public async Task TestDataByLocationAndTimePeriod()
    {
        var mockDataSource = SetupMockDataSource();
        
        var location = new Location {RegionName = "eastus"};
        var start = new DateTimeOffset(new DateTime(2021,8,9));
        var end = new DateTimeOffset(new DateTime(2021,12,9));

        var dataSource = mockDataSource.Object;
        var result = await dataSource.GetCarbonIntensityAsync(new List<Location>() { location }, start, end);
        
        Assert.AreEqual(1, result.Count());
        Assert.AreEqual("eastus", result.First().Location);
    }

    [Test]
    public async Task TestData_WhenNoMatchedCriteria()
    {
        var mockDataSource = SetupMockDataSource();
        
        var location = new Location {RegionName = "paris"};
        var start = new DateTimeOffset(new DateTime(2021,8,9));
        var end = new DateTimeOffset(new DateTime(2021,12,9));
        
        var dataSource = mockDataSource.Object;
        var result = await dataSource.GetCarbonIntensityAsync(new List<Location>() { location }, start, end);
        
        Assert.AreEqual(0, result.Count());
    }

    private Mock<JsonDataSource> SetupMockDataSource() {
        var logger = Mock.Of<ILogger<JsonDataSource>>();
        var mockDataSource = new Mock<JsonDataSource>(logger);
        
        mockDataSource.Protected()
            .Setup<List<EmissionsData>>("GetSampleJson")
            .Returns(GetTestEmissionData())
            .Verifiable();

        return mockDataSource;
    }
    private List<EmissionsData> GetTestEmissionData() {
        // All the tests above correspond to values in this mock data. If the mock values are changed, the tests need to be updated 
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