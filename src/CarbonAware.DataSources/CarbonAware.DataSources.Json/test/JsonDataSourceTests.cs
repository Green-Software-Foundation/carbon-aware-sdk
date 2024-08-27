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
using CarbonAware.DataSources.Json.Mocks;

namespace CarbonAware.DataSources.Json.Tests;

class JsonDataSourceTests
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

    [Test]
    public async Task GetCarbonIntensityAsync_ReturnsEmptyEmissionData()
    {
        var logger = Mock.Of<ILogger<JsonDataSource>>();
        var monitor = Mock.Of<IOptionsMonitor<JsonDataSourceConfiguration>>();
        var mockDataSource = new Mock<JsonDataSource>(logger, monitor);
        
        mockDataSource.Protected()
            .Setup<Task<List<EmissionsData>?>>("GetJsonDataAsync")
            .ReturnsAsync(new List<EmissionsData>())
            .Verifiable();

        var location = new Location() { Name = "midwest" };
        var locations = new List<Location>() { location };
        var start = DateTimeOffset.Parse("2022-09-07T12:45:11+00:00");
        var end = DateTimeOffset.Parse("2022-09-07T13:45:11+00:00");
        var dataSource = mockDataSource.Object;
        var result = await dataSource.GetCarbonIntensityAsync(locations, start, end);
        Assert.That(result.Count(), Is.EqualTo(0));
        Assert.That(!result.Any(), Is.True);
    }

    [TestCase(true, TestName = "Test JsonDataSource with caching")]
    [TestCase(false, TestName = "Test JsonDataSource without caching")]
    public async Task GetCarbonIntensityAsync_CacheEmissionData(bool cache)
    {
        var logger = Mock.Of<ILogger<JsonDataSource>>();

        var monitor = new Mock<IOptionsMonitor<JsonDataSourceConfiguration>>();
        var config = new JsonDataSourceConfiguration
        {
            CacheJsonData = cache
        };
        monitor.Setup(m => m.CurrentValue).Returns(config);
        var dataSource = new JsonDataSource(logger, monitor.Object);

        JsonDataSourceMocker dsMocker = new();

        var today = DateTimeOffset.Now;
        var todayEnd = today.AddMinutes(30);
        var todayLocation = new Location() { Name = "japan" };
        dsMocker.SetupDataMock(today, todayEnd, todayLocation.Name);
        var result1 = await dataSource.GetCarbonIntensityAsync(todayLocation, today, todayEnd);
        Assert.AreEqual(1, result1.Count());
        foreach (var r in result1)
        {
            Assert.AreEqual(r.Location, todayLocation.Name);
        }

        var yesterday = today.AddDays(-1);
        var yesterdayEnd = yesterday.AddMinutes(30);
        var yesterdayLocation = new Location() { Name = "uk" };
        dsMocker.SetupDataMock(yesterday, yesterdayEnd, yesterdayLocation.Name);
        if (cache)
        {
            var result2 = await dataSource.GetCarbonIntensityAsync(todayLocation, today, todayEnd);
            Assert.AreEqual(1, result2.Count());
            foreach (var r in result2)
            {
                Assert.AreEqual(r.Location, todayLocation.Name);
            }
        } else
        {
            var result2 = await dataSource.GetCarbonIntensityAsync(yesterdayLocation, yesterday, yesterdayEnd);
            Assert.AreEqual(1, result2.Count());
            foreach (var r in result2)
            {
                Assert.AreEqual(r.Location, yesterdayLocation.Name);
            }
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