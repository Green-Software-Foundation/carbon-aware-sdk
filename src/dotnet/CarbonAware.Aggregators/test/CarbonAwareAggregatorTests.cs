using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.Model;
using Microsoft.Extensions.Logging;
using System.Linq;
using Moq;
using NUnit.Framework;
using Microsoft.Extensions.DependencyInjection;
using CarbonAware.Aggregators.Configuration;
using CarbonAware.Interfaces;
using Microsoft.Extensions.Configuration;

namespace CarbonAware.Aggregators.Tests;

[TestFixture]
public class CarbonAwareAggregatorTests
{

    [TestCase("westus", "2021-11-17", "2021-11-20", ExpectedResult = 25)]
    [TestCase("eastus", "2021-11-17", "2021-12-20", ExpectedResult = 60)]
    [TestCase("westus", "2021-11-17", "2021-11-18", ExpectedResult = 20)]
    [TestCase("eastus", "2021-11-19", "2021-12-30", ExpectedResult = 0)]
    [TestCase("fakelocation", "2021-11-18", "2021-12-30", ExpectedResult = 0)]
    public async Task<double> Test_Emissions_Average_FakeData(string location, string startTime, string endTime)
    {
        var logger = Mock.Of<ILogger<CarbonAwareAggregator>>();
        var mockDataSrc = new Mock<ICarbonIntensityDataSource>();

        mockDataSrc.Setup(x => x.GetCarbonIntensityAsync(It.IsAny<IEnumerable<Location>>(), 
            It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .ReturnsAsync(TestData.GetFilteredEmissionDataList(location, startTime, endTime));
        
        var aggregator = new CarbonAwareAggregator(logger, mockDataSrc.Object);
        var props = new Dictionary<string, object>() {
            { CarbonAwareConstants.Locations, new List<Location>() { new Location() { RegionName = location } }},
            { CarbonAwareConstants.Start, startTime },
            { CarbonAwareConstants.End, endTime }
        };
        return await aggregator.CalcEmissionsAverageAsync(props);
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
        var logger = Mock.Of<ILogger<CarbonAwareAggregator>>();
        var mockDataSrc = new Mock<ICarbonIntensityDataSource>();

        mockDataSrc.Setup(x => x.GetCarbonIntensityAsync(It.IsAny<IEnumerable<Location>>(), 
            It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .ReturnsAsync(It.IsAny<IEnumerable<EmissionsData>>);
        
        var aggregator = new CarbonAwareAggregator(logger, mockDataSrc.Object);
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
        Assert.ThrowsAsync<ArgumentException>(async () => await aggregator.CalcEmissionsAverageAsync(props));
    }
}
