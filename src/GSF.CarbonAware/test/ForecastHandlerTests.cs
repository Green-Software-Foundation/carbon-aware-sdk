using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.Tools.WattTimeClient;
using GSF.CarbonAware.Exceptions;
using GSF.CarbonAware.Handlers;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using static CarbonAware.Aggregators.CarbonAware.CarbonAwareParameters;
using CarbonAware.Aggregators.Forecast;

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

    [TestCase("Sydney", "eastus", "2022-03-07T01:00:00", "2022-03-07T03:30:00", 5, TestName = "GetCurrentAsync calls aggregator: all fields")]
    [TestCase("Sydney", null, null, null, null, TestName = "GetCurrentAsync calls aggregator: required fields only")]
    public async Task GetCurrentAsync_Succeed_Call_MockAggregator_WithOutputData(string location1, string? location2, DateTimeOffset? start, DateTimeOffset? end, int? duration)
    {
        var data = new List<global::CarbonAware.Model.EmissionsForecast> {
            new global::CarbonAware.Model.EmissionsForecast {
                RequestedAt = DateTimeOffset.Now,
                GeneratedAt = DateTimeOffset.Now - TimeSpan.FromMinutes(1),
                ForecastData = Array.Empty<global::CarbonAware.Model.EmissionsData>(),
                OptimalDataPoints = Array.Empty<global::CarbonAware.Model.EmissionsData>(),
            }
        };

        var aggregator = SetupMockAggregator(data);
        var handler = new ForecastHandler(Logger!.Object, aggregator.Object);
        var locations = location2 != null ? new string[] { location1, location2 } : new string[] { location1 };
        var result = await handler.GetCurrentForecastAsync(locations, start, end, duration);
        Assert.That(result, Is.Not.Empty);
    }

    [Test]
    public async Task GetCurrentAsync_Succeed_Call_MockAggregator_WithoutOutputData()
    {
        var aggregator = SetupMockAggregator(Array.Empty<global::CarbonAware.Model.EmissionsForecast>().ToList());
        var handler = new ForecastHandler(Logger!.Object, aggregator.Object);
        var result = await handler.GetCurrentForecastAsync(new string[] { "eastus" }, DateTimeOffset.Now, DateTimeOffset.Now + TimeSpan.FromHours(1), 30);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetCurrentAsync_ThrowsException()
    {
        var aggregator = new Mock<IForecastAggregator>();
        aggregator
            .Setup(x => x.GetCurrentForecastDataAsync(It.IsAny<CarbonAwareParameters>()))
            .ThrowsAsync(new WattTimeClientException(""));
        var handler = new ForecastHandler(Logger!.Object, aggregator.Object);
        Assert.ThrowsAsync<CarbonAwareException>(async () => await handler.GetCurrentForecastAsync(It.IsAny<string[]>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<int>()));
    }

    private static Mock<IForecastAggregator> SetupMockAggregator(IEnumerable<global::CarbonAware.Model.EmissionsForecast> data)
    {
        var aggregator = new Mock<IForecastAggregator>();
        aggregator
            .Setup(x => x.GetCurrentForecastDataAsync(It.IsAny<CarbonAwareParameters>()))
            .Callback((CarbonAwareParameters parameters) =>
            {
                parameters.SetRequiredProperties(PropertyName.MultipleLocations);
                parameters.Validate();
            })
            .ReturnsAsync(data);

        return aggregator;
    }
}
