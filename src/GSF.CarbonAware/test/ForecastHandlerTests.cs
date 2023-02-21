using CarbonAware.Interfaces;
using CarbonAware.Model;
using GSF.CarbonAware.Exceptions;
using GSF.CarbonAware.Handlers;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GSF.CarbonAware.Tests;

[TestFixture]
public class ForecastHandlerTests
{
    private Mock<ILogger<ForecastHandler>>? Logger { get; set; }

    [SetUp]
    public void SetUp()
    {
        Logger = new Mock<ILogger<ForecastHandler>>();
    }

    [TestCase("Sydney", "eastus", "2022-03-07T01:00:00", "2022-03-07T01:10:00", 5, TestName = "GetCurrentForecastAsync calls datasource: all fields")]
    [TestCase("Sydney", null, null, null, null, TestName = "GetCurrentForecastAsync calls datasource: required fields only")]
    public async Task GetCurrentForecastAsync_Succeed_Call_MockDataSource_WithOutputData(string location1, string? location2, DateTimeOffset? start, DateTimeOffset? end, int? duration)
    {
        var data = new global::CarbonAware.Model.EmissionsForecast {
                RequestedAt = DateTimeOffset.Now,
                GeneratedAt = DateTimeOffset.Now - TimeSpan.FromMinutes(1),
                ForecastData = TestData.GetForecast("2022-03-07T01:00:00").ForecastData
        };

        var datasource = CreateCurrentForecastDataSource(data);
        var handler = new ForecastHandler(Logger!.Object, datasource.Object);
        var locations = location2 != null ? new string[] { location1, location2 } : new string[] { location1 };
        var result = await handler.GetCurrentForecastAsync(locations, start, end, duration);
        Assert.That(result, Is.Not.Empty);
    }

    [TestCase("Sydney","2022-03-07T01:00:00", "2022-03-07T01:10:00", "2022-03-07T01:10:00", 5, TestName = "GetForecastByDate calls datasource: all fields")]
    [TestCase("Sydney", null, null, "2022-03-07T01:00:00", null, TestName = "GetForecastByDate calls datasource: required fields only")]
    public async Task GetForecastByDateAsync_Succeed_Call_MockDataSource_WithOutputData(string location, DateTimeOffset? start, DateTimeOffset? end, DateTimeOffset requestedAt, int? duration)
    {
        var data = new global::CarbonAware.Model.EmissionsForecast {
            RequestedAt = requestedAt,
            GeneratedAt = DateTimeOffset.UtcNow - TimeSpan.FromMinutes(1),
            ForecastData = TestData.GetForecast("2022-03-07T01:00:00").ForecastData,
        };

        var datasource = CreateForecastByDateDataSource(data, requestedAt);
        var handler = new ForecastHandler(Logger!.Object, datasource.Object);
        var result = await handler.GetForecastByDateAsync(location, start, end, requestedAt, duration);
        Assert.That(result.RequestedAt, Is.EqualTo(requestedAt));
    }

    [Test]
    public void ForecastHandler_WrapsCarbonAwareExceptionsCorrectly()
    {
        var datasource = SetupMockDataSourceThatThrows();
        var handler = new ForecastHandler(Logger!.Object, datasource.Object);
        Assert.ThrowsAsync<CarbonAwareException>(async () => await handler.GetCurrentForecastAsync(new string[] { "eastus" }));
        Assert.ThrowsAsync<CarbonAwareException>(async () => await handler.GetForecastByDateAsync("eastus", null, null, DateTimeOffset.Parse("2022-03-07T01:00:00")));
    }

    [Test]
    public void ForecastHandler_WrapsSystemExceptionsCorrectly()
    {
        var datasourceMock = Mock.Of<IForecastDataSource>();
        var handler = new ForecastHandler(Logger!.Object, datasourceMock);
        Assert.ThrowsAsync<ArgumentException>(async () => await handler.GetCurrentForecastAsync(It.IsAny<string[]>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<int>()));
        Assert.ThrowsAsync<ArgumentException>(async () => await handler.GetForecastByDateAsync(It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<int>()));
    }

    [TestCase("2022-01-01T00:00:00Z", "2022-01-01T00:20:00Z", ExpectedResult = 4, TestName = "Start and end time match")]
    [TestCase("2022-01-01T00:02:00Z", "2022-01-01T00:12:00Z", ExpectedResult = 3, TestName = "Start and end match midway through a datapoint")]
    public async Task<int> GetCurrentForecastDataAsync_FiltersDate(DateTimeOffset? start, DateTimeOffset? end)
    {
        var datasource = CreateCurrentForecastDataSource(TestData.GetForecast("2022-01-01T00:00:00Z"));
        var handler = new ForecastHandler(Logger!.Object, datasource.Object);
        var results = await handler.GetCurrentForecastAsync(new string[] { "westus" }, start, end);
        var forecastData = results.First().EmissionsDataPoints;

        return forecastData.Count();
    }

    [TestCase("2022-01-01T00:00:00Z", "2022-01-01T00:20:00Z", TestName = "Full data set")]
    [TestCase("2022-01-01T00:05:00Z", "2022-01-01T00:20:00Z", TestName = "Data set minus first lowest datapoint")]
    public async Task GetCurrentForecastDataAsync_OptimalDataPoint(DateTimeOffset? start, DateTimeOffset? end)
    {
        var datasource = CreateCurrentForecastDataSource(TestData.GetForecast("2022-01-01T00:00:00Z"));
        var handler = new ForecastHandler(Logger!.Object, datasource.Object);
        var results = await handler.GetCurrentForecastAsync(new string[] { "westus" }, start, end);
        var forecast = results.First();
        var firstDataPoint = forecast.EmissionsDataPoints.First();
        var optimalDataPoint = forecast.OptimalDataPoints.First();

        Assert.That(firstDataPoint, Is.EqualTo(optimalDataPoint));
    }

    [Test]
    public async Task GetCurrentForecastDataAsync_RollingAverageWithSpecifiedWindow()
    {
        // Arrange
        var windowSize = 10;
        var dataTickSize = 5.0;
        var dataDuration = windowSize;
        var dataStartTime = new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var dataEndTime = new DateTimeOffset(2022, 1, 1, 0, 15, 0, TimeSpan.Zero);
        var datasource = CreateCurrentForecastDataSource(TestData.GetForecast("2022-01-01T00:00:00Z"));
        var locationName = "westus";

        var expectedData = new List<Models.EmissionsData>();
        var expectedRatings = new double[] { 15.0, 25.0 };
        for (var i = 0; i < expectedRatings.Length; i++)
        {
            expectedData.Add(new Models.EmissionsData() { Time = dataStartTime + i * TimeSpan.FromMinutes(dataTickSize), Location = locationName, Rating = expectedRatings[i], Duration = TimeSpan.FromMinutes(dataDuration) });
        }

        // Act
        var handler = new ForecastHandler(Logger!.Object, datasource.Object);
        var results = await handler.GetCurrentForecastAsync(new string[] { "westus" }, dataStartTime, dataEndTime, windowSize);
        var forecast = results.First();

        // Assert
        Assert.That(expectedData, Is.EqualTo(forecast.EmissionsDataPoints));
    }

    [Test]
    public async Task GetCurrentForecastDataAsync_RollingAverageWithNoSpecifiedWindow()
    {
        // Arrange
        var dataTickSize = 5.0;
        var dataDuration = 5;
        var dataStartTime = new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var dataEndTime = new DateTimeOffset(2022, 1, 1, 0, 15, 0, TimeSpan.Zero);
        var datasource = CreateCurrentForecastDataSource(TestData.GetForecast("2022-01-01T00:00:00Z"));
        var locationName = "westus";

        var expectedData = new List<Models.EmissionsData>();
        var expectedRatings = new double[] { 10.0, 20.0, 30.0, 40.0 };
        for (var i = 0; i < expectedRatings.Length; i++)
        {
            expectedData.Add(new Models.EmissionsData() { Time = dataStartTime + i * TimeSpan.FromMinutes(dataTickSize), Location = locationName, Rating = expectedRatings[i], Duration = TimeSpan.FromMinutes(dataDuration) });
        }

        // Act
        var handler = new ForecastHandler(Logger!.Object, datasource.Object);
        var results = await handler.GetCurrentForecastAsync(new string[] { "westus" }, dataStartTime, dataEndTime);
        var forecast = results.First();

        // Assert
        Assert.That(expectedData, Is.EqualTo(forecast.EmissionsDataPoints));
    }

    [TestCase("2021-12-31T00:00:00Z", "2022-01-01T00:20:00Z", TestName = "early startTime, valid endTime")]
    [TestCase("2022-01-01T00:00:00Z", "2022-01-01T00:30:00Z", TestName = "valid startTime, late endTime")]
    [TestCase("2021-12-31T00:00:00Z", null, TestName = "early startTime, default endTime")]
    [TestCase(null, "2022-01-01T00:30:00Z", TestName = "default startTime, late endTime")]
    [TestCase("2022-01-01T00:20:00Z", "2022-01-01T00:00:00Z", TestName = "startTime after endTime")]
    public void GetCurrentForecastDataAsync_InvalidStartEndTimes_ThrowsException(DateTimeOffset? start, DateTimeOffset? end)
    {
        var datasource = CreateCurrentForecastDataSource(TestData.GetForecast("2022-01-01T00:00:00Z"));
        var handler = new ForecastHandler(Logger!.Object, datasource.Object);
        Assert.ThrowsAsync<ArgumentException>(async () => await handler.GetCurrentForecastAsync(new string[] { "westus" }, start, end));
    }


    [TestCase("2021-12-31T00:00:00Z", "2022-01-01T00:20:00Z", TestName = "early startTime, valid endTime")]
    [TestCase("2022-01-01T00:00:00Z", "2022-01-01T00:30:00Z", TestName = "valid startTime, late endTime")]
    [TestCase("2021-12-31T00:00:00Z", null, TestName = "early startTime, default endTime")]
    [TestCase(null, "2022-01-01T00:30:00Z", TestName = "default startTime, late endTime")]
    [TestCase("2022-01-01T00:20:00Z", "2022-01-01T00:00:00Z", TestName = "startTime after endTime")]
    public void GetForecastDataAsync_InvalidStartEndTimes_ThrowsException(DateTimeOffset? start, DateTimeOffset? end)
    {
        var requested = new DateTimeOffset(2021, 01, 01, 00, 00, 0, TimeSpan.Zero);
        var datasource = CreateForecastByDateDataSource(TestData.GetForecast("2022-01-01T00:00:00Z"), requested);
        var handler = new ForecastHandler(Logger!.Object, datasource.Object);
        Assert.ThrowsAsync<ArgumentException>(async () => await handler.GetForecastByDateAsync("westus", start, end, requested));
    }

    [TestCase("2022-01-01T00:00:00Z", "2022-01-01T00:20:00Z", TestName = "Full data set")]
    [TestCase("2022-01-01T00:05:00Z", "2022-01-01T00:20:00Z", TestName = "Data set minus first lowest datapoint")]
    public async Task GetForecastDataAsync_OptimalDataPoint(DateTimeOffset? start, DateTimeOffset? end)
    {
        var requested = new DateTimeOffset(2021, 01, 01, 00, 00, 0, TimeSpan.Zero);
        var datasource = CreateForecastByDateDataSource(TestData.GetForecast("2022-01-01T00:00:00Z"), requested);

        var handler = new ForecastHandler(Logger!.Object, datasource.Object);
        var forecast = await handler.GetForecastByDateAsync("westus", start, end, requested);
        var firstDataPoint = forecast.EmissionsDataPoints.First();
        var optimalDataPoint = forecast.OptimalDataPoints.First();

        Assert.That(firstDataPoint, Is.EqualTo(optimalDataPoint));
    }

    private static Mock<IForecastDataSource> CreateCurrentForecastDataSource(global::CarbonAware.Model.EmissionsForecast data)
    {
        var datasource = new Mock<IForecastDataSource>();
        datasource
            .Setup(x => x.GetCurrentCarbonIntensityForecastAsync(It.IsAny<Location>()))
            .ReturnsAsync(data);

        return datasource;
    }

    private static Mock<IForecastDataSource> CreateForecastByDateDataSource(global::CarbonAware.Model.EmissionsForecast data, DateTimeOffset requested)
    {
        var datasource = new Mock<IForecastDataSource>();
        datasource
            .Setup(x => x.GetCarbonIntensityForecastAsync(It.IsAny<Location>(), requested))
            .ReturnsAsync(data);

        return datasource;
    }

    private static Mock<IForecastDataSource> SetupMockDataSourceThatThrows()
    {
        var datasource = new Mock<IForecastDataSource>();
        datasource
            .Setup(x => x.GetCurrentCarbonIntensityForecastAsync(It.IsAny<Location>()))
            .ThrowsAsync(new CarbonAware.Exceptions.CarbonAwareException(""));

        datasource
            .Setup(x => x.GetCarbonIntensityForecastAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>()))
            .ThrowsAsync(new CarbonAware.Exceptions.CarbonAwareException(""));

        return datasource;
    }
}
