using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.Interfaces;
using CarbonAware.Model;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
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
        Assert.AreEqual(expectedStart, forecastResult.DataStartAt);
        Assert.AreEqual(expectedEnd, forecastResult.DataEndAt);
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
        var dataStartTime = new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero);
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
        var expectedRatings = new double[] { 15.0, 25.0 };
        for (var i = 0; i < expectedRatings.Count(); i++)
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
        var dataStartTime = new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero);
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
        for (var i = 0; i < expectedRatings.Count(); i++)
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

    [Test]
    public void TestGetForecastDataAsync_NoLocation()
    {
        var props = new Dictionary<string, object?>()
        {
            { CarbonAwareConstants.ForecastRequestedAt, new DateTimeOffset(2021,9,1,8,30,0, TimeSpan.Zero) }
        };

        Assert.ThrowsAsync<ArgumentException>(async () => await this.Aggregator.GetForecastDataAsync(props));
    }

    [Test]
    public void TestGetForecastDataAsync_MultipleLocations()
    {
        var props = new Dictionary<string, object?>()
        {
            { CarbonAwareConstants.Locations, new List<Location>() { new Location() { RegionName = "westus" }, new Location() { RegionName = "eastus" } } },
            { CarbonAwareConstants.ForecastRequestedAt, new DateTimeOffset(2021,9,1,8,30,0, TimeSpan.Zero) }
        };

        Assert.ThrowsAsync<ArgumentException>(async () => await this.Aggregator.GetForecastDataAsync(props));
    }

    [Test]
    public void TestGetForecastDataAsync_NoRequestedAt()
    {
        var props = new Dictionary<string, object?>()
        {
            { CarbonAwareConstants.Locations, new List<Location>() { new Location() { RegionName = "eastus" } } }
        };

        Assert.ThrowsAsync<ArgumentException>(async () => await this.Aggregator.GetForecastDataAsync(props));
    }

    [TestCase("2021-12-31T00:00:00Z", "2022-01-01T00:20:00Z", TestName = "early startTime, valid endTime")]
    [TestCase("2022-01-01T00:00:00Z", "2022-01-01T00:30:00Z", TestName = "valid startTime, late endTime")]
    [TestCase("2021-12-31T00:00:00Z", null, TestName = "early startTime, default endTime")]
    [TestCase(null, "2022-01-01T00:30:00Z", TestName = "default startTime, late endTime")]
    [TestCase("2022-01-01T00:20:00Z", "2022-01-01T00:00:00Z", TestName = "startTime after endTime")]
    public void TestGetForecastDataAsync_InvalidStartEndTimes_ThrowsException(string start, string end)
    {
        var requestedAt = new DateTimeOffset(2021, 01, 01, 00, 00, 0, TimeSpan.Zero);

        this.CarbonIntensityDataSource.Setup(x => x.GetCarbonIntensityForecastAsync(It.IsAny<Location>(), requestedAt))
            .ReturnsAsync(TestData.GetForecast("2022-01-01T00:00:00Z"));
        var props = new Dictionary<string, object?>()
        {
            { CarbonAwareConstants.Locations, new List<Location>() { new Location() { RegionName = "westus" } } },
            { CarbonAwareConstants.Start, start },
            { CarbonAwareConstants.End, end },
            { CarbonAwareConstants.ForecastRequestedAt, requestedAt }

        };
        Assert.ThrowsAsync<ArgumentException>(async () => await Aggregator.GetForecastDataAsync(props));
    }

    [TestCase("2022-01-01T00:00:00Z", "2022-01-01T00:20:00Z", TestName = "Full data set")]
    [TestCase("2022-01-01T00:05:00Z", "2022-01-01T00:20:00Z", TestName = "Data set minus first lowest datapoint")]
    public async Task TestGetForecastDataAsync_OptimalDataPoint(string start, string end)
    {
        var requestedAt = new DateTimeOffset(2021, 01, 01, 00, 00, 0, TimeSpan.Zero);
        this.CarbonIntensityDataSource.Setup(x => x.GetCarbonIntensityForecastAsync(It.IsAny<Location>(), requestedAt))
            .ReturnsAsync(TestData.GetForecast("2022-01-01T00:00:00Z"));
        var props = new Dictionary<string, object>()
        {
            { CarbonAwareConstants.Locations, new List<Location>() { new Location() { RegionName = "westus" } } },
            { CarbonAwareConstants.Start, start },
            { CarbonAwareConstants.End, end },
            { CarbonAwareConstants.ForecastRequestedAt, requestedAt }

        };

        var forecast = await this.Aggregator.GetForecastDataAsync(props);
        var firstDataPoint = forecast.ForecastData.First();
        var optimalDataPoint = forecast.OptimalDataPoint;

        Assert.AreEqual(firstDataPoint, optimalDataPoint);
    }

    [Test]
    public async Task TestGetForecastDataAsync_Metadata()
    {
        var requestedAt = new DateTimeOffset(2021, 01, 01, 00, 00, 0, TimeSpan.Zero);
        this.CarbonIntensityDataSource.Setup(x => x.GetCarbonIntensityForecastAsync(It.IsAny<Location>(), requestedAt))
            .ReturnsAsync(TestData.GetForecast("2022-01-01T00:00:00Z"));
        const string reg = "westus";
        var props = new Dictionary<string, object>()
        {
            { CarbonAwareConstants.Locations, new List<Location>() { new Location() { RegionName = reg } } },
            { CarbonAwareConstants.Start, DateTimeOffset.Parse("2022-01-01T00:00:00Z") },
            { CarbonAwareConstants.End,  DateTimeOffset.Parse("2022-01-01T00:20:00Z") },
            { CarbonAwareConstants.ForecastRequestedAt, requestedAt }
        };

        var forecast = await this.Aggregator.GetForecastDataAsync(props);
        Assert.AreEqual(forecast.Location.RegionName, reg);
        Assert.AreEqual(forecast.DataStartAt, props[CarbonAwareConstants.Start]);
        Assert.AreEqual(forecast.DataEndAt, props[CarbonAwareConstants.End]);
    }

    [TestCase("eastus", "2021-11-18T00:00:00Z", "2021-11-18T08:00:00Z", ExpectedResult = 60)]
    [TestCase("westus", "2021-11-17T00:00:00Z", "2021-11-18T00:00:00Z", ExpectedResult = 20)]
    [TestCase("eastus", "2021-11-19T00:00:00Z", "2021-12-30T00:00:00Z", ExpectedResult = 0)]
    [TestCase("fakelocation", "2021-11-18T00:00:00Z", "2021-12-30T00:00:00Z", ExpectedResult = 0)]
    public async Task<double> CalculateAverageCarbonIntensityAsync_ValidTimeInterval(string regionName, string startString, string endString)
    {
        // Arrange
        var location = new Location()
        {
            LocationType = LocationType.CloudProvider,
            CloudProvider = CloudProvider.Azure,
            RegionName = regionName
        };

        List<Location> locations = new List<Location>() {
          location
        };

        DateTimeOffset start, end;
        DateTimeOffset.TryParse(startString, out start);
        DateTimeOffset.TryParse(endString, out end);

        var props = new Dictionary<string, object?>()
        {
            { CarbonAwareConstants.Locations, locations },
            { CarbonAwareConstants.Start, start },
            { CarbonAwareConstants.End, end }
        };

        this.CarbonIntensityDataSource.Setup(x => x.GetCarbonIntensityAsync(It.IsAny<IEnumerable<Location>>(),
            It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .ReturnsAsync(TestData.GetFilteredEmissionDataList(location.RegionName, startString, endString));

        // Act
        var result = await this.Aggregator.CalculateAverageCarbonIntensityAsync(props);

        // Assert
        this.CarbonIntensityDataSource.Verify(r => r.GetCarbonIntensityAsync(locations, start, end), Times.Once);
        return result;
    }

    [TestCase("2021-11-18T00:00:00Z", null, TestName = "AverageCI valid startTime, null endTime")]
    [TestCase(null, "2021-11-18T00:00:00Z", TestName = "AverageCI null startTime, valid endTime")]
    [TestCase("2021-11-19T00:00:00Z", "2021-11-18T00:00:00Z", TestName = "AverageCI startTime greater than endTime")]
    [TestCase("2021-11-18T00:00:00Z", "2021-11-18T00:00:00Z", TestName = "AverageCI startTime equal endTime")]
    public void TestCalculateAverageCarbonIntensityAsync_InvalidStartEndTimes_ThrowsException(string start, string end)
    {
        this.CarbonIntensityDataSource.Setup(x => x.GetCarbonIntensityAsync(It.IsAny<IEnumerable<Location>>(),
            It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .ReturnsAsync(TestData.GetAllEmissionDataList());

        var props = new Dictionary<string, object?>()
        {
            { CarbonAwareConstants.Locations, new List<Location>() { new Location() { RegionName = "westus" } } },
            { CarbonAwareConstants.Start, start },
            { CarbonAwareConstants.End, end }
        };

        Assert.ThrowsAsync<ArgumentException>(async () => await Aggregator.CalculateAverageCarbonIntensityAsync(props));
    }

    [Test]
    public async Task CalculateAverageCarbonIntensityAsync_UnderspecifiedTimeInterval()
    {
        // Arrange
        var location = new Location()
        {
            LocationType = LocationType.Geoposition,
            Latitude = (decimal)1.0,
            Longitude = (decimal)2.0
        };

        List<Location> locations = new List<Location>() {
          location
        };

        var start = DateTimeOffset.Parse("2019-01-01", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
        var end = DateTimeOffset.Parse("2019-01-02", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);

        var props = new Dictionary<string, object?>()
        {
            { CarbonAwareConstants.Locations, locations },
            { CarbonAwareConstants.Start, start },
            { CarbonAwareConstants.End, end }
        };

        // Act
        await this.Aggregator.CalculateAverageCarbonIntensityAsync(props);

        // Assert
        this.CarbonIntensityDataSource.Verify(r => r.GetCarbonIntensityAsync(locations, start, end), Times.Once);
    }
}
