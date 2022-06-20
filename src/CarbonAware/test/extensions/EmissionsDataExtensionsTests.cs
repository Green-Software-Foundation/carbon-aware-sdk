
using CarbonAware.Model;
using CarbonAware.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;


namespace CarbonAware.Extensions.Tests;

public class EmissionsDataExtensionsTests
{
    // Test class sets these fields in [SetUp] rather than traditional class constructor.
    #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private IEnumerable<EmissionsData> data;
    private TimeSpan dataDuration;
    private DateTimeOffset dataStartTime;
    private string dataLocation;
    #pragma warning restore CS8618

    [OneTimeSetUp]
    public void Setup()
    {
        this.dataDuration =  TimeSpan.FromMinutes(5);
        this.dataStartTime = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);
        this.dataLocation = "A";
        this.data = new List<EmissionsData>()
        {
            new EmissionsData() { Time = dataStartTime, Location = "A", Rating = 10.0, Duration = dataDuration },
            new EmissionsData() { Time = dataStartTime + dataDuration, Location = "A", Rating = 20.0, Duration = dataDuration },
            new EmissionsData() { Time = dataStartTime + dataDuration * 2, Location = "A", Rating = 30.0, Duration = dataDuration },
            new EmissionsData() { Time = dataStartTime + dataDuration * 3, Location = "A", Rating = 40.0, Duration = dataDuration },
        }; 
    }


    [TestCase(5, 5, new[] { 10.0, 20.0, 30.0, 40.0 }, TestName="window == data granularity, tick == data granularity")]
    [TestCase(10, 5, new[] { 15.0, 25.0, 35.0 }, TestName="window > data granularity, tick == data granularity")]
    [TestCase(2, 5, new[] { 10.0, 20.0, 30.0, 40.0 }, TestName="window < data granularity, tick == data granularity")]
    [TestCase(5, 4, new[] { 10.0, 18.0, 26.0, 34.0 }, TestName="window == data granularity, tick < data granularity")]
    [TestCase(10, 4, new[] { 15.0, 23.0, 31.0 }, TestName="window > data granularity, tick < data granularity")]
    [TestCase(2, 4, new[] { 10.0, 15.0, 20.0, 30.0, 40.0 }, TestName="window < data granularity, tick < data granularity")]
    [TestCase(5, 7, new[] { 10.0, 24.0, 38.0 }, TestName="window == data granularity, tick > data granularity")]
    [TestCase(10, 7, new[] { 15.0, 29.0 }, TestName="window > data granularity, tick > data granularity")]
    [TestCase(2, 7, new[] { 10.0, 20.0, 35.0 }, TestName="window < data granularity, tick > data granularity")]
    public void RollingAverage_ReturnsExpectedAverages(int windowSize, int tickSize, double[] expectedRatings)
    {
        // Arrange
        var expectedData = new List<EmissionsData>();
        for(var i = 0; i < expectedRatings.Count(); i++)
        {
            expectedData.Add(new EmissionsData() { Time = this.dataStartTime + i * TimeSpan.FromMinutes(tickSize), Location = this.dataLocation, Rating = expectedRatings[i], Duration = TimeSpan.FromMinutes(windowSize) });
        }

        // Act
        var result = this.data.RollingAverage(TimeSpan.FromMinutes(windowSize), TimeSpan.FromMinutes(tickSize));

        // Assert
        Assert.AreEqual(expectedData, result);
    }

    [TestCase(0, 5, 5, 5, new[] { 10.0, 20.0, 30.0, 40.0 }, TestName = "window == 0, tick == data granularity, returns original")]
    [TestCase(0, 4, 5, 5, new[] { 10.0, 20.0, 30.0, 40.0 }, TestName = "window == 0, tick < data granularity, returns original")]
    [TestCase(0, 7, 5, 5, new[] { 10.0, 20.0, 30.0, 40.0 }, TestName = "window == 0, tick > data granularity, returns original")]
    [TestCase(5, 0, 5, 5, new[] { 10.0, 20.0, 30.0, 40.0 }, TestName = "window == data granularity, tick == 0, uses data granularity")]
    [TestCase(10, 0, 10, 5, new[] { 15.0, 25.0, 35.0 }, TestName = "window > data granularity, tick == 0, uses data granularity")]
    [TestCase(2, 0, 2, 5, new[] { 10.0, 20.0, 30.0, 40.0 }, TestName = "window < data granularity, tick == 0, uses data granularity")]
    public void RollingAverage_ReturnsExpectedZeroCases(int windowSize, int tickSize, int expectedWindowSize, int expectedTickSize, double[] expectedRatings)
    {
        // Arrange
        var expectedData = new List<EmissionsData>();
        for (var i = 0; i < expectedRatings.Count(); i++)
        {
            expectedData.Add(new EmissionsData() { Time = this.dataStartTime + i * TimeSpan.FromMinutes(expectedTickSize), Location = this.dataLocation, Rating = expectedRatings[i], Duration = TimeSpan.FromMinutes(expectedWindowSize) });
        }

        // Act
        var result = this.data.RollingAverage(TimeSpan.FromMinutes(windowSize), TimeSpan.FromMinutes(tickSize));

        // Assert
        Assert.AreEqual(expectedData, result);
    }

    [Test]
    public void RollingAverage_EmptyListReturnsEmpty()
    {
        // Arrange
        var emptyList = new List<EmissionsData>();

        // Act
        var result = emptyList.RollingAverage(TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));

        // Assert
        Assert.AreEqual(Enumerable.Empty<EmissionsData>(), result);
    }

    [Test]
    public void RollingAverage_ThrowsForZeroDurationData()
    {
        // Arrange
        var zeroDurationData = new List<EmissionsData>()
        {
            new EmissionsData() { Duration = TimeSpan.Zero }
        };

        // Act
        var result = zeroDurationData.RollingAverage(TimeSpan.FromMinutes(5));

        // Assert
        Assert.Throws<InvalidOperationException>(() => result.ToList());
    }

    [TestCase("2020-01-01T00:00:00Z", "2020-01-01T00:20:00Z", ExpectedResult=25.00, TestName = "Full dataset")]
    [TestCase("2020-01-01T00:04:00Z", "2020-01-01T00:20:00Z", ExpectedResult=28.75, TestName = "Partial first data point")]
    [TestCase("2020-01-01T00:00:00Z", "2020-01-01T00:16:00Z", ExpectedResult=21.25, TestName = "Partial last data point")]
    [TestCase("2020-01-01T00:02:30Z", "2020-01-01T00:17:30Z", ExpectedResult=25.00, TestName = "Partial first and last data point")]
    [TestCase("2020-01-01T00:02:00Z", "2020-01-01T00:03:00Z", ExpectedResult=10.00, TestName = "Partial single data point")]
    public decimal AverageOverPeriod_ReturnsExpectedAverages(string start, string end)
    {
        // Arrange
        var startPeriod = DateTimeOffset.Parse(start);
        var endPeriod = DateTimeOffset.Parse(end);

        // Act
        var result = this.data.AverageOverPeriod(startPeriod, endPeriod);

        // Assert
        // Use decimal precision for assertion
        return Math.Round((decimal)result, 2);
    }

    [Test]
    public void AverageOverPeriod_EmptyListReturnsZero()
    {
        // Arrange
        var emptyList = new List<EmissionsData>();
        var startPeriod = new DateTimeOffset(2022, 1, 1, 0, 5, 0, TimeSpan.Zero);
        var endPeriod = new DateTimeOffset(2022, 1, 1, 0, 15, 0, TimeSpan.Zero);

        // Act
        var result = emptyList.AverageOverPeriod(startPeriod, endPeriod);

        // Assert
        Assert.AreEqual(0.0, result);
    }

    [TestCase("2022-01-01T00:00:00Z", "2022-01-01T00:10:00Z", TestName = "Discontinuous data points")]
    [TestCase("2022-01-01T00:10:00Z", "2022-01-01T00:05:00Z", TestName = "Unordered data points")]
    public void AverageOverPeriod_ThrowsForDiscontinuousData(string firstStart, string secondStart)
    {
        // Arrange
        var firstTime = DateTimeOffset.Parse(firstStart);
        var secondTime = DateTimeOffset.Parse(secondStart);
        var duration = TimeSpan.FromMinutes(5);
        var discontinuousData = new List<EmissionsData>()
        {
            new EmissionsData() {
                Time = firstTime,
                Rating = 50,
                Duration = duration
            },
            new EmissionsData() {
                Time = secondTime,
                Rating = 50,
                Duration = duration
            }
        };
        var startPeriod = new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var endPeriod = new DateTimeOffset(2022, 1, 1, 0, 15, 0, TimeSpan.Zero);
        var expectedErrorMessage = $"Previous point covered through {firstTime+duration}; Current point starts at {secondTime}.";
        
        // Act & Assert
        InvalidOperationException? ex = Assert.Throws<InvalidOperationException>(() => discontinuousData.AverageOverPeriod(startPeriod, endPeriod));
        var message = ex?.Message;
        StringAssert.Contains(expectedErrorMessage, message);
    }
}