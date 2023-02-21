using NUnit.Framework;
using Moq;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using CarbonAware.Model;
using System;
using System.Linq;
using Moq.Protected;
using Microsoft.Extensions.Options;
using CarbonAware.DataSources.Json.Configuration;

namespace CarbonAware.DataSources.Json.Tests;

public class JsonDataSourceTests
{
    [Test]
    public async Task GetCarbonIntensityAsync_ByLocationMultiple()
    {
        var mockDataSource = SetupMockDataSource();

        var location1 = new Location() { Name = "eastus"};
        var location2 = new Location() { Name = "westus"};
        IEnumerable<Location> locations = new List<Location>() { location1, location2 };
        var start = new DateTimeOffset(2021,8,9,0,0,0,TimeSpan.Zero);
        var end = new DateTimeOffset(2022,4,9,0,0,0,TimeSpan.Zero);
        var dataSource = mockDataSource.Object;
        var result = await dataSource.GetCarbonIntensityAsync(locations, start, end);
        Assert.AreEqual(3, result.Count());

        foreach (var r in result) {
            Assert.IsTrue(locations.Where(loc => loc.Name == r.Location).Any());
        }
    }

    [Test]
    public async Task GetCarbonIntensityAsync_ByLocationAndTimePeriod()
    {
        var mockDataSource = SetupMockDataSource();
        
        var location = new Location {Name = "eastus"};
        var start = new DateTimeOffset(2021,8,9,0,0,0,TimeSpan.Zero);
        var end = new DateTimeOffset(2021,12,9,0,0,0,TimeSpan.Zero);

        var dataSource = mockDataSource.Object;
        var result = await dataSource.GetCarbonIntensityAsync(new List<Location>() { location }, start, end);
        
        Assert.AreEqual(1, result.Count());
        Assert.AreEqual("eastus", result.First().Location);
    }

    [Test]
    public async Task GetCarbonIntensityAsync_NoMatchedCriteria()
    {
        var mockDataSource = SetupMockDataSource();
        
        var location = new Location {Name = "paris"};
        var start = new DateTimeOffset(2021,8,9,0,0,0,TimeSpan.Zero);
        var end = new DateTimeOffset(2021,12,9,0,0,0,TimeSpan.Zero);
        
        var dataSource = mockDataSource.Object;
        var result = await dataSource.GetCarbonIntensityAsync(new List<Location>() { location }, start, end);
        
        Assert.AreEqual(0, result.Count());
    }

    [Test]
    public async Task GetCarbonIntensityAsync_ReturnsSingleDataPoint_WhenStartParamExactlyMatchesDataStart()
    {
        var mockDataSource = SetupMockDataSource();

        var location = new Location() { Name = "midwest" };
        var locations = new List<Location>() { location };
        var start = DateTimeOffset.Parse("2022-09-07T12:45:11+00:00");
        var end = DateTimeOffset.Parse("2022-09-07T13:45:11+00:00");
        var dataSource = mockDataSource.Object;
        var result = await dataSource.GetCarbonIntensityAsync(locations, start, end);
        Assert.AreEqual(1, result.Count());

        foreach (var r in result)
        {
            Assert.IsTrue(locations.Where(loc => loc.Name == r.Location).Any());
        }
    }
    private Mock<JsonDataSource> SetupMockDataSource() {
        var logger = Mock.Of<ILogger<JsonDataSource>>();
        var monitor = Mock.Of<IOptionsMonitor<JsonDataSourceConfiguration>>();
        var mockDataSource = new Mock<JsonDataSource>(logger, monitor);
        
        mockDataSource.Protected()
            .Setup<Task<List<EmissionsData>?>>("GetJsonDataAsync")
            .ReturnsAsync(GetTestEmissionData)
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
                    Time = DateTime.Parse("2022-09-07T04:45:11+00:00"),
                    Duration = TimeSpan.FromHours(8)
                },
                new EmissionsData {
                    Location = "midwest",
                    Time = DateTime.Parse("2022-09-07T12:45:11+00:00"),
                    Duration = TimeSpan.FromHours(8)
                },
                new EmissionsData {
                    Location = "midwest",
                    Time = DateTime.Parse("2022-09-07T20:45:11+00:00"),
                    Duration = TimeSpan.FromHours(8)
                }
            };
    }
}