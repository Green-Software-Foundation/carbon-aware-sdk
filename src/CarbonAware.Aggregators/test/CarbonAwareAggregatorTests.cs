using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.Aggregators.Configuration;
using CarbonAware.Interfaces;
using CarbonAware.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CarbonAware.Aggregators.Tests;

[TestFixture]
public class CarbonAwareAggregatorTests
{
    // Test class sets these fields in [SetUp] rather than traditional class constructor.
    #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private Mock<ILogger<CarbonAwareAggregator>> Logger { get; set; }
    private Mock<ICarbonIntensityDataSource> CarbonIntensityDataSource { get; set; }
    private CarbonAwareAggregator Aggregator { get; set; }
    #pragma warning restore CS8618

    [SetUp]
    public void Setup()
    {
        this.Logger = new Mock<ILogger<CarbonAwareAggregator>>();
        this.CarbonIntensityDataSource = new Mock<ICarbonIntensityDataSource>();
        this.Aggregator = new CarbonAwareAggregator(this.Logger.Object, this.CarbonIntensityDataSource.Object);
    }

    [TestCase(null, null, TestName = "no start param, no end param")]
    [TestCase("2022-01-01T00:05:00Z", null, TestName = "start param, no end param")]
    [TestCase(null, "2022-01-01T00:15:00Z", TestName = "no start param, end param")]
    [TestCase("2022-01-01T00:05:00Z", "2022-01-01T00:15:00Z", TestName = "start param, end param")]
    public async Task TestGetCurrentForecastDataAsync_StartAndEndUsePropsOrDefault(string start, string end)
    {
        // Arrange
        var forecast = TestData.GetForecast("2022-01-01T00:00:00Z");
        var firstDataPoint = forecast.ForecastData.First();
        var lastDataPoint = forecast.ForecastData.Last();
        var expectedStart = start != null ? DateTimeOffset.Parse(start) : firstDataPoint.Time;
        var expectedEnd = end != null ? DateTimeOffset.Parse(end) : lastDataPoint.Time + lastDataPoint.Duration;

        this.CarbonIntensityDataSource.Setup(x => x.GetCurrentCarbonIntensityForecastAsync(It.IsAny<Location>()))
            .ReturnsAsync(forecast);

        var props = new Dictionary<string, object?>()
        {
            { CarbonAwareConstants.Locations, new List<Location>() { new Location() { RegionName = "westus" } } },
            { CarbonAwareConstants.Start, start },
            { CarbonAwareConstants.End, end },
        };

        // Act
        var results = await this.Aggregator.GetCurrentForecastDataAsync(props);

        // Assert
        var forecastResult = results.First();
        Assert.AreEqual(expectedStart, forecastResult.StartTime);
        Assert.AreEqual(expectedEnd, forecastResult.EndTime);
    }

    [Test]
    public async Task TestGetCurrentForecastDataAsync_NoLocation()
    {        
        var emptyProps = new Dictionary<string, object>();
        var emptyLocationProps = new Dictionary<string, object>()
        {
            { CarbonAwareConstants.Locations, new List<Location>() }
        };

        Assert.ThrowsAsync<ArgumentException>(async () => await this.Aggregator.GetCurrentForecastDataAsync(emptyProps));
        var results = await this.Aggregator.GetCurrentForecastDataAsync(emptyLocationProps);
        Assert.IsEmpty(results);
    }

    [TestCase("2022-01-01T00:00:00Z", "2022-01-01T00:20:00Z", ExpectedResult = 4, TestName = "Start and end time match")]
    [TestCase("2022-01-01T00:02:00Z", "2022-01-01T00:12:00Z", ExpectedResult = 3, TestName = "Start and end match midway through a datapoint")]
    public async Task<int> TestGetCurrentForecastDataAsync_FiltersDate(string start, string end)
    {
        this.CarbonIntensityDataSource.Setup(x => x.GetCurrentCarbonIntensityForecastAsync(It.IsAny<Location>()))
            .ReturnsAsync(TestData.GetForecast("2022-01-01T00:00:00Z"));
        var props = new Dictionary<string, object>()
        {
            { CarbonAwareConstants.Locations, new List<Location>() { new Location() { RegionName = "westus" } } },
            { CarbonAwareConstants.Start, start },
            { CarbonAwareConstants.End, end }
        };

        var results = await this.Aggregator.GetCurrentForecastDataAsync(props);
        var forecastData = results.First().ForecastData;

        return forecastData.Count();
    }

    [TestCase("2022-01-01T00:00:00Z", "2022-01-01T00:20:00Z", TestName = "Full data set")]
    [TestCase("2022-01-01T00:05:00Z", "2022-01-01T00:20:00Z", TestName = "Data set minus first lowest datapoint")]
    public async Task TestGetCurrentForecastDataAsync_OptimalDataPoint(string start, string end)
    {
        this.CarbonIntensityDataSource.Setup(x => x.GetCurrentCarbonIntensityForecastAsync(It.IsAny<Location>()))
            .ReturnsAsync(TestData.GetForecast("2022-01-01T00:00:00Z"));
        var props = new Dictionary<string, object>()
        {
            { CarbonAwareConstants.Locations, new List<Location>() { new Location() { RegionName = "westus" } } },
            { CarbonAwareConstants.Start, start },
            { CarbonAwareConstants.End, end }
        };

        var results = await this.Aggregator.GetCurrentForecastDataAsync(props);
        var forecast = results.First();
        var firstDataPoint = forecast.ForecastData.First();
        var optimalDataPoint = forecast.OptimalDataPoint;

        Assert.AreEqual(firstDataPoint, optimalDataPoint);
    }

    [Test]
    public async Task TestGetCurrentForecastDataAsync_RollingAverageWithSpecifiedWindow()
    {
        // Arrange
        var windowSize = 10;
        var dataTickSize = 5.0;
        var dataDuration = windowSize;
        var dataStartTime = new DateTimeOffset(2022,1,1,0,0,0,TimeSpan.Zero);
        this.CarbonIntensityDataSource.Setup(x => x.GetCurrentCarbonIntensityForecastAsync(It.IsAny<Location>()))
            .ReturnsAsync(TestData.GetForecast("2022-01-01T00:00:00Z"));
        var locationName = "westus";
        var props = new Dictionary<string, object>()
        {
            { CarbonAwareConstants.Locations, new List<Location>() { new Location() { RegionName = "westus" } } },
            { CarbonAwareConstants.Start, "2022-01-01T00:00:00Z" },
            { CarbonAwareConstants.End, "2022-01-01T00:15:00Z" },
            { CarbonAwareConstants.Duration, windowSize }
        };

        var expectedData = new List<EmissionsData>();
        var expectedRatings = new double[] { 15.0, 25.0, 35.0 };
        for(var i = 0; i < expectedRatings.Count(); i++)
        {
            expectedData.Add(new EmissionsData() { Time = dataStartTime + i * TimeSpan.FromMinutes(dataTickSize), Location = locationName, Rating = expectedRatings[i], Duration = TimeSpan.FromMinutes(dataDuration) });
        }

        // Act
        var results = await this.Aggregator.GetCurrentForecastDataAsync(props);
        var forecast = results.First();

        // Assert
        Assert.AreEqual(expectedData, forecast.ForecastData);
    }

    [Test]
    public async Task TestGetCurrentForecastDataAsync_RollingAverageWithNoSpecifiedWindow()
    {
        // Arrange
        var dataTickSize = 5.0;
        var dataDuration = 5;
        var dataStartTime = new DateTimeOffset(2022,1,1,0,0,0,TimeSpan.Zero);
        this.CarbonIntensityDataSource.Setup(x => x.GetCurrentCarbonIntensityForecastAsync(It.IsAny<Location>()))
            .ReturnsAsync(TestData.GetForecast("2022-01-01T00:00:00Z"));
        var locationName = "westus";
        var props = new Dictionary<string, object>()
        {
            { CarbonAwareConstants.Locations, new List<Location>() { new Location() { RegionName = "westus" } } },
            { CarbonAwareConstants.Start, "2022-01-01T00:00:00Z" },
            { CarbonAwareConstants.End, "2022-01-01T00:20:00Z" },
        };

        var expectedData = new List<EmissionsData>();
        var expectedRatings = new double[] { 10.0, 20.0, 30.0, 40.0 };
        for(var i = 0; i < expectedRatings.Count(); i++)
        {
            expectedData.Add(new EmissionsData() { Time = dataStartTime + i * TimeSpan.FromMinutes(dataTickSize), Location = locationName, Rating = expectedRatings[i], Duration = TimeSpan.FromMinutes(dataDuration) });
        }

        // Act
        var results = await this.Aggregator.GetCurrentForecastDataAsync(props);
        var forecast = results.First();

        // Assert
        Assert.AreEqual(expectedData, forecast.ForecastData);
    }

    [TestCase("2021-12-31T00:00:00Z", "2022-01-01T00:20:00Z", TestName = "early startTime, valid endTime")]
    [TestCase("2022-01-01T00:00:00Z", "2022-01-01T00:30:00Z", TestName = "valid startTime, late endTime")]
    [TestCase("2021-12-31T00:00:00Z", null, TestName = "early startTime, default endTime")]
    [TestCase(null, "2022-01-01T00:30:00Z", TestName = "default startTime, late endTime")]
    [TestCase("2022-01-01T00:20:00Z", "2022-01-01T00:00:00Z", TestName = "startTime after endTime")]
    public void TestGetCurrentForecastDataAsync_InvalidStartEndTimes_ThrowsException(string start, string end)
    {
        this.CarbonIntensityDataSource.Setup(x => x.GetCurrentCarbonIntensityForecastAsync(It.IsAny<Location>()))
            .ReturnsAsync(TestData.GetForecast("2022-01-01T00:00:00Z"));
        var props = new Dictionary<string, object?>()
        {
            { CarbonAwareConstants.Locations, new List<Location>() { new Location() { RegionName = "westus" } } },
            { CarbonAwareConstants.Start, start },
            { CarbonAwareConstants.End, end }
        };
        Assert.ThrowsAsync<ArgumentException>(async () => await Aggregator.GetCurrentForecastDataAsync(props));
    }
}
