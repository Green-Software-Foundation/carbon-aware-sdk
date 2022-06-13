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

    [TestCase("westus", "2021-11-17", "2021-11-20", ExpectedResult = 25)]
    [TestCase("eastus", "2021-11-17", "2021-12-20", ExpectedResult = 60)]
    [TestCase("westus", "2021-11-17", "2021-11-18", ExpectedResult = 20)]
    [TestCase("eastus", "2021-11-19", "2021-12-30", ExpectedResult = 0)]
    [TestCase("fakelocation", "2021-11-18", "2021-12-30", ExpectedResult = 0)]
    public async Task<double> Test_Emissions_Average_FakeData(string location, string startTime, string endTime)
    {
        this.CarbonIntensityDataSource.Setup(x => x.GetCarbonIntensityAsync(It.IsAny<IEnumerable<Location>>(), 
            It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .ReturnsAsync(TestData.GetFilteredEmissionDataList(location, startTime, endTime));
        
        var props = new Dictionary<string, object>() {
            { CarbonAwareConstants.Locations, new List<Location>() { new Location() { RegionName = location } }},
            { CarbonAwareConstants.Start, startTime },
            { CarbonAwareConstants.End, endTime }
        };
        return await this.Aggregator.CalcEmissionsAverageAsync(props);
    }

    [TestCase("westus", "2021-11-17", "2021-11-20", 20)]
    [TestCase("eastus", "2021-12-19", "2021-12-30", 20)]
    [TestCase("fake", "2021-12-19", "2021-12-30", 0)]
    public async Task Test_Emissions_Average_With_Plugin_Association(string location, string startTime, string endTime, int expected)
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging(); 
        serviceCollection.AddCarbonAwareEmissionServices(It.IsAny<IConfiguration>());
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var aggregator = serviceProvider.GetRequiredService<ICarbonAwareAggregator>();
        Assert.NotNull(aggregator);

        var props = new Dictionary<string, object>() {
            { CarbonAwareConstants.Locations, new List<Location>() { new Location() { RegionName = location } } },
            { CarbonAwareConstants.Start, startTime },
            { CarbonAwareConstants.End, endTime}
        };

        var average = await aggregator.CalcEmissionsAverageAsync(props);
        Assert.GreaterOrEqual(average, expected);
    }

    [TestCase(null, null, null)]
    [TestCase("westus", null, null)]
    [TestCase(null, "2021-12-19", null)]
    [TestCase(null, null, "2021-12-20")]
    [TestCase(null, "2021-12-19", "2021-12-20")]
    public void Test_Emissions_Average_Missing_Properties(string location, string startTime, string endTime)
    {
        this.CarbonIntensityDataSource.Setup(x => x.GetCarbonIntensityAsync(It.IsAny<IEnumerable<Location>>(), 
            It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .ReturnsAsync(It.IsAny<IEnumerable<EmissionsData>>);
        
        var props = new Dictionary<string, object>();
        if (!String.IsNullOrEmpty(location))
        {
            props[CarbonAwareConstants.Locations] =  new List<Location>() { new Location() { RegionName = location } };
        }
        if (!String.IsNullOrEmpty(startTime))
        {
            props[CarbonAwareConstants.Start] = startTime;
        }
        if (!String.IsNullOrEmpty(endTime))
        {
            props[CarbonAwareConstants.End] = endTime;
        }
        Assert.ThrowsAsync<ArgumentException>(async () => await this.Aggregator.CalcEmissionsAverageAsync(props));
    }

    [Test]
    public async Task TestGetCurrentForecastDataAsync_SetsDefaults()
    {
        this.CarbonIntensityDataSource.Setup(x => x.GetCurrentCarbonIntensityForecastAsync(It.IsAny<Location>()))
            .ReturnsAsync(new EmissionsForecast());
        
        var props = new Dictionary<string, object>()
        {
            { CarbonAwareConstants.Locations, new List<Location>() { new Location() { RegionName = "westus" } } }
        };
        var results = await this.Aggregator.GetCurrentForecastDataAsync(props);
        var forecast = results.First();
        Assert.IsInstanceOf<DateTimeOffset>(forecast.StartTime);
        Assert.IsInstanceOf<DateTimeOffset>(forecast.EndTime);
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

    [TestCase("2021-12-31T23:00:00Z", "2022-01-01T01:00:00Z", ExpectedResult = 4)] // Full data set
    [TestCase("2022-01-01T00:00:00Z", "2022-01-01T01:00:00Z", ExpectedResult = 4)] // Start time exact match
    [TestCase("2021-12-31T23:00:00Z", "2022-01-01T00:15:00Z", ExpectedResult = 4)] // End time exact match
    [TestCase("2022-01-01T00:02:00Z", "2022-01-01T00:12:00Z", ExpectedResult = 3)] // Start and end midway through a datapoint
    public async Task<int> TestGetCurrentForecastDataAsync_FiltersDate(string start, string end)
    {
        this.CarbonIntensityDataSource.Setup(x => x.GetCurrentCarbonIntensityForecastAsync(It.IsAny<Location>()))
            .ReturnsAsync(TestData.GetForecast());
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

    [TestCase("2022-01-01T00:00:00Z", "2022-01-01T01:00:00Z")] // full data set
    [TestCase("2022-01-01T00:05:00Z", "2022-01-01T01:00:00Z")] // data set minus first lowest datapoint
    public async Task TestGetCurrentForecastDataAsync_OptimalDataPoint(string start, string end)
    {
        this.CarbonIntensityDataSource.Setup(x => x.GetCurrentCarbonIntensityForecastAsync(It.IsAny<Location>()))
            .ReturnsAsync(TestData.GetForecast());
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
            .ReturnsAsync(TestData.GetForecast());
        var locationName = "westus";
        var props = new Dictionary<string, object>()
        {
            { CarbonAwareConstants.Locations, new List<Location>() { new Location() { RegionName = "westus" } } },
            { CarbonAwareConstants.Start, "2022-01-01T00:00:00Z" },
            { CarbonAwareConstants.End, "2022-01-01T01:00:00Z" },
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
            .ReturnsAsync(TestData.GetForecast());
        var locationName = "westus";
        var props = new Dictionary<string, object>()
        {
            { CarbonAwareConstants.Locations, new List<Location>() { new Location() { RegionName = "westus" } } },
            { CarbonAwareConstants.Start, "2022-01-01T00:00:00Z" },
            { CarbonAwareConstants.End, "2022-01-01T01:00:00Z" },
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
}
