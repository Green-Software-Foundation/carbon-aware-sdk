using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CarbonAware.Aggregators.CarbonAware;
using GSF.CarbonIntensity.Exceptions;
using GSF.CarbonIntensity.Handlers;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using static CarbonAware.Aggregators.CarbonAware.CarbonAwareParameters;

namespace GSF.CarbonIntensity.Tests;

[TestFixture]
public class ForecastHandlerTests
{
    #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private Mock<ILogger<ForecastHandler>> Logger { get; set; }
    #pragma warning restore CS8618

    [SetUp]
    public void SetUp()
    {
        Logger = new Mock<ILogger<ForecastHandler>>();
    }

    [Test]
    public async Task GetCurrentAsync_Succeed_Call_MockAggregator_WithOuputData()
    {
        var data = new List<CarbonAware.Model.EmissionsForecast> {
            new CarbonAware.Model.EmissionsForecast {
                RequestedAt = DateTimeOffset.Now,
                GeneratedAt = DateTimeOffset.Now - TimeSpan.FromMinutes(1),
                ForecastData = Array.Empty<CarbonAware.Model.EmissionsData>(),
                OptimalDataPoints = Array.Empty<CarbonAware.Model.EmissionsData>(),
            }
        };

        var aggregator = SetupMockAggregator(data);
        var handler = new ForecastHandler(Logger.Object, aggregator.Object);
        var result = await handler.GetCurrentAsync("eastus", DateTimeOffset.Now, DateTimeOffset.Now + TimeSpan.FromHours(1), 30);
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task GetCurrentAsync_Succeed_Call_MockAggregator_WithoutOutputData()
    {
        var aggregator = SetupMockAggregator(Array.Empty<CarbonAware.Model.EmissionsForecast>().ToList());
        var handler = new ForecastHandler(Logger.Object, aggregator.Object);
        var result = await handler.GetCurrentAsync("eastus", DateTimeOffset.Now, DateTimeOffset.Now + TimeSpan.FromHours(1), 30);
        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetCurrentAsync_ThrowsException()
    {
        var aggregator = new Mock<ICarbonAwareAggregator>();
        aggregator
            .Setup(x => x.GetCurrentForecastDataAsync(It.IsAny<CarbonAwareParameters>()))
            .ThrowsAsync(new Exception());
        var handler = new ForecastHandler(Logger.Object, aggregator.Object);
        Assert.ThrowsAsync<CarbonIntensityException>(async () => await handler.GetCurrentAsync(It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<int>()));
    }

    private static Mock<ICarbonAwareAggregator> SetupMockAggregator(IEnumerable<CarbonAware.Model.EmissionsForecast> data)
    {
        var aggregator = new Mock<ICarbonAwareAggregator>();
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
