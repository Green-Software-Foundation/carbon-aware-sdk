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

    [Test]
    public void TestGetEmissionsDataAsync_LocationMissing()
    {
        //Arrange
        this.CarbonIntensityDataSource
            .Setup(x => x.GetCarbonIntensityAsync(It.IsAny<IEnumerable<Location>>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .ReturnsAsync(TestData.GetAllEmissionDataList());

        //Act and assert
        Assert.ThrowsAsync<ArgumentException>(async () => await this.Aggregator.GetEmissionsDataAsync(new CarbonAwareParametersBaseDTO()));
    }

    [Test]
    public async Task TestGetEmissionsDataAsync_StartProvidedAndEndMissing()
    {
        //Arrange
        var emmisionsData = TestData.GetAllEmissionDataList();
        var end = new DateTimeOffset(2021,11,17,0,0,0,TimeSpan.Zero);
        var start = new DateTimeOffset(2021,11,16,0,55,0,TimeSpan.Zero);
        var expectedTimeValue = new DateTimeOffset(2021,11,17,0,0,0,TimeSpan.Zero);

        var parameters = new CarbonAwareParametersBaseDTO()
        {
            MultipleLocations = new string[] { "westus" },
            Start = start,
        };

        this.CarbonIntensityDataSource
            .Setup(x => x.GetCarbonIntensityAsync(It.IsAny<IEnumerable<Location>>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .ReturnsAsync(TestData.GetFilteredEmissionDataList("westus", start, end));

        //Act
        var results = (await this.Aggregator.GetEmissionsDataAsync(parameters)).ToList();

        //Assert   
        Assert.AreEqual(results.Count(), 1);
        Assert.AreEqual(results.First().Time, expectedTimeValue);
    }
   

    [Test]
    public void TestGetEmissionsDataAsync_EndProvidedButStartMissing()
    {
        //Arrange
        var emmisionsData = TestData.GetAllEmissionDataList();
        var end = new DateTimeOffset(2021,11,17,0,0,0,TimeSpan.Zero);
        var start = new DateTimeOffset(2021,11,16,0,55,0,TimeSpan.Zero);

        var parameters = new CarbonAwareParametersBaseDTO()
        {
            MultipleLocations = new string[] { "westus" },
            End = end
        };

        this.CarbonIntensityDataSource
            .Setup(x => x.GetCarbonIntensityAsync(It.IsAny<IEnumerable<Location>>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .ReturnsAsync(TestData.GetFilteredEmissionDataList("westus", start, end));

        //Act and Assert
        Assert.ThrowsAsync<ArgumentException>(async () => await this.Aggregator.GetEmissionsDataAsync(parameters));
    }

    [Test]
    public async Task TestGetEmissionsDataAsync_FullTimeWindow()
    {
        //Arrange
        var emmisionsData = TestData.GetAllEmissionDataList();
        var start = new DateTimeOffset(2021,11,17,0,0,0,TimeSpan.Zero);
        var end = new DateTimeOffset(2021,11,19,0,0,0,TimeSpan.Zero);

        var parameters = new CarbonAwareParametersBaseDTO()
        {
            MultipleLocations = new string[] { "westus" },
            Start = start,
            End = end
        };

        this.CarbonIntensityDataSource
            .Setup(x => x.GetCarbonIntensityAsync(It.IsAny<IEnumerable<Location>>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .ReturnsAsync(TestData.GetFilteredEmissionDataList("westus", start, end));

        //Act
        var results = (await this.Aggregator.GetEmissionsDataAsync(parameters)).ToList();

        //Assert   
        Assert.AreEqual(results.Count(), 4);
        Assert.AreEqual(results.First().Time, start);
        Assert.AreEqual(results.Last().Time, end);
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

        this.CarbonIntensityDataSource.Setup(x => x.GetCurrentCarbonIntensityForecastAsync(It.IsAny<Location>()))
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
        this.CarbonIntensityDataSource.Setup(x => x.GetCurrentCarbonIntensityForecastAsync(It.IsAny<Location>()))
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
        this.CarbonIntensityDataSource.Setup(x => x.GetCurrentCarbonIntensityForecastAsync(It.IsAny<Location>()))
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
        this.CarbonIntensityDataSource.Setup(x => x.GetCurrentCarbonIntensityForecastAsync(It.IsAny<Location>()))
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
        this.CarbonIntensityDataSource.Setup(x => x.GetCurrentCarbonIntensityForecastAsync(It.IsAny<Location>()))
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
        this.CarbonIntensityDataSource.Setup(x => x.GetCurrentCarbonIntensityForecastAsync(It.IsAny<Location>()))
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
        this.CarbonIntensityDataSource.Setup(x => x.GetCarbonIntensityForecastAsync(It.IsAny<Location>(), requested))
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
        this.CarbonIntensityDataSource.Setup(x => x.GetCarbonIntensityForecastAsync(It.IsAny<Location>(), requested))
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
        this.CarbonIntensityDataSource.Setup(x => x.GetCarbonIntensityForecastAsync(It.IsAny<Location>(), requested))
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

    [TestCase("eastus", "2021-11-18T00:00:00Z", "2021-11-18T08:00:00Z", ExpectedResult = 60)]
    [TestCase("westus", "2021-11-17T00:00:00Z", "2021-11-18T00:00:00Z", ExpectedResult = 20)]
    [TestCase("eastus", "2021-11-19T00:00:00Z", "2021-12-30T00:00:00Z", ExpectedResult = 0)]
    [TestCase("fakelocation", "2021-11-18T00:00:00Z", "2021-12-30T00:00:00Z", ExpectedResult = 0)]
    public async Task<double> CalculateAverageCarbonIntensityAsync_ValidTimeInterval(string regionName, string startString, string endString)
    {
        // Arrange
        var location = new Location()
        {
            Name = regionName
        };

        var start = DateTimeOffset.Parse(startString);
        var end = DateTimeOffset.Parse(endString);

        var parameters = new CarbonAwareParameters() { 
            SingleLocation = location, 
            Start = start, 
            End = end
        };

        this.CarbonIntensityDataSource.Setup(x => x.GetCarbonIntensityAsync(It.IsAny<Location>(),
            It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .ReturnsAsync(TestData.GetFilteredEmissionDataList(location.Name, start, end));

        // Act
        var result = await this.Aggregator.CalculateAverageCarbonIntensityAsync(parameters);

        // Assert
        this.CarbonIntensityDataSource.Verify(r => r.GetCarbonIntensityAsync(location, start, end), Times.Once);
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

        var parameters = new CarbonAwareParameters() { 
            SingleLocation = new Location() { Name = "westus" }, 
        };
        if (start!=null) parameters.Start = DateTimeOffset.Parse(start);
        if (end != null) parameters.End = DateTimeOffset.Parse(end);

        Assert.ThrowsAsync<ArgumentException>(async () => await Aggregator.CalculateAverageCarbonIntensityAsync(parameters));
    }

    [Test]
    public async Task CalculateAverageCarbonIntensityAsync_UnderspecifiedTimeInterval()
    {
        // Arrange
        var location = new Location()
        {
            Latitude = (decimal)1.0,
            Longitude = (decimal)2.0
        };

        var start = DateTimeOffset.Parse("2019-01-01", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
        var end = DateTimeOffset.Parse("2019-01-02", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);

        var parameters = new CarbonAwareParameters()
        {
            SingleLocation = location,
            Start = start,
            End = end
        };

        // Act
        await this.Aggregator.CalculateAverageCarbonIntensityAsync(parameters);

        // Assert
        this.CarbonIntensityDataSource.Verify(r => r.GetCarbonIntensityAsync(location, start, end), Times.Once);
    }

    [Test]
    public async Task GetBestEmissionsDataAsync_ReturnsExpected()
    {
        // Arrange
        var parameters = new CarbonAwareParameters(){ MultipleLocations = new List<Location>() { new Location() } };
        var optimalDataPoint = new EmissionsData(){ Rating = 10 };
        var mockData = new List<EmissionsData>() { 
            optimalDataPoint,
            new EmissionsData(){ Rating = 20 },
            new EmissionsData(){ Rating = 30 }
        };

        this.CarbonIntensityDataSource.Setup(x => x.GetCarbonIntensityAsync(
            It.IsAny<IEnumerable<Location>>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .ReturnsAsync(mockData);

        // Act
        var results = await this.Aggregator.GetBestEmissionsDataAsync(parameters);

        // Assert
        Assert.AreEqual(new List<EmissionsData>() { optimalDataPoint }, results);
    }

    [TestCase("2000-01-01T00:00:00Z", "2000-01-02T00:00:00Z", 0, TestName = "GetBestEmissionsDataAsync calls data source with expected dates: start & end")]
    [TestCase("2000-01-01T00:00:00Z", null, 1000, TestName = "GetBestEmissionsDataAsync calls data source with expected dates: only start")]
    [TestCase(null, null, 1000, TestName = "GetBestEmissionsDataAsync calls data source with expected dates: no start or end")]
    public async Task GetBestEmissionsDataAsync_CallsWithExpectedDates(DateTimeOffset? start, DateTimeOffset? end, long tickTolerance)
    {
        // Arrange
        // Default to a random time that will always be wrong for these test cases.
        DateTimeOffset actualStart = DateTimeOffset.Parse("1984-06-06Z");
        DateTimeOffset actualEnd = DateTimeOffset.Parse("1984-06-06Z");
        this.CarbonIntensityDataSource.Setup(x => x.GetCarbonIntensityAsync(
            It.IsAny<IEnumerable<Location>>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .Callback((IEnumerable<Location> _, DateTimeOffset _start, DateTimeOffset _end) =>
            {
                actualStart = _start;
                actualEnd = _end;
            });

        var parameters = new CarbonAwareParameters() { MultipleLocations = new List<Location>() { new Location() } };
        DateTimeOffset expectedEnd;
        DateTimeOffset expectedStart;

        if (start.HasValue)
        {
            expectedStart = parameters.Start = start.Value;
        }
        else
        {
            expectedStart = DateTimeOffset.UtcNow;
        }

        if (end.HasValue)
        {
            expectedEnd = parameters.End = end.Value;
        }
        else
        {
            expectedEnd = expectedStart;
        }

        // Because this method uses DateTimeOffset.UtcNow as a default, we cannot precisely check our date expectations
        // so instead, we test that the dates are within a tolerable range using DateTimeOffset Ticks.
        // 1 Second == 10,000,000 Ticks
        // 1000 Ticks == 0.1 Milliseconds
        var minAllowableStartTicks = expectedStart.Ticks - tickTolerance;
        var maxAllowableStartTicks = expectedStart.Ticks + tickTolerance;
        var minAllowableEndTicks = expectedEnd.Ticks - tickTolerance;
        var maxAllowableEndTicks = expectedEnd.Ticks + tickTolerance;

        // Act
        await this.Aggregator.GetBestEmissionsDataAsync(parameters);

        // Assert
        Assert.That(actualStart.Ticks, Is.InRange(minAllowableStartTicks, maxAllowableStartTicks));
        Assert.That(actualEnd.Ticks, Is.InRange(minAllowableEndTicks, maxAllowableEndTicks));
    }

    [Test]
    public void GetBestEmissionsDataAsync_ThrowsWhenNoLocations()
    {
        // Arrange
        var emptyListAssigned = new CarbonAwareParameters() { MultipleLocations = new List<Location>() };
        var noLocationsAssigned = new CarbonAwareParameters();

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(async () => await this.Aggregator.GetBestEmissionsDataAsync(emptyListAssigned));
        Assert.ThrowsAsync<ArgumentException>(async () => await this.Aggregator.GetBestEmissionsDataAsync(noLocationsAssigned));
    }

    public void GetBestEmissionsDataAsync_ThrowsWhenEndButNoStart()
    {
        // Arrange
        var endNoStart = new CarbonAwareParameters() { MultipleLocations = new List<Location>() { new Location() }, End = new DateTimeOffset() };

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(async () => await this.Aggregator.GetBestEmissionsDataAsync(endNoStart));
    }

    [Test]
    public async Task GetEmissionsDataAsync_ReturnsExpected()
    {
        // Arrange
        var parameters = new CarbonAwareParameters() { MultipleLocations = new List<Location>() { new Location() } };
        var optimalDataPoint = new EmissionsData() { Rating = 10 };
        var mockData = new List<EmissionsData>() {
            optimalDataPoint,
            new EmissionsData(){ Rating = 20 },
            new EmissionsData(){ Rating = 30 }
        };

        this.CarbonIntensityDataSource.Setup(x => x.GetCarbonIntensityAsync(
            It.IsAny<IEnumerable<Location>>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .ReturnsAsync(mockData);

        // Act
        var results = await this.Aggregator.GetEmissionsDataAsync(parameters);

        // Assert
        CollectionAssert.AreEqual(mockData, results);
    }

    [TestCase("2000-01-01T00:00:00Z", "2000-01-02T00:00:00Z", 0, TestName = "GetEmissionsDataAsync calls data source with expected dates: start & end")]
    [TestCase("2000-01-01T00:00:00Z", null, 1000, TestName = "GetEmissionsDataAsync calls data source with expected dates: only start")]
    [TestCase(null, null, 1000, TestName = "GetEmissionsDataAsync calls data source with expected dates: no start or end")]
    public async Task GetEmissionsDataAsync_CallsWithExpectedDates(DateTimeOffset? start, DateTimeOffset? end, long tickTolerance)
    {
        // Arrange
        // Default to a random time that will always be wrong for these test cases.
        DateTimeOffset actualStart = DateTimeOffset.Parse("1984-06-06Z");
        DateTimeOffset actualEnd = DateTimeOffset.Parse("1984-06-06Z");
        this.CarbonIntensityDataSource.Setup(x => x.GetCarbonIntensityAsync(
            It.IsAny<IEnumerable<Location>>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .Callback((IEnumerable<Location> _, DateTimeOffset _start, DateTimeOffset _end) =>
            {
                actualStart = _start;
                actualEnd = _end;
            });

        var parameters = new CarbonAwareParameters() { MultipleLocations = new List<Location>() { new Location() } };
        DateTimeOffset expectedEnd;
        DateTimeOffset expectedStart;

        if (start.HasValue)
        {
            expectedStart = parameters.Start = start.Value;
        }
        else
        {
            expectedStart = DateTimeOffset.UtcNow;
        }

        if (end.HasValue)
        {
            expectedEnd = parameters.End = end.Value;
        }
        else
        {
            expectedEnd = expectedStart;
        }

        // Because this method uses DateTimeOffset.UtcNow as a default, we cannot precisely check our date expectations
        // so instead, we test that the dates are within a tolerable range using DateTimeOffset Ticks.
        // 1 Second == 10,000,000 Ticks
        // 1000 Ticks == 0.1 Milliseconds
        var minAllowableStartTicks = expectedStart.Ticks - tickTolerance;
        var maxAllowableStartTicks = expectedStart.Ticks + tickTolerance;
        var minAllowableEndTicks = expectedEnd.Ticks - tickTolerance;
        var maxAllowableEndTicks = expectedEnd.Ticks + tickTolerance;

        // Act
        await this.Aggregator.GetEmissionsDataAsync(parameters);

        // Assert
        Assert.That(actualStart.Ticks, Is.InRange(minAllowableStartTicks, maxAllowableStartTicks));
        Assert.That(actualEnd.Ticks, Is.InRange(minAllowableEndTicks, maxAllowableEndTicks));
    }

    [Test]
    public void GetEmissionsDataAsync_ThrowsWhenNoLocations()
    {
        // Arrange
        var emptyListAssigned = new CarbonAwareParameters() { MultipleLocations = new List<Location>() };
        var noLocationsAssigned = new CarbonAwareParameters();

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(async () => await this.Aggregator.GetEmissionsDataAsync(emptyListAssigned));
        Assert.ThrowsAsync<ArgumentException>(async () => await this.Aggregator.GetEmissionsDataAsync(noLocationsAssigned));
    }

    public void GetEmissionsDataAsync_ThrowsWhenEndButNoStart()
    {
        // Arrange
        var endNoStart = new CarbonAwareParameters() { MultipleLocations = new List<Location>() { new Location() }, End = new DateTimeOffset() };

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(async () => await this.Aggregator.GetEmissionsDataAsync(endNoStart));
    }
}
