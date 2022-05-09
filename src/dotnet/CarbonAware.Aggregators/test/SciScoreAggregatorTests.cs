using CarbonAware.Aggregators.SciScore;
using CarbonAware.Interfaces;
using CarbonAware.Model;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarbonAware.Aggregators.Tests;
// Test class sets these fields in [SetUp] rather than traditional class constructor.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

public class SciScoreAggregatorTests
{
    private Mock<ILogger<SciScoreAggregator>> Logger { get; set; }
    private Mock<ICarbonIntensityDataSource> CarbonIntensityDataSource { get; set; }
    private SciScoreAggregator Aggregator { get; set; }

    [SetUp]
    public void Setup()
    {
        this.Logger = new Mock<ILogger<SciScoreAggregator>>();
        this.CarbonIntensityDataSource = new Mock<ICarbonIntensityDataSource>();
        this.Aggregator = new SciScoreAggregator(this.Logger.Object, this.CarbonIntensityDataSource.Object);
    }

    [TestCase("westus", "2021-11-17T00:00:00Z", "2021-11-20T00:00:00Z", ExpectedResult = 25)]
    [TestCase("eastus", "2021-11-17T00:00:00Z", "2021-12-20T00:00:00Z", ExpectedResult = 60)]
    [TestCase("westus", "2021-11-17T00:00:00Z", "2021-11-18T00:00:00Z", ExpectedResult = 20)]
    [TestCase("eastus", "2021-11-19T00:00:00Z", "2021-12-30T00:00:00Z", ExpectedResult = 0)]
    [TestCase("fakelocation", "2021-11-18T00:00:00Z", "2021-12-30T00:00:00Z", ExpectedResult = 0)]
    public async Task<double> CalculateAverageCarbonIntensityAsync_ValidTimeInterval(string regionName, string startString, string endString)
    {
        // Arrange
        var location = new Location() { 
          LocationType = LocationType.CloudProvider,
          CloudProvider = CloudProvider.Azure,
          RegionName = regionName
        };

        List<Location> locations = new List<Location>() {
          location
        };

        var timeInterval = $"{startString}/{endString}";
        DateTimeOffset start, end;
        DateTimeOffset.TryParse(startString, out start);
        DateTimeOffset.TryParse(endString, out end);

        this.CarbonIntensityDataSource.Setup(x => x.GetCarbonIntensityAsync(It.IsAny<IEnumerable<Location>>(), 
            It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .ReturnsAsync(TestData.GetFilteredEmissionDataList(location.RegionName, startString, endString));

        // Act
        var result = await this.Aggregator.CalculateAverageCarbonIntensityAsync(location, timeInterval);

        // Assert
        this.CarbonIntensityDataSource.Verify(r => r.GetCarbonIntensityAsync(locations, start, end), Times.Once);
        return result;        
    }

    [Test]
    public void CalculateAverageCarbonIntensityAsync_UnderspecifiedTimeInterval()
    {
        // Arrange
        var location = new Location() { 
          LocationType = LocationType.Geoposition,
          Latitude = (decimal) 1.0,
          Longitude = (decimal) 2.0
        };

        List<Location> locations = new List<Location>() {
          location
        };

        var timeInterval = "2019-01-01/2019-01-02";
        var start = new DateTimeOffset(2019, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2019, 1, 2, 0, 0, 0, TimeSpan.Zero);

        // Act
        this.Aggregator.CalculateAverageCarbonIntensityAsync(location, timeInterval);

        // Assert
        this.CarbonIntensityDataSource.Verify(r => r.GetCarbonIntensityAsync(locations, start, end), Times.Once);
    }

    [Test]
    public void CalculateAverageCarbonIntensityAsync_InvalidTimeInterval()
    {
        // Arrange
        var location = new Location() { 
          LocationType = LocationType.Geoposition,
          Latitude = (decimal) 1.0,
          Longitude = (decimal) 2.0
        };

        List<Location> locations = new List<Location>() {
          location
        };

        var badStartDate = "not-a-date/2019-01-02";
        var badEndDate = "2019-01-01/not-a-date";
        var noSeparator = "2019-01-01 2019-01-02";
        var startAfterEnd = "2020-01-01/2019-12-31";

        // Act // Assert
        Assert.ThrowsAsync<ArgumentException>(async () => await this.Aggregator.CalculateAverageCarbonIntensityAsync(location, badStartDate));
        Assert.ThrowsAsync<ArgumentException>(async () => await this.Aggregator.CalculateAverageCarbonIntensityAsync(location, badEndDate));
        Assert.ThrowsAsync<ArgumentException>(async () => await this.Aggregator.CalculateAverageCarbonIntensityAsync(location, noSeparator));
        Assert.ThrowsAsync<ArgumentException>(async () => await this.Aggregator.CalculateAverageCarbonIntensityAsync(location, startAfterEnd));
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}