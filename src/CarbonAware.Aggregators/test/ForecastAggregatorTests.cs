using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.Aggregators.Forecast;
using CarbonAware.Aggregators.Tests;
using CarbonAware.Interfaces;
using CarbonAware.Model;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonAware.Aggregator.Tests;
public class ForecastAggregatorTests
{

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private Mock<ILogger<ForecastAggregator>> Logger { get; set; }
    private Mock<IForecastDataSource> ForecastDataSource { get; set; }
    private ForecastAggregator Aggregator { get; set; }
#pragma warning restore CS8618

    [SetUp]
    public void Setup()
    {
        this.Logger = new Mock<ILogger<ForecastAggregator>>();
        this.ForecastDataSource = new Mock<IForecastDataSource>();
        this.Aggregator = new ForecastAggregator(this.Logger.Object, this.ForecastDataSource.Object);
    }

    [TestCase(null, null, TestName = "no start param, no end param")]
    [TestCase("2022-01-01T00:05:00Z", null, TestName = "start param, no end param")]
    [TestCase(null, "2022-01-01T00:15:00Z", TestName = "no start param, end param")]
    [TestCase("2022-01-01T00:05:00Z", "2022-01-01T00:15:00Z", TestName = "start param, end param")]
    public async Task TestGetCurrentForecastDataAsync_StartAndEndUsePropsOrDefault(DateTimeOffset? start, DateTimeOffset? end)
    {
        // Arrange
        var forecast = TestData.GetForecast("2022-01-01T00:00:00Z");
        var firstDataPoint = forecast.ForecastData.First();
        var lastDataPoint = forecast.ForecastData.Last();
        var expectedStart = start ?? firstDataPoint.Time;
        var expectedEnd = end ?? lastDataPoint.Time + lastDataPoint.Duration;

        this.ForecastDataSource.Setup(x => x.GetCurrentCarbonIntensityForecastAsync(It.IsAny<Location>()))
            .ReturnsAsync(forecast);

        var parameters = new CarbonAwareParametersBaseDTO()
        {
            MultipleLocations = new string[] { "westus" },
            Start = start,
            End = end
        };

        // Act
        var results = await this.Aggregator.GetCurrentForecastDataAsync(parameters);

        // Assert
        var forecastResult = results.First();
        Assert.AreEqual(expectedStart, forecastResult.DataStartAt);
        Assert.AreEqual(expectedEnd, forecastResult.DataEndAt);
    }

    [Test]
    public void TestGetCurrentForecastDataAsync_NoLocation()
    {
        Assert.ThrowsAsync<ArgumentException>(
            async () => await this.Aggregator.GetCurrentForecastDataAsync(new CarbonAwareParametersBaseDTO()));
    }

    [TestCase("2022-01-01T00:00:00Z", "2022-01-01T00:20:00Z", ExpectedResult = 4, TestName = "Start and end time match")]
    [TestCase("2022-01-01T00:02:00Z", "2022-01-01T00:12:00Z", ExpectedResult = 3, TestName = "Start and end match midway through a datapoint")]
    public async Task<int> TestGetCurrentForecastDataAsync_FiltersDate(DateTimeOffset? start, DateTimeOffset? end)
    {
        this.ForecastDataSource.Setup(x => x.GetCurrentCarbonIntensityForecastAsync(It.IsAny<Location>()))
            .ReturnsAsync(TestData.GetForecast("2022-01-01T00:00:00Z"));
        var parameters = new CarbonAwareParametersBaseDTO()
        {
            MultipleLocations = new string[] { "westus" },
            Start = start,
            End = end
        };

        var results = await this.Aggregator.GetCurrentForecastDataAsync(parameters);
        var forecastData = results.First().ForecastData;

        return forecastData.Count();
    }

    [TestCase("2022-01-01T00:00:00Z", "2022-01-01T00:20:00Z", TestName = "Full data set")]
    [TestCase("2022-01-01T00:05:00Z", "2022-01-01T00:20:00Z", TestName = "Data set minus first lowest datapoint")]
    public async Task TestGetCurrentForecastDataAsync_OptimalDataPoint(DateTimeOffset? start, DateTimeOffset? end)
    {
        this.ForecastDataSource.Setup(x => x.GetCurrentCarbonIntensityForecastAsync(It.IsAny<Location>()))
            .ReturnsAsync(TestData.GetForecast("2022-01-01T00:00:00Z"));
        var parameters = new CarbonAwareParametersBaseDTO()
        {
            MultipleLocations = new string[] { "westus" },
            Start = start,
            End = end
        };

        var results = await this.Aggregator.GetCurrentForecastDataAsync(parameters);
        var forecast = results.First();
        var firstDataPoint = forecast.ForecastData.First();
        var optimalDataPoint = forecast.OptimalDataPoints.First();

        Assert.AreEqual(firstDataPoint, optimalDataPoint);
    }

    [Test]
    public async Task TestGetCurrentForecastDataAsync_RollingAverageWithSpecifiedWindow()
    {
        // Arrange
        var windowSize = 10;
        var dataTickSize = 5.0;
        var dataDuration = windowSize;
        var dataStartTime = new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var dataEndTime = new DateTimeOffset(2022, 1, 1, 0, 15, 0, TimeSpan.Zero);
        this.ForecastDataSource.Setup(x => x.GetCurrentCarbonIntensityForecastAsync(It.IsAny<Location>()))
            .ReturnsAsync(TestData.GetForecast("2022-01-01T00:00:00Z"));
        var locationName = "westus";
        var parameters = new CarbonAwareParametersBaseDTO()
        {
            MultipleLocations = new string[] { "westus" },
            Start = dataStartTime,
            End = dataEndTime,
            Duration = windowSize,
        };

        var expectedData = new List<EmissionsData>();
        var expectedRatings = new double[] { 15.0, 25.0 };
        for (var i = 0; i < expectedRatings.Count(); i++)
        {
            expectedData.Add(new EmissionsData() { Time = dataStartTime + i * TimeSpan.FromMinutes(dataTickSize), Location = locationName, Rating = expectedRatings[i], Duration = TimeSpan.FromMinutes(dataDuration) });
        }

        // Act
        var results = await this.Aggregator.GetCurrentForecastDataAsync(parameters);
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
        var dataStartTime = new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var dataEndTime = new DateTimeOffset(2022, 1, 1, 0, 15, 0, TimeSpan.Zero);
        this.ForecastDataSource.Setup(x => x.GetCurrentCarbonIntensityForecastAsync(It.IsAny<Location>()))
            .ReturnsAsync(TestData.GetForecast("2022-01-01T00:00:00Z"));
        var locationName = "westus";
        var parameters = new CarbonAwareParametersBaseDTO()
        {
            MultipleLocations = new string[] { locationName },
            Start = dataStartTime,
            End = dataEndTime,
        };


        var expectedData = new List<EmissionsData>();
        var expectedRatings = new double[] { 10.0, 20.0, 30.0, 40.0 };
        for (var i = 0; i < expectedRatings.Count(); i++)
        {
            expectedData.Add(new EmissionsData() { Time = dataStartTime + i * TimeSpan.FromMinutes(dataTickSize), Location = locationName, Rating = expectedRatings[i], Duration = TimeSpan.FromMinutes(dataDuration) });
        }

        // Act
        var results = await this.Aggregator.GetCurrentForecastDataAsync(parameters);
        var forecast = results.First();

        // Assert
        Assert.AreEqual(expectedData, forecast.ForecastData);
    }

    [TestCase("2021-12-31T00:00:00Z", "2022-01-01T00:20:00Z", TestName = "early startTime, valid endTime")]
    [TestCase("2022-01-01T00:00:00Z", "2022-01-01T00:30:00Z", TestName = "valid startTime, late endTime")]
    [TestCase("2021-12-31T00:00:00Z", null, TestName = "early startTime, default endTime")]
    [TestCase(null, "2022-01-01T00:30:00Z", TestName = "default startTime, late endTime")]
    [TestCase("2022-01-01T00:20:00Z", "2022-01-01T00:00:00Z", TestName = "startTime after endTime")]
    public void TestGetCurrentForecastDataAsync_InvalidStartEndTimes_ThrowsException(DateTimeOffset? start, DateTimeOffset? end)
    {
        this.ForecastDataSource.Setup(x => x.GetCurrentCarbonIntensityForecastAsync(It.IsAny<Location>()))
            .ReturnsAsync(TestData.GetForecast("2022-01-01T00:00:00Z"));
        var parameters = new CarbonAwareParametersBaseDTO()
        {
            MultipleLocations = new string[] { "westus" },
            Start = start,
            End = end
        };
        Assert.ThrowsAsync<ArgumentException>(async () => await Aggregator.GetCurrentForecastDataAsync(parameters));
    }

    [Test]
    public void TestGetForecastDataAsync_RequiredParameters()
    {
        var noRequiredParams = new CarbonAwareParametersBaseDTO();
        var noLocation = new CarbonAwareParametersBaseDTO() { Requested = new DateTimeOffset(2021, 9, 1, 8, 30, 0, TimeSpan.Zero) };
        var multipleLocations = new CarbonAwareParametersBaseDTO() { MultipleLocations = new string[] { "westus", "eastus" }, };
        var noRequestedAt = new CarbonAwareParametersBaseDTO() { SingleLocation = "westus" };

        Assert.ThrowsAsync<ArgumentException>(async () => await this.Aggregator.GetForecastDataAsync(noRequiredParams));
        Assert.ThrowsAsync<ArgumentException>(async () => await this.Aggregator.GetForecastDataAsync(noLocation));
        Assert.ThrowsAsync<ArgumentException>(async () => await this.Aggregator.GetForecastDataAsync(multipleLocations));
        Assert.ThrowsAsync<ArgumentException>(async () => await this.Aggregator.GetForecastDataAsync(noRequestedAt));
    }

    [TestCase("2021-12-31T00:00:00Z", "2022-01-01T00:20:00Z", TestName = "early startTime, valid endTime")]
    [TestCase("2022-01-01T00:00:00Z", "2022-01-01T00:30:00Z", TestName = "valid startTime, late endTime")]
    [TestCase("2021-12-31T00:00:00Z", null, TestName = "early startTime, default endTime")]
    [TestCase(null, "2022-01-01T00:30:00Z", TestName = "default startTime, late endTime")]
    [TestCase("2022-01-01T00:20:00Z", "2022-01-01T00:00:00Z", TestName = "startTime after endTime")]
    public void TestGetForecastDataAsync_InvalidStartEndTimes_ThrowsException(DateTimeOffset? start, DateTimeOffset? end)
    {
        var requested = new DateTimeOffset(2021, 01, 01, 00, 00, 0, TimeSpan.Zero);
        this.ForecastDataSource.Setup(x => x.GetCarbonIntensityForecastAsync(It.IsAny<Location>(), requested))
            .ReturnsAsync(TestData.GetForecast("2022-01-01T00:00:00Z"));
        var parameters = new CarbonAwareParametersBaseDTO()
        {
            SingleLocation = "westus",
            Start = start,
            End = end,
            Requested = requested
        };
        Assert.ThrowsAsync<ArgumentException>(async () => await Aggregator.GetForecastDataAsync(parameters));
    }

    [TestCase("2022-01-01T00:00:00Z", "2022-01-01T00:20:00Z", TestName = "Full data set")]
    [TestCase("2022-01-01T00:05:00Z", "2022-01-01T00:20:00Z", TestName = "Data set minus first lowest datapoint")]
    public async Task TestGetForecastDataAsync_OptimalDataPoint(DateTimeOffset? start, DateTimeOffset? end)
    {
        var requested = new DateTimeOffset(2021, 01, 01, 00, 00, 0, TimeSpan.Zero);
        this.ForecastDataSource.Setup(x => x.GetCarbonIntensityForecastAsync(It.IsAny<Location>(), requested))
            .ReturnsAsync(TestData.GetForecast("2022-01-01T00:00:00Z"));
        var parameters = new CarbonAwareParametersBaseDTO()
        {
            SingleLocation = "westus",
            Start = start,
            End = end,
            Requested = requested
        };

        var forecast = await this.Aggregator.GetForecastDataAsync(parameters);
        var firstDataPoint = forecast.ForecastData.First();
        var optimalDataPoint = forecast.OptimalDataPoints.First();

        Assert.AreEqual(firstDataPoint, optimalDataPoint);
    }

    [Test]
    public async Task TestGetForecastDataAsync_Metadata()
    {
        var requested = new DateTimeOffset(2021, 01, 01, 00, 00, 0, TimeSpan.Zero);// "2021-01-01T00:00:00Z";
        var region = "westus";
        var start = new DateTimeOffset(2022, 01, 01, 00, 00, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2022, 01, 01, 00, 20, 0, TimeSpan.Zero);
        this.ForecastDataSource.Setup(x => x.GetCarbonIntensityForecastAsync(It.IsAny<Location>(), requested))
            .ReturnsAsync(TestData.GetForecast("2022-01-01T00:00:00Z"));
        var parameters = new CarbonAwareParametersBaseDTO()
        {
            SingleLocation = region,
            Start = start,
            End = end,
            Requested = requested
        };

        var forecast = await this.Aggregator.GetForecastDataAsync(parameters);
        Assert.AreEqual(region, forecast.Location.Name);
        Assert.AreEqual(start, forecast.DataStartAt);
        Assert.AreEqual(end, forecast.DataEndAt);
    }
}
