
using EmissionsData = CarbonAware.Model.EmissionsData;
using GSF.CarbonAware.Exceptions;
using GSF.CarbonAware.Handlers;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;
using System;
using CarbonAware.Interfaces;
using GSF.CarbonAware.Handlers.CarbonAware;
using System.Collections;
using CarbonAware.Model;
using System.Collections.Generic;

namespace GSF.CarbonAware.Tests;

[TestFixture]
public class EmissionsHandlerTests
{
    private Mock<ILogger<EmissionsHandler>>? Logger { get; set; }

    private readonly EmissionsData[] EmptyTestData = Array.Empty<EmissionsData>();

    [SetUp]
    public void SetUp()
    {
        Logger = new Mock<ILogger<EmissionsHandler>>();
    }

    /// <summary>
    /// GetEmissionsData: Tests empty or null location arrays throw ArgumentException.
    /// </summary>
    [TestCase(new object?[] { null, null }, TestName = "GetEmissions, array of nulls, throws: simulates 'location=&location=' empty value input")]
    [TestCase(new object?[] { null, }, TestName = "GetEmissions, array of nulls, throws: simulates 'location=' empty value input")]
    [TestCase(new object?[] { }, TestName = "GetEmissions, empty array, throws: simulates no 'location' input")]
    public void GetEmissionsData_NoLocations_ThrowsException(params string[] locations)
    {
        // Arrange
        var emissionsHandler = new EmissionsHandler(Logger!.Object, CreateEmissionsDataSource(EmptyTestData).Object);

        //Act/Assert
        Assert.ThrowsAsync<ArgumentException>(async () => await emissionsHandler.GetEmissionsDataAsync(locations));
    }

    /// <summary>
    /// GetEmissionsData: Tests that successful emissions call to datasource with varied location input returns expected data.
    /// </summary>
    [TestCase(new object?[] { null, "Sydney" }, TestName = "GetEmissions, successful: simulates 'location=&location=Sydney'")]
    [TestCase(new object?[] { "Sydney", null }, TestName = "GetEmissions, successful: simulates 'location=Sydney&location='")]
    [TestCase(new object?[] { "Sydney", "Melbourne" }, TestName = "GetEmissions, successful: simulates 'location=Sydney&location=Melbourne'")]
    [TestCase(new object?[] { "Sydney" }, TestName = "GetEmissions, successful: simulates 'location=Sydney'")]
    public async Task GetEmissionsData_VariedLocationsInput_SuccessfulCall(params string[] locations)
    {
        // Arrange
        var data = new EmissionsData[]
        {
            new EmissionsData()
            {
                Location = "Sydney",
                Rating = 0.9,
                Time = DateTime.Now
            }
        };
        var emissionsHandler = new EmissionsHandler(Logger!.Object, CreateEmissionsDataSource(data).Object);

        // Act
        var result = await emissionsHandler.GetEmissionsDataAsync(locations);

        // Assert
        Assert.That(result, Is.Not.Empty);
        Assert.That(result.First().Location, Is.EqualTo("Sydney"));
    }

    [Test]
    public void GetEmissionsDataAsync_LocationMissing()
    {
        //Arrange
        var emissionsHandler = new EmissionsHandler(Logger!.Object, CreateEmissionsDataSource(EmptyTestData).Object);

        //Act and assert
        Assert.ThrowsAsync<ArgumentException>(async () => await emissionsHandler.GetEmissionsDataAsync("", DateTimeOffset.Now, DateTimeOffset.Now + TimeSpan.FromHours(1)));
    }

    [Test]
    public async Task GetEmissionsDataAsync_StartProvidedAndEndMissing()
    {
        //Arrange
        var location = "westus";
        var end = new DateTimeOffset(2021, 11, 17, 0, 0, 0, TimeSpan.Zero);
        var start = new DateTimeOffset(2021, 11, 16, 0, 55, 0, TimeSpan.Zero);
        var expectedTimeValue = new DateTimeOffset(2021, 11, 17, 0, 0, 0, TimeSpan.Zero);

        var data = TestData.GetFilteredEmissionDataList(location, start, end);
        var emissionsHandler = new EmissionsHandler(Logger!.Object, CreateEmissionsDataSource(data).Object);


        //Act
        var results = await emissionsHandler.GetEmissionsDataAsync(location, start, null);

        //Assert   
        Assert.That(results.Count(), Is.EqualTo(1));
        Assert.That(expectedTimeValue, Is.EqualTo(results.First().Time));
    }

    [Test]
    public void GetEmissionsDataAsync_EndProvidedButStartMissing()
    {
        //Arrange
        var emmisionsData = TestData.GetAllEmissionDataList();
        var end = new DateTimeOffset(2021, 11, 17, 0, 0, 0, TimeSpan.Zero);
        var start = new DateTimeOffset(2021, 11, 16, 0, 55, 0, TimeSpan.Zero);
        var data = TestData.GetFilteredEmissionDataList("westus", start, end);

        var emissionsHandler = new EmissionsHandler(Logger!.Object, CreateEmissionsDataSource(data).Object);

        //Act and Assert
        Assert.ThrowsAsync<ArgumentException>(async () => await emissionsHandler.GetEmissionsDataAsync("westus", null, end));
    }

    [Test]
    public async Task TestGetEmissionsDataAsync_FullTimeWindow()
    {
        //Arrange
        var start = new DateTimeOffset(2021, 11, 17, 0, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2021, 11, 19, 0, 0, 0, TimeSpan.Zero);

        var data = TestData.GetFilteredEmissionDataList("westus", start, end);
        var emissionsHandler = new EmissionsHandler(Logger!.Object, CreateEmissionsDataSource(data).Object);

        //Act
        var results = await emissionsHandler.GetEmissionsDataAsync("westus", start, end);

        //Assert   
        Assert.That(results.Count(), Is.EqualTo(4));
        Assert.That(start, Is.EqualTo(results.First().Time));
        Assert.That(end, Is.EqualTo(results.Last().Time));
    }

    [Test]
    public async Task GetBestEmissionsDataAsync_ReturnsExpected()
    {
        // Arrange
        var optimalDataPoint = new EmissionsData() { Rating = 10 };
        var mockData = new List<EmissionsData>() {
            optimalDataPoint,
            new EmissionsData(){ Rating = 20 },
            new EmissionsData(){ Rating = 30 }
        };

        var emissionsHandler = new EmissionsHandler(Logger!.Object, CreateEmissionsDataSource(mockData).Object);

        // Act
        var results = await emissionsHandler.GetBestEmissionsDataAsync("eastus");

        // Assert
        Assert.That(results, Is.EqualTo(new List<Models.EmissionsData>() { optimalDataPoint }));
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
        var datasource = new Mock<IEmissionsDataSource>();
        datasource.Setup(x => x.GetCarbonIntensityAsync(
            It.IsAny<IEnumerable<Location>>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .Callback((IEnumerable<Location> _, DateTimeOffset _start, DateTimeOffset _end) =>
            {
                actualStart = _start;
                actualEnd = _end;
            });

        DateTimeOffset expectedEnd;
        DateTimeOffset expectedStart;

        if (start.HasValue)
        {
            expectedStart = start.Value;
        }
        else
        {
            expectedStart = DateTimeOffset.UtcNow;
        }

        if (end.HasValue)
        {
            expectedEnd = end.Value;
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
        var emissionsHandler = new EmissionsHandler(Logger!.Object, datasource.Object);

        await emissionsHandler.GetBestEmissionsDataAsync("eastus", start, end);

        // Assert
        Assert.That(actualStart.Ticks, Is.InRange(minAllowableStartTicks, maxAllowableStartTicks));
        Assert.That(actualEnd.Ticks, Is.InRange(minAllowableEndTicks, maxAllowableEndTicks));
    }

    [Test]
    public void GetBestEmissionsDataAsync_ThrowsWhenNoLocations()
    {
        // Arrange
        var emissionsHandler = new EmissionsHandler(Logger!.Object, CreateEmissionsDataSource(EmptyTestData).Object);

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(async () => await emissionsHandler.GetBestEmissionsDataAsync(""));
    }

    [Test]
    public void GetBestEmissionsDataAsync_ThrowsWhenEndButNoStart()
    {
        // Arrange
        var emissionsHandler = new EmissionsHandler(Logger!.Object, CreateEmissionsDataSource(EmptyTestData).Object);

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(async () => await emissionsHandler.GetBestEmissionsDataAsync("eastus",null, DateTimeOffset.Now));
    }

    [TestCase("eastus", "2021-11-18T00:00:00Z", "2021-11-18T08:00:00Z", ExpectedResult = 60)]
    [TestCase("westus", "2021-11-17T00:00:00Z", "2021-11-18T00:00:00Z", ExpectedResult = 20)]
    [TestCase("eastus", "2021-11-19T00:00:00Z", "2021-12-30T00:00:00Z", ExpectedResult = 0)]
    [TestCase("fakelocation", "2021-11-18T00:00:00Z", "2021-12-30T00:00:00Z", ExpectedResult = 0)]
    public async Task<double> CalculateAverageCarbonIntensityAsync_ValidTimeInterval(string regionName, string startString, string endString)
    {
        // Arrange
       
        var start = DateTimeOffset.Parse(startString);
        var end = DateTimeOffset.Parse(endString);
        var data = TestData.GetFilteredEmissionDataList(regionName, start, end);
        var dataSource = CreateEmissionsDataSource(data);
        var emissionsHandler = new EmissionsHandler(Logger!.Object, dataSource.Object);

        // Act
        var result = await emissionsHandler.GetAverageCarbonIntensityAsync(regionName, start, end);

        return result;
    }

    /// <summary>
    /// Tests that when an error is thrown, it is caught and wrapped in the custom exception.
    /// </summary>
    [Test]
    public void GetAverageCarbonIntensity_ErrorThrowsCustomException()
    {
        // Arrange
        var datasource = new Mock<IEmissionsDataSource>();
        datasource.Setup(x => x.GetCarbonIntensityAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>())).ThrowsAsync(new CarbonAwareException(""));
        
        var emissionsHandler = new EmissionsHandler(Logger!.Object, datasource.Object);

        //Act Assert
        Assert.ThrowsAsync<CarbonAwareException>(async () => await emissionsHandler.GetAverageCarbonIntensityAsync("location", DateTimeOffset.Now, DateTimeOffset.Now));
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
        var datasource = new Mock<IEmissionsDataSource>();
        datasource.Setup(x => x.GetCarbonIntensityAsync(
            It.IsAny<IEnumerable<Location>>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .Callback((IEnumerable<Location> _, DateTimeOffset _start, DateTimeOffset _end) =>
            {
                actualStart = _start;
                actualEnd = _end;
            });

        var emissionsHandler = new EmissionsHandler(Logger!.Object, datasource.Object);

        DateTimeOffset expectedEnd;
        DateTimeOffset expectedStart;

        if (start.HasValue)
        {
            expectedStart = start.Value;
        }
        else
        {
            expectedStart = DateTimeOffset.UtcNow;
        }

        if (end.HasValue)
        {
            expectedEnd = end.Value;
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
        await emissionsHandler.GetEmissionsDataAsync("eastus", start, end);

        // Assert
        Assert.That(actualStart.Ticks, Is.InRange(minAllowableStartTicks, maxAllowableStartTicks));
        Assert.That(actualEnd.Ticks, Is.InRange(minAllowableEndTicks, maxAllowableEndTicks));
    }
    
    private static Mock<IEmissionsDataSource> CreateEmissionsDataSource(IEnumerable<EmissionsData> data)
    {
        var datasource = new Mock<IEmissionsDataSource>();
        datasource
            .Setup(x => x.GetCarbonIntensityAsync(It.IsAny<IEnumerable<Location>>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .ReturnsAsync(data);
        datasource
           .Setup(x => x.GetCarbonIntensityAsync(It.IsAny<Location>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
           .ReturnsAsync(data);
        return datasource;
    }
}